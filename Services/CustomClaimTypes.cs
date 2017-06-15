namespace MVCCoreVue.Services
{
    /// <summary>
    /// The colleciton of custom Claim types used by the framework.
    /// </summary>
    public static class CustomClaimTypes
    {
        /// <summary>
        /// Indicates site admin level permissions.
        /// </summary>
        public const string PermissionGroupSiteAdmin = "permission/group/siteadmin";

        /// <summary>
        /// Indicates admin level permissions.
        /// </summary>
        public const string PermissionGroupAdmin = "permission/group/admin";

        /// <summary>
        /// Indicates group manager level permissions.
        /// </summary>
        public const string PermissionGroupManager = "permission/group/manager";

        /// <summary>
        /// Indicates permission for all data operations.
        /// </summary>
        public const string PermissionDataAll = "permission/data/all";

        /// <summary>
        /// Indicates ownership permissions for a data item.
        /// </summary>
        public const string PermissionDataOwner = "permission/data/owner";

        /// <summary>
        /// Indicates all permissions.
        /// </summary>
        public const string PermissionAll = "permission/all";
    }
}
