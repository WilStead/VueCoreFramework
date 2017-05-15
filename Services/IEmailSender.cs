using System.Threading.Tasks;

namespace MVCCoreVue.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
