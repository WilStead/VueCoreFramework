namespace VueCoreFramework.Core.Data.Identity
{
    /// <summary>
    /// The collection of custom Roles used by the framework.
    /// </summary>
    public static class CustomRoles
    {
        /// <summary>
        /// The site admin role.
        /// </summary>
        /// <remarks>
        /// Always a member of the admin role as well.
        /// Always only one member.
        /// Can add/remove users to/from the admin group.
        /// Can transfer the site-admin role to another admin user.
        /// </remarks>
        public const string SiteAdmin = "SiteAdmin";

        /// <summary>
        /// The admin role.
        /// </summary>
        /// <remarks>
        /// Can share/hide data with all (removes the need for permission).
        /// Can share/hide data with any group (even if the admin is not a member; but note that
        /// nothing can be hidden from the admin or site admin roles).
        /// Can lock/unlock user accounts (except admin accounts).
        /// </remarks>
        public const string Admin = "Admin";

        /// <summary>
        /// The default user role.
        /// </summary>
        /// <remarks>
        /// Implemented as a role so that data permission claims may be added to all users as a
        /// group. No individual users are expected to have this role added, and doing so will have
        /// no effect. Rather, claims for this role are automatically included for every user.
        /// </remarks>
        public const string AllUsers = "All Users";
    }
}
