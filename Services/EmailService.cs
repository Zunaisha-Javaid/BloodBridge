using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace BloodBridge.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody);
        Task SendBloodRequestAlertAsync(string toEmail, string toName, string bloodType, string city, string hospitalName, bool isUrgent, int requestId);
        Task SendDonorEligibilityReminderAsync(string toEmail, string toName);
        Task SendRequestResponseNotificationAsync(string toEmail, string toName, string donorName, int requestId);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _config["EmailSettings:SenderName"] ?? "BloodBridge",
                _config["EmailSettings:SenderEmail"] ?? "noreply@bloodbridge.com"
            ));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _config["EmailSettings:SmtpHost"] ?? "smtp.gmail.com",
                int.Parse(_config["EmailSettings:SmtpPort"] ?? "587"),
                SecureSocketOptions.StartTls
            );
            await client.AuthenticateAsync(
                _config["EmailSettings:Username"],
                _config["EmailSettings:Password"]
            );
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendBloodRequestAlertAsync(string toEmail, string toName, string bloodType, string city, string hospitalName, bool isUrgent, int requestId)
        {
            var urgentBadge = isUrgent ? "<span style='background:#e53e3e;color:#fff;padding:2px 8px;border-radius:4px;font-size:12px;'>URGENT</span>" : "";
            var subject = isUrgent ? $"[URGENT] Blood Request - {bloodType} needed in {city}" : $"Blood Request - {bloodType} needed in {city}";

            var html = $@"
<div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;'>
  <div style='background:#c53030;padding:20px;text-align:center;border-radius:8px 8px 0 0;'>
    <h1 style='color:#fff;margin:0;'>🩸 BloodBridge</h1>
  </div>
  <div style='background:#fff;padding:24px;border:1px solid #e2e8f0;border-radius:0 0 8px 8px;'>
    <h2>Blood Donation Request {urgentBadge}</h2>
    <p>Dear {toName},</p>
    <p>A new blood donation request matching your blood type has been posted.</p>
    <table style='width:100%;border-collapse:collapse;margin:16px 0;'>
      <tr><td style='padding:8px;border:1px solid #e2e8f0;font-weight:bold;'>Blood Type</td><td style='padding:8px;border:1px solid #e2e8f0;'>{bloodType}</td></tr>
      <tr><td style='padding:8px;border:1px solid #e2e8f0;font-weight:bold;'>Hospital</td><td style='padding:8px;border:1px solid #e2e8f0;'>{hospitalName}</td></tr>
      <tr><td style='padding:8px;border:1px solid #e2e8f0;font-weight:bold;'>City</td><td style='padding:8px;border:1px solid #e2e8f0;'>{city}</td></tr>
    </table>
    <a href='/Request/Details/{requestId}' style='background:#c53030;color:#fff;padding:12px 24px;text-decoration:none;border-radius:6px;display:inline-block;margin-top:8px;'>View Request & Respond</a>
    <p style='margin-top:24px;color:#718096;font-size:13px;'>You are receiving this because you are a registered donor with a matching blood type in {city}. To stop receiving alerts, update your availability in your profile.</p>
  </div>
</div>";

            await SendEmailAsync(toEmail, toName, subject, html);
        }

        public async Task SendDonorEligibilityReminderAsync(string toEmail, string toName)
        {
            var subject = "You are eligible to donate blood again!";
            var html = $@"
<div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;'>
  <div style='background:#c53030;padding:20px;text-align:center;border-radius:8px 8px 0 0;'>
    <h1 style='color:#fff;margin:0;'>🩸 BloodBridge</h1>
  </div>
  <div style='background:#fff;padding:24px;border:1px solid #e2e8f0;border-radius:0 0 8px 8px;'>
    <h2>You Can Donate Again! 🎉</h2>
    <p>Dear {toName},</p>
    <p>It has been 56 days since your last donation. You are now eligible to donate blood again.</p>
    <p>Your donation can save up to <strong>3 lives</strong>. Consider checking the latest blood requests in your city.</p>
    <a href='/Request/Feed' style='background:#c53030;color:#fff;padding:12px 24px;text-decoration:none;border-radius:6px;display:inline-block;margin-top:8px;'>View Blood Requests</a>
  </div>
</div>";

            await SendEmailAsync(toEmail, toName, subject, html);
        }

        public async Task SendRequestResponseNotificationAsync(string toEmail, string toName, string donorName, int requestId)
        {
            var subject = "A donor has responded to your blood request!";
            var html = $@"
<div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;'>
  <div style='background:#c53030;padding:20px;text-align:center;border-radius:8px 8px 0 0;'>
    <h1 style='color:#fff;margin:0;'>🩸 BloodBridge</h1>
  </div>
  <div style='background:#fff;padding:24px;border:1px solid #e2e8f0;border-radius:0 0 8px 8px;'>
    <h2>Donor Response Received!</h2>
    <p>Dear {toName},</p>
    <p><strong>{donorName}</strong> has responded to your blood request and wants to help.</p>
    <p>Please log in to view their contact details and coordinate with them.</p>
    <a href='/Request/MyRequests' style='background:#c53030;color:#fff;padding:12px 24px;text-decoration:none;border-radius:6px;display:inline-block;margin-top:8px;'>View My Requests</a>
  </div>
</div>";

            await SendEmailAsync(toEmail, toName, subject, html);
        }
    }
}
