using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
            _logger.LogInformation("Sending email to {0} with the subject {1} message: {2}", email, subject, message);
        }
    }
}