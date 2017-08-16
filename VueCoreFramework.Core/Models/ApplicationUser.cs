using Microsoft.AspNetCore.Identity;
using System;

namespace VueCoreFramework.Core.Models
{
    /// <summary>
    /// Represents a user.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// The user's selected culture.
        /// </summary>
        public string Culture { get; set; } = "en-US";

        /// <summary>
        /// The new email requested during an email change.
        /// </summary>
        public string NewEmail { get; set; }

        /// <summary>
        /// The original email after an email change.
        /// </summary>
        public string OldEmail { get; set; }

        /// <summary>
        /// The date/time of the last email change.
        /// </summary>
        /// <remarks>
        /// Only one is allowed per day, to make it more difficult for an unauthorized party who
        /// gains access to an account to perform a double-change, and thereby make it impossible for
        /// the original owner to recover the account.
        /// </remarks>
        public DateTime LastEmailChange { get; set; }

        /// <summary>
        /// Indicates that the user's account has been locked by an admin.
        /// </summary>
        /// <remarks>
        /// An admin lock prevents any use or access with the account. An account is locked rather
        /// than deleted to prevent re-registration with the same information. Since only verified
        /// email accounts are allowed access, a repeat offender must at least go to the trouble of
        /// creating new valid email accounts in order to create new site accounts.
        /// </remarks>
        public bool AdminLocked { get; set; }
    }
}