using Microsoft.Extensions.Options;
using Wanvi.Contract.Services.Interfaces;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.EmailModelViews;
using Wanvi.Contract.Repositories.IUOW;
using Microsoft.EntityFrameworkCore;

namespace Wanvi.Services.Services
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SenderEmail { get; set; }
        public string SenderPassword { get; set; }
        public bool UseSSL { get; set; }
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly IUnitOfWork _unitOfWork;

        public EmailService(IOptions<EmailSettings> emailSettings, IUnitOfWork unitOfWork)
        {
            _emailSettings = emailSettings.Value;
            _unitOfWork = unitOfWork;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public async Task SendEmailAsync(string email, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Wanvi", _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("Traveler", email));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            message.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
                await smtp.SendAsync(message);
            }
            finally
            {
                await smtp.DisconnectAsync(true);
                smtp.Dispose();
            }
        }

        public async Task AdminSendEmailAsync(SendEmailRequestModel model)
        {
            var userRepo = _unitOfWork.GetRepository<ApplicationUser>();
            List<ApplicationUser> usersToSend = new List<ApplicationUser>();

            if (model.UserIds != null && model.UserIds.Any())
            {
                usersToSend = await userRepo.FindAllAsync(u => model.UserIds.Contains(u.Id));
            }
            else if (model.RoleId.HasValue)
            {
                usersToSend = await userRepo.Entities
                    .Where(u => u.UserRoles.Any(ur => ur.RoleId == model.RoleId))
                    .ToListAsync();
            }

            if (!usersToSend.Any())
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Không tìm thấy người dùng để gửi email!");
            }

            List<string> invalidEmails = new List<string>();

            foreach (var user in usersToSend)
            {
                if (string.IsNullOrWhiteSpace(user.Email) || !IsValidEmail(user.Email))
                {
                    invalidEmails.Add(user.Email ?? "Không có email");
                    continue;
                }

                try
                {
                    await SendEmailAsync(user.Email, model.Subject, model.Body);
                }
                catch (Exception ex)
                {
                    invalidEmails.Add($"{user.Email} (Lỗi: {ex.Message})");
                }
            }

            if (invalidEmails.Any())
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST,
                    $"Một số email không hợp lệ hoặc không thể gửi: {string.Join(", ", invalidEmails)}");
            }
        }
    }
}
