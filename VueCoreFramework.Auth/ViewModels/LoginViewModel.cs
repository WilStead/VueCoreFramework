using System.ComponentModel.DataAnnotations;

namespace VueCoreFramework.Auth.ViewModels
{
    /// <summary>
    /// A ViewModel used to transfer information during user account login tasks.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// The username or email address of the user account.
        /// </summary>
        [Required]
        public string Username { get; set; }

        /// <summary>
        /// The password for the user account.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// The name of a third-party authorization provider.
        /// </summary>
        public string AuthProvider { get; set; }

        /// <summary>
        /// Indicates that the user wishes their authorization to be stored in the browser and used
        /// again during future sessions, rather than forgotten after navigating away from the site.
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberUser { get; set; }
    }
}
