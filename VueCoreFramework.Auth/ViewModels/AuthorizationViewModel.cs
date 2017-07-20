namespace VueCoreFramework.Auth.ViewModels
{
    /// <summary>
    /// A ViewModel used to transfer information during user account authorization tasks.
    /// </summary>
    public class AuthorizationViewModel
    {
        /// <summary>
        /// A value indicating whether the user is authorized for the requested action or not.
        /// </summary>
        public string Authorization { get; set; }

        /// <summary>
        /// Indicates that the user is authorized to share/hide the requested data.
        /// </summary>
        public string CanShare { get; set; }

        /// <summary>
        /// The email address of the user account.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Indicates whether the user is a member of the administrator role.
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Indicates whether the user is a member of the site administrator role.
        /// </summary>
        public bool IsSiteAdmin { get; set; }

        /// <summary>
        /// A JWT bearer token.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The username of the user account.
        /// </summary>
        public string Username { get; set; }
    }
}
