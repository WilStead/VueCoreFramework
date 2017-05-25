namespace MVCCoreVue.Services
{
    public class AuthMessageSenderOptions
    {
        public string EmailFromName { get; set; }
        public string EmailFromAddress { get; set; }
        public string EmailFromPassword { get; set; }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public bool UseSsl { get; set; }
        public bool UseXOAUTH2 { get; set; }
        public bool RequiresAuth { get; set; }
    }
}
