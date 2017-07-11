using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VueCoreFramework.Models.ViewModels
{
    /// <summary>
    /// A ViewModel used to transfer information during user account management tasks.
    /// </summary>
    public class ManageUserViewModel
    {
        /// <summary>
        /// The username of the account.
        /// </summary>
        [EmailAddress]
        [Display(Name = "Username")]
        public string Username { get; set; }

        /// <summary>
        /// The email address of the user account.
        /// </summary>
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        /// The original password of the user account.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        /// <summary>
        /// The new password for the user account.
        /// </summary>
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        /// <summary>
        /// The new password for the user account, repeated.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// The name of a third-party authorization provider.
        /// </summary>
        public string AuthProvider { get; set; }
    }
}
