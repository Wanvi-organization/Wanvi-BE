using Wanvi.ModelViews.AuthModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IAuthService
    {
        Task VerifyOtp(ConfirmOTPModelView model, bool isResetPassword);
        Task ForgotPassword(EmailModelView model);
        Task ResetPassword(ResetPasswordModelView model);
        Task<AuthResponseModelView> LoginGoogle(TokenModelView model);
        Task<AuthResponseModelView> LoginFacebook(TokenModelView model);
        Task CreateRole(RoleModel model);
        Task Register(string id, RegisterModel model);
        Task<LoginResponse> LoginAsync(LoginRequestModel request);
        Task<AuthResponseModelView> RefreshToken(RefreshTokenModel refreshTokenModel);
    }
}
