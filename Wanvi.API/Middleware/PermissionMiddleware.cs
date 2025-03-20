﻿using System.Net;
using System.Text.Json;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.Services.Services.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;

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
            _excludedUris = new List<string>
            {
                "/api/auth/create_role",
                "/api/auth/login",
                "/api/auth/register_user",
                "/api/auth/confirm_otp_email_verification",
                "/api/auth/login_google",
                "/api/auth/login_facebook",
                "/api/auth/refreshtoken",
                "/api/auth/forgotpassword",
                "/api/auth/confirm_otp_reset_password",
                "/api/auth/reset_password",
                "/api/auth/create_user_by_phone",
                "/api/auth/check_phone",
                "/api/auth/forgot_password",
                "/api/user/get_local_guides",
                "/api/payment/payos_callback",
                "/api/booking/cancel_booking_for_admin",
                "/api/user/unlock_booking_of_tourguide"
            };
            _rolePermissions = new Dictionary<string, List<string>>()
            {
                // ... (your role permissions) ...
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
                await HandleForbiddenRequest(context);
            }
        }

        private bool HasPermission(HttpContext context, IUnitOfWork unitOfWork)
        {
            string requestUri = context.Request.Path.Value!;

            // Exclude specified URIs and non-API requests
            if (_excludedUris.Contains(requestUri) || !requestUri.StartsWith("/api/"))
            {
                return true;
            }

            string authorizationHeader = context.Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                throw new ErrorException(StatusCodes.Status401Unauthorized, ErrorCode.Unauthorized, "Bạn chưa được xác thực. Vui lòng đăng nhập!");
            }
            try
            {
                // Get user ID from the authenticated context
                string idUser = Authentication.GetUserIdFromHttpContext(context);

                if (Guid.TryParse(idUser, out Guid guidId))
                {
                    ApplicationUser? user = unitOfWork.GetRepository<ApplicationUser>().Entities
                        .FirstOrDefault(x => x.Id == guidId && !x.DeletedTime.HasValue);

                    if (user == null)
                    {
                        return false; // User not found
                    }


                    ////Check if the user is authenticated
                    if (!context.User.Identity.IsAuthenticated)
                    {
                        return false; // Not authenticated
                    }

                    // Check role-based permissions
                    //string userRole = Authentication.GetUserRoleFromHttpContext(context);
                    //if (userRole == "admin")
                    //{
                    //    return true; // Admin has all access
                    //}

                    //if (_rolePermissions.TryGetValue(userRole, out var allowedControllers))
                    //{
                    //    string[] segments = requestUri.Split('/');
                    //    string controller = segments.Length > 2 ? $"/api/{segments[2]}" : string.Empty;

                    //    return allowedControllers.Any(uri => requestUri.StartsWith(uri, System.StringComparison.OrdinalIgnoreCase));
                    //}
                    // else, no role found for current user, or user not in role dictionary.

                    return true; // authenticated user with valid ID, grant access.
                }

                return false; // Invalid user ID format
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permissions");
                return false; // Error occurred, deny access
            }
        }

        private static async Task HandleForbiddenRequest(HttpContext context)
        {
            int code = (int)HttpStatusCode.Forbidden;
            var error = new ErrorException(code, ResponseCodeConstants.FORBIDDEN, "Không tìm thấy tài khoản");
            string result = JsonSerializer.Serialize(error);

            context.Response.ContentType = "application/json";
            context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            context.Response.StatusCode = code;

            await context.Response.WriteAsync(result);
        }
    }
}