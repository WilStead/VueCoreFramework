using System.Net;

namespace VueCoreFramework.Core.Configuration
{
    /// <summary>
    /// Provides the URLs for the different hosts which form the application.
    /// </summary>
    /// <remarks>
    /// This static class is used by the Program files for self-hosting, when dependency injection
    /// from settings is not yet available. When deploying to production, it may or may not be
    /// necessary to update these, depending on how you are deploying and hosting.
    /// </remarks>
    public static class URLs
    {
        /// <summary>
        /// The IP of the API server.
        /// </summary>
        public const string Api_IP = "127.0.0.1";

        /// <summary>
        /// The port number of the API server.
        /// </summary>
        public const int Api_Port = 44325;

        /// <summary>
        /// The IP of the authentication server.
        /// </summary>
        public const string Auth_IP = "127.0.0.1";

        /// <summary>
        /// The port number of the authentication server.
        /// </summary>
        public const int Auth_Port = 44300;

        /// <summary>
        /// The IP of the default Vue client.
        /// </summary>
        public const string Client_IP = "127.0.0.1";

        /// <summary>
        /// The port number of the default Vue client.
        /// </summary>
        public const int Client_Port = 44350;
    }
}
