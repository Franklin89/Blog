using System.Threading.Tasks;

namespace MLSoftware.Web.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}