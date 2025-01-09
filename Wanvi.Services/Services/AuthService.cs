using MailKit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _memoryCache;
        private readonly IEmailService _emailService;

        public AuthService(UserManager<ApplicationUser> userManager, IMemoryCache memoryCache, IEmailService emailService)
        {
            _userManager = userManager;
            _memoryCache = memoryCache;
            _emailService = emailService;
        }

        #region Private Service
        private string GenerateOtp()
        {
            Random random = new Random();
            string otp = random.Next(100000, 999999).ToString();
            return otp;
        }
        #endregion

        #region Implementation Interface
        public async Task ForgotPassword(EmailModelView emailModelView)
        {
            ApplicationUser? user = await _userManager.FindByEmailAsync(emailModelView.Email)
                ?? throw new BaseException.ErrorException(StatusCode.BadRequest, ErrorCode.BadRequest, "Vui lòng kiểm tra email của bạn");

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                throw new BaseException.ErrorException(StatusCode.BadRequest, ErrorCode.BadRequest, "Vui lòng kiểm tra email của bạn");
            }

            string OTP = GenerateOtp();
            string otpCacheKey = $"OTPResetPassword_{emailModelView.Email}";
            _memoryCache.Set(otpCacheKey, OTP, TimeSpan.FromMinutes(1));

            string emailCacheKey = "EmailForResetPassword";
            _memoryCache.Set(emailCacheKey, emailModelView.Email, TimeSpan.FromMinutes(10));

            await _emailService.SendEmailAsync(emailModelView.Email, "Đặt lại mật khẩu", $"Vui lòng xác nhận tài khoản của bạn, OTP của bạn là: <div class='otp'>{OTP}</div>");
        }

        public async Task VerifyOtp(ConfirmOTPModel model, bool isResetPassword)
        {
            string emailCacheKey = "EmailForResetPassword";
            if (!_memoryCache.TryGetValue(emailCacheKey, out string email))
            {
                throw new BaseException.ErrorException(StatusCode.BadRequest, ErrorCode.BadRequest, "Email không hợp lệ hoặc đã hết hạn");
            }

            string otpCacheKey = isResetPassword ? $"OTPResetPassword_{email}" : $"OTP_{email}";

            if (_memoryCache.TryGetValue(otpCacheKey, out string storedOtp))
            {
                if (storedOtp == model.OTP)
                {
                    ApplicationUser? user = await _userManager.FindByEmailAsync(email)
                        ?? throw new BaseException.ErrorException(StatusCode.NotFound, ErrorCode.NotFound, "Không tìm thấy user");

                    string? token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _userManager.ConfirmEmailAsync(user, token);

                    _memoryCache.Remove(otpCacheKey);
                }
                else
                {
                    throw new BaseException.ErrorException(StatusCode.BadRequest, ErrorCode.BadRequest, "OTP không hợp lệ");
                }
            }
            else
            {
                throw new BaseException.ErrorException(StatusCode.BadRequest, ErrorCode.BadRequest, "OTP không hợp lệ hoặc đã hết hạn");
            }
        }

        public async Task ResetPassword(ResetPasswordModel resetPasswordModel)
        {
            string emailCacheKey = "EmailForResetPassword";

            if (!_memoryCache.TryGetValue(emailCacheKey, out string email))
            {
                throw new BaseException.ErrorException(StatusCode.BadRequest, ErrorCode.BadRequest, "Email không hợp lệ hoặc đã hết hạn");
            }

            ApplicationUser? user = await _userManager.FindByEmailAsync(email)
                ?? throw new BaseException.ErrorException(StatusCode.NotFound, ErrorCode.NotFound, "Không tìm thấy user");

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                throw new BaseException.ErrorException(StatusCode.BadRequest, ErrorCode.BadRequest, "Vui lòng kiểm tra email của bạn");
            }

            string? token = await _userManager.GeneratePasswordResetTokenAsync(user);
            IdentityResult? result = await _userManager.ResetPasswordAsync(user, token, resetPasswordModel.Password);

            if (!result.Succeeded)
            {
                throw new BaseException.ErrorException(StatusCode.BadRequest, ErrorCode.BadRequest, result.Errors.FirstOrDefault()?.Description);
            }

            _memoryCache.Remove(emailCacheKey);
        }

        #endregion
    }
}
