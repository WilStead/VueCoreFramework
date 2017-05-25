using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace MVCCoreVue.Services
{
    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link https://go.microsoft.com/fwlink/?LinkID=532713
    public class AuthMessageSender : IEmailSender
    {
        private readonly ILogger<AuthMessageSender> _logger;

        public AuthMessageSenderOptions Options { get; }

        public AuthMessageSender(
            IOptions<AuthMessageSenderOptions> optionsAccessor,
            ILogger<AuthMessageSender> logger)
        {
            Options = optionsAccessor.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(Options.EmailFromName, Options.EmailFromAddress));
                mimeMessage.To.Add(new MailboxAddress(email));
                mimeMessage.Subject = subject;
                mimeMessage.Body = new TextPart("plain") { Text = message };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(Options.SmtpServer, Options.SmtpPort, Options.UseSsl).ConfigureAwait(false);
                    if (!Options.UseXOAUTH2)
                    {
                        // remove XOAuth2 auth mechanism without a token (ex: gmail)
                        client.AuthenticationMechanisms.Remove("XOAUTH2");
                    }
                    if (Options.RequiresAuth)
                    {
                        // If smtp server requires authentication:
                        await client.AuthenticateAsync(Options.EmailFromAddress, Options.EmailFromPassword).ConfigureAwait(false);
                    }
                    await client.SendAsync(mimeMessage).ConfigureAwait(false);
                    await client.DisconnectAsync(true).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEvent.SEND_EMAIL_ERROR, ex, "Error sending email to {EMAIL}.", email);
            }
        }
    }
}
