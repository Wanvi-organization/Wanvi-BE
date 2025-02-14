using Wanvi.ModelViews.AuthModelViews;
using Wanvi.ModelViews.UserModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IAuthService
    {
        Task VerifyOtp(ConfirmOTPModelView model, bool isResetPassword);
        Task ForgotPassword(EmailModelView model);
        Task ResetPassword(ResetPasswordModelView model);
        Task<AuthResponseModelView> CheckGoogle(CheckGoogleModel model);
        Task<AuthResponseModelView> LoginGoogle(TokenModelView model);
        Task<AuthResponseModelView> LoginFacebook(TokenModelView model);
        Task CreateRole(RoleModel model);
        Task Register(RegisterModel model);
        Task<LoginResponse> LoginAsync(LoginRequestModel request);
        Task<AuthResponseModelView> RefreshToken(RefreshTokenModel refreshTokenModel);
        Task<ResponsePhoneModel> CreateUserByPhone(CreateUseByPhoneModel model);
        Task<Guid> CheckPhone(CheckPhoneModel model);
        Task LogoutAsync(RefreshTokenModel model);
    }
}
