using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VueCoreFramework.Models.AccountViewModels
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

        /// <summary>
        /// An optional URL to which the user will be redirected.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// Indicates that the user is to be redirected to another page.
        /// </summary>
        public bool Redirect { get; set; }

        /// <summary>
        /// A JWT bearer token.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// A list of errors generated during the operation.
        /// </summary>
        public List<string> Errors { get; set; }
    }
}
