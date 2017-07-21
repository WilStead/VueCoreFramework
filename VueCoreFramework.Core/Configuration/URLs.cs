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
        /// The URL of the API server.
        /// </summary>
        public const string ApiURL = "https://localhost:44325/";

        /// <summary>
        /// The URL of the authentication server.
        /// </summary>
        public const string AuthURL = "https://localhost:44300/";

        /// <summary>
        /// The URL of the default Vue client.
        /// </summary>
        public const string ClientURL = "https://localhost:44350/";
    }
}
