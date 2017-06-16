using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MVCCoreVue.Models.AccountViewModels
{
    /// <summary>
    /// A ViewModel used to transfer information during user account registration tasks.
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// The username for the account.
        /// </summary>
        [Required]
        [StringLength(24, MinimumLength = 6, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        /// <summary>
        /// The email address of the user account.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        /// The password for the user account.
        /// </summary>
        [Required]
        [StringLength(24, MinimumLength = 6, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// The password for the user account, repeated.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// An optional URL to which the user will be redirected.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// Indicates that the user is to be redirected to another page.
        /// </summary>
        public bool Redirect { get; set; }

        /// <summary>
        /// A list of errors generated during the operation.
        /// </summary>
        public List<string> Errors { get; set; }
    }
}
