using Wanvi.ModelViews.AuthModelViews;
using Wanvi.ModelViews.UserModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IAuthService
    {
        Task VerifyOtp(ConfirmOTPModelView model, bool isResetPassword);
        Task ForgotPassword(EmailModelView model);
        Task ResetPassword(ResetPasswordModelView model);
        Task<LoginResponse> CheckGoogle(CheckGoogleModel model);
        Task<LoginResponse> LoginGoogle(TokenModelView model);
        Task<LoginResponse> LoginFacebook(TokenModelView model);
        Task CreateRole(RoleModel model);
        Task Register(RegisterModel model);
        Task<LoginResponse> LoginAsync(LoginRequestModel request);
        Task<LoginResponse> RefreshToken(RefreshTokenModel refreshTokenModel);
        Task<ResponsePhoneModel> CreateUserByPhone(CreateUseByPhoneModel model);
        Task<Guid> CheckPhone(CheckPhoneModel model);
        Task LogoutAsync(RefreshTokenModel model);
    }
}
