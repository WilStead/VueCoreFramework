namespace VueCoreFramework.Core.Configuration
{
    /// <summary>
    /// Provides the URLs for the different hosts which form the application.
    /// </summary>
    public class URLOptions
    {
        /// <summary>
        /// The URL of the API server.
        /// </summary>
        public string ApiURL { get; set; }

        /// <summary>
        /// The URL of the authentication server.
        /// </summary>
        public string AuthURL { get; set; }

        /// <summary>
        /// The URL of the default Vue client.
        /// </summary>
        public string ClientURL { get; set; }
    }
}
