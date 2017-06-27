using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VueCoreFramework.Models;
using VueCoreFramework.Services;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace VueCoreFramework.Controllers
{
    /// <summary>
    /// An MVC controller for handling user authorization tasks.
    /// </summary>
    [Authorize]
    [Route("api/[controller]/[action]")]
    public class AuthorizationController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly TokenProviderOptions _tokenOptions;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Initializes a new instance of <see cref="AuthorizationController"/>.
        /// </summary>
        public AuthorizationController(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IOptions<TokenProviderOptions> tokenOptions)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _tokenOptions = tokenOptions.Value;
        }

        /// <summary>
        /// Called to authenticate the user.
        /// </summary>
        /// <returns>
        /// An <see cref="AuthorizationViewModel"/> indicating whether the current user is authenticated.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> Authenticate(string full = null)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.AdminLocked)
            {
                return Json(new AuthorizationViewModel { Authorization = AuthorizationViewModel.Login });
            }

            var vm = new AuthorizationViewModel
            {
                Authorization = AuthorizationViewModel.Authorized,
                Token = AccountController.GetLoginToken(user, _userManager, _tokenOptions)
            };
            if (full != "true")
            {
                return Json(vm);
            }

            vm.Email = user.Email;
            vm.Username = user.UserName;

            var roles = await _userManager.GetRolesAsync(user);
            roles.Add(CustomRoles.AllUsers);
            var claims = await _userManager.GetClaimsAsync(user);
            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                claims = claims.Concat(roleClaims).ToList();
            }
            vm.IsAdmin = roles.Any(r => r == CustomRoles.Admin);
            vm.IsSiteAdmin = vm.IsAdmin && roles.Any(r => r == CustomRoles.SiteAdmin);

            return Json(vm);
        }

        /// <summary>
        /// Called to authorize the current user for a particular operation.
        /// </summary>
        /// <param name="dataType">
        /// The type of data involved in the current operation.
        /// </param>
        /// <param name="operation">An optional operation being performed.</param>
        /// <param name="id">An optional primary key for the item involved in the current operation.</param>
        /// <returns>
        /// An <see cref="AuthorizationViewModel"/> indicating whether the current user is authorized.
        /// </returns>
        [HttpGet("{dataType}")]
        public async Task<IActionResult> Authorize(string dataType, string operation = "view", string id = null)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.AdminLocked)
            {
                return Json(new AuthorizationViewModel { Authorization = AuthorizationViewModel.Login });
            }
            var token = AccountController.GetLoginToken(user, _userManager, _tokenOptions);
            var vm = new AuthorizationViewModel
            {
                Email = user.Email,
                Token = token,
                Username = user.UserName
            };

            var roles = await _userManager.GetRolesAsync(user);
            roles.Add(CustomRoles.AllUsers);
            var claims = await _userManager.GetClaimsAsync(user);
            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                claims = claims.Concat(roleClaims).ToList();
            }
            vm.IsAdmin = roles.Any(r => r == CustomRoles.Admin);
            vm.IsSiteAdmin = vm.IsAdmin && roles.Any(r => r == CustomRoles.SiteAdmin);

            operation = $"permission/data/{operation}";
            vm.Authorization = GetAuthorization(claims, dataType, operation, id);

            // Admins can share/hide any data, and managers can share/hide any data they have
            // authorization for with their own group. Other users can only share/hide data they own.
            if (vm.Authorization != AuthorizationViewModel.Unauthorized)
            {
                if (vm.IsAdmin
                    || (!string.IsNullOrEmpty(id)
                    && claims.Any(c => c.Type == CustomClaimTypes.PermissionDataOwner && c.Value == $"{dataType}{{{id}}}")))
                {
                    vm.CanShare = AuthorizationViewModel.ShareAny;
                }
                else if (!string.IsNullOrEmpty(id) && claims.Any(c => c.Type == CustomClaimTypes.PermissionGroupManager))
                {
                    vm.CanShare = AuthorizationViewModel.ShareGroup;
                }
            }

            return Json(vm);
        }

        internal static string GetAuthorization(IList<Claim> claims, string dataType, string claimType = CustomClaimTypes.PermissionDataView, string id = null)
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
                return AuthorizationViewModel.Unauthorized;
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

        internal static bool PermissionIncludesTarget(string permission, string targetPermission)
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
