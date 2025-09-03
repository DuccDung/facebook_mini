using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using mail_service.Internal;

namespace mail_service.service
{
    public class SmtpEmailSender(IConfiguration config) : IEmailSender
    {
        public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(config["Email:FromName"], config["Email:FromAddress"]));
            msg.To.Add(MailboxAddress.Parse(to));
            msg.Subject = subject;
            msg.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            var host = config["Smtp:Host"];
            var port = int.Parse(config["Smtp:Port"] ?? "587");
            var useStartTls = bool.Parse(config["Smtp:UseStartTls"] ?? "true");
            var timeout = TimeSpan.FromSeconds(int.Parse(config["Smtp:TimeoutSeconds"] ?? "20"));

            client.Timeout = (int)timeout.TotalMilliseconds;
            await client.ConnectAsync(host, port, useStartTls ? MailKit.Security.SecureSocketOptions.StartTls : MailKit.Security.SecureSocketOptions.Auto, ct);

            var user = config["Smtp:User"]; var pass = config["Smtp:Pass"];
            if (!string.IsNullOrWhiteSpace(user)) await client.AuthenticateAsync(user, pass, ct);

            await client.SendAsync(msg, ct);
            await client.DisconnectAsync(true, ct);
        }
    }
}
