namespace VueCoreFramework.Models
{
    /// <summary>
    /// A ViewModel used to transfer information during user account authorization tasks.
    /// </summary>
    public class AuthorizationViewModel
    {
        /// <summary>
        /// A value indicating that the user is authorized for the requested action.
        /// </summary>
        public const string Authorized = "authorized";

        /// <summary>
        /// A value indicating that the user is not logged in.
        /// </summary>
        public const string Login = "login";

        /// <summary>
        /// A value indicating that the user is not authorized for the requested action.
        /// </summary>
        public const string Unauthorized = "unauthorized";

        /// <summary>
        /// A value indicating whether the user is authorized for the requested action or not.
        /// </summary>
        public string Authorization { get; set; }

        /// <summary>
        /// Indicates that the user is authorized to share/hide the requested data.
        /// </summary>
        public bool CanShare { get; set; }

        /// <summary>
        /// The email address of the user account.
        /// </summary>
        public string Email { get; set; }

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
