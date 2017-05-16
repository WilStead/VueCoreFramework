using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace MVCCoreVue.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string OldEmail { get; set; }
    }
}