namespace VueCoreFramework.Core.Configuration
{
    /// <summary>
    /// Options configuration object containing information about the default admin email account.
    /// </summary>
    public class AdminOptions
    {
        /// <summary>
        /// The default site admin email address.
        /// </summary>
        public string AdminEmailAddress { get; set; }

        /// <summary>
        /// The default site admin email password.
        /// </summary>
        public string AdminPassword { get; set; }
    }
}
