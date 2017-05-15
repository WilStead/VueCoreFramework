using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MVCCoreVue.Models.AccountViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }

        public bool Redirect { get; set; }

        public Dictionary<string, string> Errors { get; set; }
    }
}
