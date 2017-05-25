using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;

namespace MVCCoreVue.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string NewEmail { get; set; }
        public string OldEmail { get; set; }
        public DateTime LastEmailChange { get; set; }
    }
}