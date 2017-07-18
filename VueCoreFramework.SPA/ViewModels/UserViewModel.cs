namespace VueCoreFramework.ViewModels
{
    /// <summary>
    /// Used to transfer information about a user.
    /// </summary>
    public class UserViewModel
    {
        /// <summary>
        /// The user's email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Indicates whether the user's account has been locked by an admin.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// The username of the user.
        /// </summary>
        public string Username { get; set; }
    }
}
