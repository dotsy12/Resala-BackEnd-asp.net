using BackEnd.Application.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;


namespace BackEnd.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config) => _config = config;

        public async Task SendOtpEmailAsync(string toEmail, string otpCode,
            string purpose, CancellationToken ct = default)
        {
            var subject = purpose == "EmailVerification"
                ? "تأكيد البريد الإلكتروني — رسالة الخيرية"
                : "إعادة تعيين كلمة المرور — رسالة الخيرية";

            var purposeText = purpose == "EmailVerification"
                ? "التحقق من بريدك الإلكتروني"
                : "إعادة تعيين كلمة المرور";

            var body = $@"
<div dir='rtl' style='font-family:Arial,sans-serif;max-width:500px;margin:auto'>
  <div style='background:#1B4F72;padding:20px;border-radius:8px 8px 0 0'>
    <h2 style='color:#fff;margin:0'>🕌 رسالة الخيرية</h2>
  </div>
  <div style='background:#f9f9f9;padding:24px;border:1px solid #ddd'>
    <p>مرحباً،</p>
    <p>رمز {purposeText} الخاص بك هو:</p>
    <div style='background:#fff;border:2px dashed #1B4F72;border-radius:8px;
                padding:16px;text-align:center;margin:20px 0'>
      <span style='font-size:36px;letter-spacing:10px;
                   font-weight:bold;color:#1B4F72'>{otpCode}</span>
    </div>
    <p style='color:#e74c3c'>⏰ صالح لمدة 10 دقائق فقط</p>
    <p>إذا لم تطلب هذا الرمز، يُرجى تجاهل هذا البريد.</p>
  </div>
</div>";

            await SendAsync(toEmail, subject, body, ct);
        }

        private async Task SendAsync(string toEmail, string subject,
            string htmlBody, CancellationToken ct)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_config["Email:SenderEmail"]));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _config["Email:Host"],
                int.Parse(_config["Email:Port"] ?? "587"),
                SecureSocketOptions.StartTls, ct);
            await smtp.AuthenticateAsync(
                _config["Email:Username"],
                _config["Email:Password"], ct);
            await smtp.SendAsync(message, ct);
            await smtp.DisconnectAsync(true, ct);
        }
    }
}