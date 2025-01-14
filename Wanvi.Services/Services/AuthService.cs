using AutoMapper;
using Google.Apis.Auth;
using MailKit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.Core.Utils;
using Wanvi.ModelViews.AuthModelViews;
using Wanvi.Services.Services.Infrastructure;
using static Wanvi.Core.Bases.CoreException;

namespace Wanvi.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _jwtSettings;

        public AuthService(UserManager<ApplicationUser> userManager, IEmailService emailService, IConfiguration configuration, IUnitOfWork unitOfWork,ITokenService tokenService, JwtSettings jwtSettings)
        {
            _userManager = userManager;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _tokenService = tokenService;
            _jwtSettings = jwtSettings;
        }

        #region Private Service
        private (string token, IEnumerable<string> roles) GenerateJwtToken(ApplicationUser user)
        {
            byte[] key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"] ?? throw new Exception("JWT_KEY is not set"));
            List<Claim> claims = new List<Claim> {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };
            IEnumerable<string> roles = _userManager.GetRolesAsync(user).Result;
            foreach (string role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            string issuer = _configuration["JwtSettings:Issuer"] ?? throw new Exception("JWT_ISSUER is not set");
            string audience = _configuration["JwtSettings:Audience"] ?? throw new Exception("JWT_AUDIENCE is not set");

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return (tokenHandler.WriteToken(token), roles);
        }

        private async Task<string> GenerateRefreshToken(ApplicationUser user)
        {
            string? refreshToken = Guid.NewGuid().ToString();

            string? initToken = await _userManager.GetAuthenticationTokenAsync(user, "Default", "RefreshToken");
            if (initToken != null)
            {

                await _userManager.RemoveAuthenticationTokenAsync(user, "Default", "RefreshToken");

            }

            await _userManager.SetAuthenticationTokenAsync(user, "Default", "RefreshToken", refreshToken);
            return refreshToken;
        }

        private string GenerateOtp()
        {
            Random random = new Random();
            string otp = random.Next(100000, 999999).ToString();
            return otp;
        }
        #endregion

        #region Implementation Interface
        public async Task ForgotPassword(EmailModelView model)
        {
            ApplicationUser? user = await _userManager.FindByEmailAsync(model.Email)
                ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Vui lòng kiểm tra email của bạn");

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Vui lòng kiểm tra email của bạn");
            }

            string OTP = GenerateOtp();
            user.EmailCode = int.Parse(OTP);
            user.CodeGeneratedTime = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không thể lưu OTP, vui lòng thử lại sau.");
            }

            await _emailService.SendEmailAsync(model.Email, "Đặt lại mật khẩu", $"Vui lòng xác nhận tài khoản của bạn, OTP của bạn là: <div class='otp'>{OTP}</div>");
        }

        public async Task VerifyOtp(ConfirmOTPModelView model, bool isResetPassword)
        {
            ApplicationUser? user = await _userManager.FindByEmailAsync(model.Email)
                ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Vui lòng kiểm tra email của bạn");

            if (user.EmailCode == null || user.EmailCode.ToString() != model.OTP)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "OTP không hợp lệ");
            }

            if (!user.CodeGeneratedTime.HasValue || DateTime.UtcNow > user.CodeGeneratedTime.Value.AddMinutes(5))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "OTP đã hết hạn");
            }

            if (!isResetPassword)
            {
                string? token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _userManager.ConfirmEmailAsync(user, token);
            }

            user.EmailCode = null;
            user.CodeGeneratedTime = null;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không thể cập nhật thông tin người dùng");
            }
        }

        public async Task ResetPassword(ResetPasswordModelView model)
        {
            ApplicationUser? user = await _userManager.FindByEmailAsync(model.Email)
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Không tìm thấy user");

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Vui lòng kiểm tra email của bạn");
            }

            string? token = await _userManager.GeneratePasswordResetTokenAsync(user);
            IdentityResult? result = await _userManager.ResetPasswordAsync(user, token, model.Password);

            if (!result.Succeeded)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, result.Errors.FirstOrDefault()?.Description);
            }
        }

        public async Task Register(RegisterModel model)
        {
            // Kiểm tra email đã tồn tại
            var applicationUser = await _unitOfWork.GetRepository<ApplicationUser>().Entities
                .FirstOrDefaultAsync(x => x.Email.ToLower().Equals(model.Email.ToLower()) && !x.DeletedTime.HasValue);

            if (applicationUser != null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Email đã được sử dụng, vui lòng nhập email khác!");
            }

            // Kiểm tra số điện thoại đã tồn tại
            var userWithPhone = await _unitOfWork.GetRepository<ApplicationUser>().Entities
                .FirstOrDefaultAsync(x => x.PhoneNumber == model.Phone && !x.DeletedTime.HasValue);

            if (userWithPhone != null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Số điện thoại đã được sử dụng!");
            }

            // Kiểm tra xác nhận mật khẩu
            if (model.Password != model.ConfirmPassword)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Xác nhận mật khẩu không đúng!");
            }

            // Xác định vai trò
            var roleName = model.RoleName ? "Traveler" : "LocalGuide";
            var applicationRole = await _unitOfWork.GetRepository<ApplicationRole>().Entities
                .FirstOrDefaultAsync(x => x.Name == roleName);

            if (applicationRole == null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Vai trò không tồn tại!");
            }

            // Sử dụng PasswordHasher để băm mật khẩu
            var passwordHasher = new FixedSaltPasswordHasher<ApplicationUser>(Options.Create(new PasswordHasherOptions()));
            var newUser = new ApplicationUser
            {
                FullName = model.Name,
                Email = model.Email,
                UserName = model.Email,
                PhoneNumber = model.Phone,
                Address = model.PlaceOfBirth,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                NormalizedEmail = model.Email.ToUpper(),
                NormalizedUserName = model.Email.ToUpper(),
                SecurityStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, model.Password), // Băm mật khẩu tại đây
            };

            // Lưu người dùng mới vào cơ sở dữ liệu
            _unitOfWork.GetRepository<ApplicationUser>().Insert(newUser);

            ApplicationUserRole applicationUserRole = new ApplicationUserRole()
            {
                UserId = newUser.Id,
                RoleId = applicationRole.Id,
            };
            string OTP = GenerateOtp();
            newUser.EmailCode = int.Parse(OTP);
            newUser.CodeGeneratedTime = DateTime.UtcNow;

            await _emailService.SendEmailAsync(model.Email, "Xác nhận tài khoản", $"Vui lòng xác nhận tài khoản của bạn, OTP của bạn là: <div class='otp'>{OTP}</div>");
            await _unitOfWork.GetRepository<ApplicationUserRole>().InsertAsync(applicationUserRole);

            await _unitOfWork.SaveAsync();

        }

        public async Task CreateRole(RoleModel model)
        {
            ApplicationRole applicationRole = await _unitOfWork.GetRepository<ApplicationRole>().Entities.FirstOrDefaultAsync(x => x.Name == model.RoleName);
            if (applicationRole != null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Vai trò đã tồn tại!");
            }
            ApplicationRole role = new ApplicationRole();
            role.Name = model.RoleName;
            await _unitOfWork.GetRepository<ApplicationRole>().InsertAsync(role);
            await _unitOfWork.SaveAsync();
        }

        public async Task<LoginResponse> LoginAsync(LoginRequestModel request)
        {
            var user = _unitOfWork.GetRepository<ApplicationUser>().Entities
                .Where(u => !u.DeletedTime.HasValue && u.UserName == request.Username)
                .FirstOrDefault()
                ?? throw new ErrorException(StatusCodes.Status401Unauthorized, ResponseCodeConstants.BADREQUEST, "Không tìm thấy tài khoản");
            if(user.EmailConfirmed == false)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Tài khoản chưa được xác thực!");
            }
            // create hash
            var passwordHasher = new FixedSaltPasswordHasher<ApplicationUser>(Options.Create(new PasswordHasherOptions()));

            var hashedInputPassword = passwordHasher.HashPassword(null, request.Password);

            if (hashedInputPassword != user.PasswordHash)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy tài khoản");
            }

            //// Get the user's role
            //var roles = await _userManager.GetRolesAsync(user);
            //var role = roles.FirstOrDefault(); // Assuming a single role for simplicity
            ApplicationUserRole roleUser = _unitOfWork.GetRepository<ApplicationUserRole>().Entities.Where(x => x.UserId == user.Id).FirstOrDefault()
                                ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không tìm thấy tài khoản");
            string roleName = _unitOfWork.GetRepository<ApplicationRole>().Entities.Where(x => x.Id == roleUser.RoleId).Select(x => x.Name).FirstOrDefault()
             ?? "unknow";
            var tokenResponse = _tokenService.GenerateTokens(user, roleName);
            var token = Authentication.CreateToken(user.Id.ToString(), _jwtSettings);
            var loginResponse = new LoginResponse
            {
                TokenResponse = tokenResponse,
                Role = roleName,
                token = token,
            };
            return loginResponse;

        }

        public async Task<AuthResponseModelView> RefreshToken(RefreshTokenModel refreshTokenModel)
        {
            ApplicationUser? user = await CheckRefreshToken(refreshTokenModel.refreshToken);
            (string token, IEnumerable<string> roles) = GenerateJwtToken(user);
            string refreshToken = await GenerateRefreshToken(user);
            return new AuthResponseModelView
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                TokenType = "JWT",
                AuthType = "Bearer",
                ExpiresIn = DateTime.UtcNow.AddHours(1),
                User = new UserInfo
                {
                    Email = user.Email,
                    Roles = roles.ToList()
                }
            };
        }

        private async Task<ApplicationUser> CheckRefreshToken(string refreshToken)
        {

            List<ApplicationUser>? users = await _userManager.Users.ToListAsync();
            foreach (ApplicationUser user in users)
            {
                string? storedToken = await _userManager.GetAuthenticationTokenAsync(user, "Default", "RefreshToken");

                if (storedToken == refreshToken)
                {
                    return user;
                }
            }
            throw new ErrorException(StatusCodes.Status401Unauthorized, ErrorCode.Unauthorized, "Token không hợp lệ");
        }

        public async Task<AuthResponseModelView> LoginGoogle(TokenModelView model)
        {
            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(model.Token);
            string email = payload.Email;
            string providerKey = payload.Subject;
            ApplicationUser? user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tài khoản chưa được tạo. Vui lòng tạo tài khoản trước khi đăng nhập.");
            }

            if (user.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tài khoản đã bị xóa");
            }

            (string token, IEnumerable<string> roles) = GenerateJwtToken(user);
            string refreshToken = await GenerateRefreshToken(user);

            return new AuthResponseModelView
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                TokenType = "JWT",
                AuthType = "Bearer",
                ExpiresIn = DateTime.UtcNow.AddHours(1),
                User = new UserInfo
                {
                    Email = user.Email,
                    Roles = roles.ToList()
                }
            };
        }

        public async Task<AuthResponseModelView> LoginFacebook(TokenModelView model)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"https://graph.facebook.com/me?fields=id,email&access_token={model.Token}");

            if (!response.IsSuccessStatusCode)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Token không hợp lệ hoặc đã hết hạn.");
            }

            var content = await response.Content.ReadAsStringAsync();
            var facebookData = JsonConvert.DeserializeObject<FacebookPayloadModelView>(content);

            if (facebookData == null || string.IsNullOrEmpty(facebookData.Email))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Không thể lấy thông tin người dùng từ Facebook.");
            }

            string email = facebookData.Email;
            string providerKey = facebookData.Id;

            ApplicationUser? user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tài khoản chưa được tạo. Vui lòng tạo tài khoản trước khi đăng nhập.");
            }

            if (user.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tài khoản đã bị xóa");
            }

            (string token, IEnumerable<string> roles) = GenerateJwtToken(user);
            string refreshToken = await GenerateRefreshToken(user);

            return new AuthResponseModelView
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                TokenType = "JWT",
                AuthType = "Bearer",
                ExpiresIn = DateTime.UtcNow.AddHours(1),
                User = new UserInfo
                {
                    Email = user.Email,
                    Roles = roles.ToList()
                }
            };
        }

        #endregion
    }
}
