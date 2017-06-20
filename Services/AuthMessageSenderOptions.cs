namespace VueCoreFramework.Services
{
    /// <summary>
    /// Options configuration object containing information about the email service used to send email from the application.
    /// </summary>
    public class AuthMessageSenderOptions
    {
        /// <summary>
        /// The 'from' name for sent emails.
        /// </summary>
        public string EmailFromName { get; set; }

        /// <summary>
        /// The email address from which messages will be sent.
        /// </summary>
        public string EmailFromAddress { get; set; }

        /// <summary>
        /// The password for the email account from which messages will be sent.
        /// </summary>
        public string EmailFromPassword { get; set; }

        /// <summary>
        /// The address of the SMTP server used to send email.
        /// </summary>
        public string SmtpServer { get; set; }

        /// <summary>
        /// The port number used for email.
        /// </summary>
        public int SmtpPort { get; set; }

        /// <summary>
        /// Indicates whether SSL is required by the mail service.
        /// </summary>
        public bool UseSsl { get; set; }

        /// <summary>
        /// Indicates whether the XOATH2 protocol is used by the mail service.
        /// </summary>
        public bool UseXOAUTH2 { get; set; }

        /// <summary>
        /// Indicates whether the mail service requires authorization.
        /// </summary>
        public bool RequiresAuth { get; set; }
    }
}
