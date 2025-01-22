using System.Net;
using System.Text.Json;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.Services.Services.Infrastructure;

namespace Wanvi.API.Middleware
{
    public class PermissionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PermissionMiddleware> _logger;
        private readonly Dictionary<string, List<string>> _rolePermissions;
        private readonly IEnumerable<string> _excludedUris;

        public PermissionMiddleware(RequestDelegate next, ILogger<PermissionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _excludedUris =
            [
                "/api/auth/create_role",
                "/api/auth/login",
                "/api/auth/register_user",
                "/api/auth/confirm_otp_email_verification",
                "/api/auth/login-google",
                "/api/auth/login-facebook",
                "/api/auth/refreshtoken",
                "/api/auth/forgotpassword",
                "/api/auth/confirm_otp_reset_password",
                "/api/auth/reset_password",
                "/api/user/get_nearby_local_guides",
                "/api/auth/create-user-by-phone",
                "/api/auth/check-phone",
                "/api/auth/forgot_password"
            ];
            _rolePermissions = new Dictionary<string, List<string>>()
            {
                //author bang role, roleClaim userClaim
                { "QcManagement", new List<string> { "/api/dashboards"} },
                { "WarehouseManagement", new List<string> {"/api/WareHouse-Management", "/api/excelexport" } },
                { "LineManagement", new List<string> { "/api/dashboards"} }
            };

        }

        public async Task Invoke(HttpContext context, IUnitOfWork unitOfWork)
        {
            if (HasPermission(context, unitOfWork))
            {
                await _next(context);
            }
            else
            {
                await Authentication.HandleForbiddenRequest(context);
            }
        }

        private bool HasPermission(HttpContext context, IUnitOfWork unitOfWork)
        {
            bool isPersmission = false;
            string requestUri = context.Request.Path.Value!;

            if (_excludedUris.Contains(requestUri) || !requestUri.StartsWith("/api/"))
                return true;

            string[] segments = requestUri.Split('/');

            string featureUri = string.Join("/", segments.Take(segments.Length - 1));

            string controller = segments.Length > 2 ? $"/api/{segments[2]}" : string.Empty;

            try
            {
                string idUser = Authentication.GetUserIdFromHttpContext(context);
                if (Guid.TryParse(idUser, out Guid guidId))
                {
                    ApplicationUser? user = unitOfWork.GetRepository<ApplicationUser>().Entities.Where(x => x.Id == guidId & !x.DeletedTime.HasValue).FirstOrDefault();
                    if (user is null)
                    {
                        isPersmission = false;
                    }
                }

                //string userRole = Authentication.GetUserRoleFromHttpContext(context);

                ////// If the user role is admin, allow access to all controllers
                //if (userRole == "admin") return true;

                //// Check if the user's role has permission to access the requested controller
                //if (_rolePermissions.TryGetValue(userRole, out var allowedControllers))
                //{
                //    return allowedControllers.Any(uri => requestUri.StartsWith(uri, System.StringComparison.OrdinalIgnoreCase));
                //}
                //return false;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking permissions");
            }
            return isPersmission;
        }
        private static async Task HandleForbiddenRequest(HttpContext context)
        {
            int code = (int)HttpStatusCode.Forbidden;
            var a = new ErrorException(code, ResponseCodeConstants.FORBIDDEN, "Không tìm thấy tài khoản");
            string result = JsonSerializer.Serialize(a);
            //string result = JsonSerializer.Serialize(new { error = "You don't have permission to access this feature" });

            context.Response.ContentType = "application/json";
            //context.Response.Headers?.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers?.Append("Access-Control-Allow-Origin", "*");
            //context.Response.Headers!["Access-Control-Allow-Origin"] = "*";

            context.Response.StatusCode = code;

            await context.Response.WriteAsync(result);
        }
    }
}
