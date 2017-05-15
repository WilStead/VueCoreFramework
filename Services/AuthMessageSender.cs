using MailKit.Net.Smtp;
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
        public AuthMessageSenderOptions Options { get; }

        public AuthMessageSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            ExecuteSendEmail(Options.EmailFromAddress, Options.EmailFromPassword, email, subject, message).Wait();
            return Task.FromResult(0);
        }

        private async Task ExecuteSendEmail(string fromAddress, string fromPassword, string email, string subject, string message)
        {
            // TODO: use real info
            string fromAddressTitle = "Test Email from ASP.NET Core MVC Vue";

            string smtpServer = "smtp.example.com";
            int smtpPort = 587;
            bool useSsl = false;

            try
            {
                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(fromAddressTitle, fromAddress));
                mimeMessage.To.Add(new MailboxAddress(email));
                mimeMessage.Subject = subject;
                mimeMessage.Body = new TextPart("plain") { Text = message };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(smtpServer, smtpPort, useSsl).ConfigureAwait(false);
                    // remove XOAuth2 auth mechanism without a token (ex: gmail)
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    // If smtp server requires authentication:
                    await client.AuthenticateAsync(fromAddress, fromPassword).ConfigureAwait(false);
                    await client.SendAsync(mimeMessage).ConfigureAwait(false);
                    await client.DisconnectAsync(true).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                // TODO: implement exception handling
                Console.WriteLine(ex);
            }
        }
    }
}
