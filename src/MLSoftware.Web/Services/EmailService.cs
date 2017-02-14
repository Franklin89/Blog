using System.Threading.Tasks;
using System;

namespace MLSoftware.Web.Services
{
    public class EmailService : IEmailService
    {
        private readonly IHostingEnvironment _hostingEnviroment;

        public EmailService(IHostingEnvironment hostingEnvironment)
        {
            // ToDo: Pass in ILogger and Configuration (which will contain thins like smtp host - use UserSecrets for this!!!!)
            _hostingEnviroment = hostingEnvironment;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            throw new NotImplementedException();
        }
    }
}