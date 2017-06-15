namespace MVCCoreVue.Services
{
    /// <summary>
    /// The colleciton of custom Roles used by the framework.
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
    }
}
