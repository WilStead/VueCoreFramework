using System.Threading.Tasks;

namespace VueCoreFramework.Core.Services
{
    /// <summary>
    /// This interface is used by the application to send email.
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Sends an email message to the supplied address.
        /// </summary>
        /// <param name="email">The email address to which the message will be sent.</param>
        /// <param name="subject">The subject of the email message.</param>
        /// <param name="message">The body of the email message.</param>
        Task SendEmailAsync(string email, string subject, string message);
    }
}
