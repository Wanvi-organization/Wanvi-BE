using Wanvi.ModelViews.AuthModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IAuthService
    {
        Task VerifyOtp(ConfirmOTPModelView model, bool isResetPassword);
        Task ForgotPassword(EmailModelView model);
        Task ResetPassword(ResetPasswordModelView model);
        //Task<AuthResponseModelView> LoginGoogle(TokenGoogleModelView model);
    }
}
