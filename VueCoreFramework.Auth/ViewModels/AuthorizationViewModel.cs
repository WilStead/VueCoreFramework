namespace VueCoreFramework.Auth.ViewModels
{
    /// <summary>
    /// A ViewModel used to transfer information during user account authorization tasks.
    /// </summary>
    public class AuthorizationViewModel
    {
        /// <summary>
        /// A value indicating the user's level of authorization for the requested data.
        /// </summary>
        public string Authorization { get; set; }

        /// <summary>
        /// Indicates whether the user is authorized to share/hide the requested data with anyone, their group, or no one.
        /// </summary>
        public string CanShare { get; set; }
    }
}
