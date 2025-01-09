using Wanvi.ModelViews.AuthModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IAuthService
    {
        Task VerifyOtp(ConfirmOTPModel model, bool isResetPassword);
        Task ForgotPassword(EmailModelView emailModelView);
        Task ResetPassword(ResetPasswordModel resetPassword);
    }
}
