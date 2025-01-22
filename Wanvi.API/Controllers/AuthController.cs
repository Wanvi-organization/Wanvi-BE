using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.AuthModelViews;
using Wanvi.ModelViews.UserModelViews;

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

        [HttpPost("Create_Role")]
        public async Task<IActionResult> CreateRole(RoleModel model)
        {
            await _authService.CreateRole(model);
            return Ok(BaseResponse<string>.OkResponse("Tạo vai trò thành công!"));
        }

        [HttpPost("Register_User")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            await _authService.Register(model);
            return Ok(BaseResponse<string>.OkResponse("Đăng kí thành công!"));
        }

        [HttpPatch("Confirm_OTP_Email_Verification")]
        public async Task<IActionResult> ConfirmOTPEmailVerification(ConfirmOTPModelView model)
        {
            await _authService.VerifyOtp(model, false);
            return Ok(BaseResponse<string>.OkResponse("Xác nhận email thành công!"));
        }
        
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequestModel request)
        {
            LoginResponse res = await _authService.LoginAsync(request);
            return Ok(new BaseResponseModel<LoginResponse>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }
        [HttpPost("Create_User_By_Phone")]
        public async Task<IActionResult> CreateUsrByPhone(CreateUseByPhoneModel model)
        {
             var res = await _authService.CreateUserByPhone(model);
            return Ok(new BaseResponseModel<ResponsePhoneModel>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }
        [HttpPost("Check_Phone")]
        public async Task<IActionResult> CheckPhone(CheckPhoneModel model)
        {
            var res = await _authService.CheckPhone(model);
            return Ok(new BaseResponseModel<Guid>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }

        [HttpPost("Login_Google")]
        public async Task<IActionResult> LoginGoogle(TokenModelView model)
        {
            AuthResponseModelView? result = await _authService.LoginGoogle(model);
            return Ok(BaseResponse<AuthResponseModelView>.OkResponse(result));
        }

        [HttpPost("Login_Facebook")]
        public async Task<IActionResult> LoginFacebook(TokenModelView model)
        {
            AuthResponseModelView? result = await _authService.LoginFacebook(model);
            return Ok(BaseResponse<AuthResponseModelView>.OkResponse(result));
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken(RefreshTokenModel model)
        {
            AuthResponseModelView? res = await _authService.RefreshToken(model);
            return Ok(new BaseResponseModel<AuthResponseModelView>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }

        [HttpPost("Forgot_Password")]
        public async Task<IActionResult> ForgotPassword(EmailModelView model)
        {
            await _authService.ForgotPassword(model);
            return Ok(BaseResponse<string>.OkResponse("Đã gửi email xác nhận yêu cầu thay đổi mật khẩu."));
        }

        [HttpPatch("Confirm_OTP_Reset_Password")]
        public async Task<IActionResult> ConfirmOTPResetPassword(ConfirmOTPModelView model)
        {
            await _authService.VerifyOtp(model, true);
            return Ok(BaseResponse<string>.OkResponse("Xác nhận thay đổi mật khẩu thành công!"));
        }

        [HttpPatch("Reset_Password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordModelView model)
        {
            await _authService.ResetPassword(model);
            return Ok(BaseResponse<string>.OkResponse("Đã đặt lại mật khẩu thành công!"));
        }
    }
}
