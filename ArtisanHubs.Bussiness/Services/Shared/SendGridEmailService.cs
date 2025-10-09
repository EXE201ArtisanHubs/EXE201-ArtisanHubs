using Microsoft.Extensions.Options;
using SendGrid.Helpers.Mail;
using SendGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtisanHubs.Bussiness.Settings;
using MailSettings = ArtisanHubs.Bussiness.Settings.MailSettings;

namespace ArtisanHubs.Bussiness.Services.Shared
{
    public class SendGridEmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;

        // Inject IOptions<MailSettings> để đọc cấu hình từ appsettings.json
        public SendGridEmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var client = new SendGridClient(_mailSettings.ApiKey);
            var from = new EmailAddress(_mailSettings.FromEmail, _mailSettings.FromName);
            var to = new EmailAddress(toEmail);
            var subject = "Reset Your Password for ArtisanHubs";
            var plainTextContent = $"Please reset your password by clicking here: {resetLink}";
            var htmlContent = $"<strong>Please reset your password by clicking the link below:</strong><br><a href='{resetLink}'>Reset Password</a>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            // Gửi email
            var response = await client.SendEmailAsync(msg);
        }
    }
}