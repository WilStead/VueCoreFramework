using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace VueCoreFramework.Core.Data.Identity
{
    /// <summary>
    /// Provides static methods related to authorization.
    /// </summary>
    public static class Authorization
    {
        /// <summary>
        /// Indicates that the user is authorized for the requested operation.
        /// </summary>
        public const string authorized = "authorized";
        /// <summary>
        /// Indicates that the user must sign in before performing the requested operation.
        /// </summary>
        public const string login = "login";
        /// <summary>
        /// Indicates that the user is authorized to share the requested data with anyone.
        /// </summary>
        public const string shareAny = "any";
        /// <summary>
        /// Indicates that the user is authorized to share the requested data with a group.
        /// </summary>
        public const string shareGroup = "group";
        /// <summary>
        /// Indicates that the user is not authorized for the requested operation.
        /// </summary>
        public const string unauthorized = "unauthorized";

        /// <summary>
        /// Given a set of claims, determine if the user is authorized for the given operation on the
        /// given data.
        /// </summary>
        /// <param name="claims">A set of user <see cref="Claim"/> objects.</param>
        /// <param name="dataType">The type of data.</param>
        /// <param name="claimType">The type of claim requested.</param>
        /// <param name="id">
        /// The primary key of the specific data item requested (if any), as a string.
        /// </param>
        /// <returns>
        /// A string representing the authorization level held by the user for the requested data.
        /// </returns>
        public static string GetAuthorization(IList<Claim> claims, string dataType, string claimType = CustomClaimTypes.PermissionDataView, string id = null)
        {
            // First authorization for all data is checked.
            if (claims.Any(c => c.Type == CustomClaimTypes.PermissionDataAll && c.Value == CustomClaimTypes.PermissionAll))
            {
                return CustomClaimTypes.PermissionDataAll;
            }

            var claimTypes = new List<string>();

            // Otherwise, authorization for the specific operation on all data is checked.
            // In the absence of a specific operation, the default action is View.
            var claim = GetHighestClaimForValue(claims, CustomClaimTypes.PermissionAll);
            if (claim != null && PermissionIncludesTarget(claim.Type, claimType))
            {
                // If the permission is for all, return it immediately.
                if (claim.Type == CustomClaimTypes.PermissionDataAll)
                {
                    return claim.Type;
                }
                // Otherwise, store it and continue checking for other claims, so that the highest
                // can be returned.
                else
                {
                    claimTypes.Add(claim.Type);
                }
            }

            // Authorization for the specific data type is also checked.
            claim = GetHighestClaimForValue(claims, dataType);
            if (claim != null && PermissionIncludesTarget(claim.Type, claimType))
            {
                // If the permission is for all, return it immediately.
                if (claim.Type == CustomClaimTypes.PermissionDataAll)
                {
                    return claim.Type;
                }
                // Otherwise, store it and continue checking for other claims, so that the highest
                // can be returned.
                else
                {
                    claimTypes.Add(claim.Type);
                }
            }

            // If not authorized for the operation on the data type and an id is provided, the specific item is checked.
            if (!string.IsNullOrEmpty(id))
            {
                // Authorization for either all operations or the specific operation is checked.
                claim = GetHighestClaimForValue(claims, $"{dataType}{{{id}}}");
                if (claim != null && PermissionIncludesTarget(claim.Type, claimType))
                {
                    // If the permission is for all, return it immediately.
                    if (claim.Type == CustomClaimTypes.PermissionDataAll)
                    {
                        return claim.Type;
                    }
                    // Otherwise, store it and continue checking for other claims, so that the highest
                    // can be returned.
                    else
                    {
                        claimTypes.Add(claim.Type);
                    }
                }
            }
            // If no id is provided, View is allowed (to permit data table display, even if no items can be listed).
            else if (claimType == CustomClaimTypes.PermissionDataView)
            {
                claimTypes.Add(CustomClaimTypes.PermissionDataView);
            }

            var highest = GetHighestClaimType(claimTypes);
            if (string.IsNullOrEmpty(highest))
            {
                return unauthorized;
            }
            else
            {
                return highest;
            }
        }

        private static Claim GetHighestClaimForValue(IList<Claim> claims, string claimValue)
        {
            Claim max = null;
            foreach (var claim in claims.Where(c => c.Value == claimValue))
            {
                if (max == null || PermissionIncludesTarget(claim.Type, max.Type))
                {
                    max = claim;
                }
            }
            return max;
        }

        private static string GetHighestClaimType(List<string> claimTypes)
        {
            string highest = null;
            foreach (var type in claimTypes)
            {
                if (highest == null || PermissionIncludesTarget(type, highest))
                {
                    highest = type;
                }
            }
            return highest;
        }

        /// <summary>
        /// Determines whether the given claim type is a superset of the target claim type.
        /// </summary>
        /// <param name="permission">A claim type to test.</param>
        /// <param name="targetPermission">A target claim type.</param>
        /// <returns>true if the given claim type is a superset of the target claim type; false otherwise.</returns>
        public static bool PermissionIncludesTarget(string permission, string targetPermission)
        {
            if (permission == CustomClaimTypes.PermissionDataAll)
            {
                return true;
            }
            else if (permission == CustomClaimTypes.PermissionDataAdd)
            {
                return targetPermission != CustomClaimTypes.PermissionDataAll;
            }
            else if (permission == CustomClaimTypes.PermissionDataEdit)
            {
                return targetPermission == CustomClaimTypes.PermissionDataEdit
                    || targetPermission == CustomClaimTypes.PermissionDataView;
            }
            else
            {
                return targetPermission == CustomClaimTypes.PermissionDataView;
            }
        }
    }
}
