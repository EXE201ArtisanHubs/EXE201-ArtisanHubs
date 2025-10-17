//using Microsoft.Extensions.Options;
//using SendGrid.Helpers.Mail;
//using SendGrid;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ArtisanHubs.Bussiness.Settings;
//using MailSettings = ArtisanHubs.Bussiness.Settings.MailSettings;

//namespace ArtisanHubs.Bussiness.Services.Shared
//{
//    public class SendGridEmailService : IEmailService
//    {
//        private readonly MailSettings _mailSettings;

//        // Inject IOptions<MailSettings> để đọc cấu hình từ appsettings.json
//        public SendGridEmailService(IOptions<MailSettings> mailSettings)
//        {
//            _mailSettings = mailSettings.Value;
//        }

//        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
//        {
//            var client = new SendGridClient(_mailSettings.ApiKey);
//            var from = new EmailAddress(_mailSettings.FromEmail, _mailSettings.FromName);
//            var to = new EmailAddress(toEmail);
//            var subject = "Reset Your Password for ArtisanHubs";
//            var plainTextContent = $"Please reset your password by clicking here: {resetLink}";
//            var htmlContent = $"<strong>Please reset your password by clicking the link below:</strong><br><a href='{resetLink}'>Reset Password</a>";

//            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

//            // Gửi email
//            var response = await client.SendEmailAsync(msg);
//        }
//    }
//}
using Microsoft.Extensions.Options;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Threading.Tasks;
using ArtisanHubs.Bussiness.Settings;
using MailSettings = ArtisanHubs.Bussiness.Settings.MailSettings;

// Giả sử IEmailService nằm ở namespace này, bạn hãy điều chỉnh nếu cầ

namespace ArtisanHubs.Bussiness.Services.Shared
{
    public class SendGridEmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;

        public SendGridEmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        // --- PHƯƠNG THỨC MỚI ĐỂ GỬI OTP ---
        public async Task SendPasswordResetOtpAsync(string toEmail, string otpCode)
        {
            var client = new SendGridClient(_mailSettings.ApiKey);
            var from = new EmailAddress(_mailSettings.FromEmail, _mailSettings.FromName);
            var to = new EmailAddress(toEmail);
            var subject = "Your Password Reset Code for ArtisanHubs";
            var plainTextContent = $"Your verification code is: {otpCode}. This code will expire in 10 minutes.";
            var htmlContent = $@"
                <h1>Password Reset Request</h1>
                <p>You requested a password reset. Use the code below to reset your password.</p>
                <p>Your verification code is:</p>
                <h2 style='color: #007BFF; text-align: center; letter-spacing: 2px;'>{otpCode}</h2>
                <p>This code will expire in 10 minutes.</p>
                <p>If you did not request this, please ignore this email.</p>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            // Gửi email
            var response = await client.SendEmailAsync(msg);

            // Bạn có thể thêm log hoặc xử lý response ở đây nếu cần
        }

        // --- PHƯƠNG THỨC CŨ GỬI LINK (bạn có thể xóa đi nếu không dùng) ---
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