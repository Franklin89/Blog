using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MimeKit.Text;

namespace MLSoftware.Web.Services
{
    public class EmailService : IEmailService
    {
        private readonly IHostingEnvironment _hostingEnviroment;
        private readonly ILogger _logger;
        private readonly MailSettings _mailSettings;

        public EmailService(IHostingEnvironment hostingEnvironment, ILogger<EmailService> logger, IOptions<MailSettings> mailSettings)
        {
            _hostingEnviroment = hostingEnvironment;
            _logger = logger;
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email));
            }

            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message));
            }

            _logger.LogInformation("Sending email to {0} with the subject {1} message: {2}", email, subject, message);

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("ML-Software Blog", _mailSettings.Login));
            mimeMessage.To.Add(new MailboxAddress(_mailSettings.DefaultTo));
            mimeMessage.Subject = subject;

            mimeMessage.Body = new TextPart(TextFormat.Html)
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await client.ConnectAsync(_mailSettings.Host, _mailSettings.Port, false);

                // Note: since we don't have an OAuth2 token, disable
                // the XOAUTH2 authentication mechanism.
                client.AuthenticationMechanisms.Remove("XOAUTH2");

                // Note: only needed if the SMTP server requires authentication
                await client.AuthenticateAsync(_mailSettings.Login, _mailSettings.Password);

                await client.SendAsync(mimeMessage);
                await client.DisconnectAsync(true);
            }
        }
    }
}