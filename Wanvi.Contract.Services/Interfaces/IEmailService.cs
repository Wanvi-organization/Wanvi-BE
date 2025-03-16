using Wanvi.ModelViews.EmailModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string body);
        Task StaffSendEmailAsync(SendEmailRequestModel model);
    }
}
