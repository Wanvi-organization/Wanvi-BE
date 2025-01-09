using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.ModelViews.AuthModelViews;

namespace WanviBE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPatch("Confirm_OTP_Email_Verification")]
        public async Task<IActionResult> ConfirmOTPEmailVerification(ConfirmOTPModel model)
        {
            await _authService.VerifyOtp(model, false);
            return Ok(BaseResponse<string>.OkResponse("Xác nhận email thành công!"));
        }

        [HttpPost("Forgot_Password")]
        public async Task<IActionResult> ForgotPassword(EmailModelView model)
        {
            await _authService.ForgotPassword(model);
            return Ok(BaseResponse<string>.OkResponse("Đã gửi email xác nhận yêu cầu thay đổi mật khẩu."));
        }

        [HttpPatch("Confirm_OTP_Reset_Password")]
        public async Task<IActionResult> ConfirmOTPResetPassword(ConfirmOTPModel model)
        {
            await _authService.VerifyOtp(model, true);
            return Ok(BaseResponse<string>.OkResponse("Xác nhận thay đổi mật khẩu thành công!"));
        }

        [HttpPatch("Reset_Password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            await _authService.ResetPassword(model);
            return Ok(BaseResponse<string>.OkResponse("Đã đặt lại mật khẩu thành công!"));
        }


    }
}
