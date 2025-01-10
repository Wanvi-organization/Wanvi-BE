using AutoMapper;
using Google.Apis.Auth;
using MailKit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.AuthModelViews;

namespace Wanvi.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<ApplicationUser> userManager, IEmailService emailService, IConfiguration configuration)
        {
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
        }

        #region Private Service
        private (string token, IEnumerable<string> roles) GenerateJwtToken(ApplicationUser user)
        {
            byte[] key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Key"] ?? throw new Exception("JWT_KEY is not set"));
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
                ?? throw new BaseException.ErrorException(StatusCode.BadRequest, ErrorCode.BadRequest, "Vui lòng kiểm tra email của bạn");

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                throw new BaseException.ErrorException(StatusCode.BadRequest, ErrorCode.BadRequest, "Vui lòng kiểm tra email của bạn");
            }

            string OTP = GenerateOtp();
            user.EmailCode = int.Parse(OTP);
            user.CodeGeneratedTime = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new BaseException.ErrorException(StatusCode.BadRequest, ErrorCode.BadRequest, "Không thể lưu OTP, vui lòng thử lại sau.");
            }

            await _emailService.SendEmailAsync(model.Email, "Đặt lại mật khẩu", $"Vui lòng xác nhận tài khoản của bạn, OTP của bạn là: <div class='otp'>{OTP}</div>");
        }

        public async Task VerifyOtp(ConfirmOTPModelView model, bool isResetPassword)
        {
            ApplicationUser? user = await _userManager.FindByEmailAsync(model.Email)
                ?? throw new BaseException.ErrorException(StatusCode.BadRequest, ErrorCode.BadRequest, "Vui lòng kiểm tra email của bạn");

            if (user.EmailCode == null || user.EmailCode.ToString() != model.OTP)
            {
                throw new BaseException.ErrorException(StatusCode.BadRequest, ErrorCode.BadRequest, "OTP không hợp lệ");
            }

            if (!user.CodeGeneratedTime.HasValue || DateTime.UtcNow > user.CodeGeneratedTime.Value.AddMinutes(5))
            {
                throw new BaseException.ErrorException(StatusCode.BadRequest, ErrorCode.BadRequest, "OTP đã hết hạn");
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
                throw new BaseException.ErrorException(StatusCode.BadRequest, ErrorCode.BadRequest, "Không thể cập nhật thông tin người dùng");
            }
        }

        public async Task ResetPassword(ResetPasswordModelView model)
        {
            ApplicationUser? user = await _userManager.FindByEmailAsync(model.Email)
                ?? throw new BaseException.ErrorException(StatusCode.NotFound, ErrorCode.NotFound, "Không tìm thấy user");

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                throw new BaseException.ErrorException(StatusCode.BadRequest, ErrorCode.BadRequest, "Vui lòng kiểm tra email của bạn");
            }

            string? token = await _userManager.GeneratePasswordResetTokenAsync(user);
            IdentityResult? result = await _userManager.ResetPasswordAsync(user, token, model.Password);

            if (!result.Succeeded)
            {
                throw new BaseException.ErrorException(StatusCode.BadRequest, ErrorCode.BadRequest, result.Errors.FirstOrDefault()?.Description);
            }
        }

        //public async Task<AuthResponseModelView> LoginGoogle(TokenGoogleModelView model)
        //{
        //    GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(model.Token);
        //    string email = payload.Email;
        //    string providerKey = payload.Subject;
        //    ApplicationUser? user = await _userManager.FindByEmailAsync(email);

        //    if (user == null)
        //    {
        //        throw new BaseException.ErrorException(StatusCode.BadRequest, ErrorCode.BadRequest, "Tài khoản chưa được tạo. Vui lòng tạo tài khoản trước khi đăng nhập.");
        //    }

        //    if (user.DeletedTime.HasValue)
        //    {
        //        throw new BaseException.ErrorException(StatusCode.BadRequest, ErrorCode.BadRequest, "Tài khoản đã bị xóa");
        //    }

        //    (string token, IEnumerable<string> roles) = GenerateJwtToken(user);
        //    string refreshToken = await GenerateRefreshToken(user);

        //    return new AuthResponseModelView
        //    {
        //        AccessToken = token,
        //        RefreshToken = refreshToken,
        //        TokenType = "JWT",
        //        AuthType = "Bearer",
        //        ExpiresIn = DateTime.UtcNow.AddHours(1),
        //        User = new UserInfo
        //        {
        //            Email = user.Email,
        //            Roles = roles.ToList()
        //        }
        //    };
        //}
        #endregion
    }
}
