using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VueCoreFramework.Data;
using VueCoreFramework.Data.Attributes;
using VueCoreFramework.Extensions;
using VueCoreFramework.Models;
using VueCoreFramework.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private readonly AdminOptions _adminOptions;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly TokenProviderOptions _tokenOptions;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Initializes a new instance of <see cref="AuthorizationController"/>.
        /// </summary>
        public AuthorizationController(
            IOptions<AdminOptions> adminOptions,
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IOptions<TokenProviderOptions> tokenOptions)
        {
            _adminOptions = adminOptions.Value;
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _tokenOptions = tokenOptions.Value;
        }

        /// <summary>
        /// Called to authorize the current user for a particular operation, or simply to
        /// authenticate the user if no oepration is specified.
        /// </summary>
        /// <param name="dataType">
        /// An optional name indicating the type of data involved in the current operation.
        /// </param>
        /// <param name="operation">An optional operation being performed.</param>
        /// <param name="id">An optional primary key for the item involved in the current operation.</param>
        /// <returns>
        /// An <see cref="AuthorizationViewModel"/> indicating whether the current user is authorized
        /// (or authenticated).
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> Authorize(string dataType = null, string operation = "view", string id = null)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.AdminLocked)
            {
                return Json(new AuthorizationViewModel { Authorization = AuthorizationViewModel.Unauthorized });
            }
            var token = AccountController.GetLoginToken(user, _userManager, _tokenOptions);
            var vm = new AuthorizationViewModel
            {
                Username = user.UserName,
                Email = user.Email,
                Token = token
            };

            // If no specific data is being requested, just being a recognized user is sufficient authorization.
            if (string.IsNullOrEmpty(dataType))
            {
                vm.Authorization = AuthorizationViewModel.Authorized;
                return Json(vm);
            }

            var roles = await _userManager.GetRolesAsync(user);
            roles.Add(CustomRoles.AllUsers);
            var claims = await _userManager.GetClaimsAsync(user);
            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                claims = claims.Concat(roleClaims).ToList();
            }
            var claimType = $"permission/data/{operation}";
            vm.Authorization = IsAuthorized(claims, dataType, claimType, id)
                ? AuthorizationViewModel.Authorized
                : AuthorizationViewModel.Unauthorized;
            return Json(vm);
        }

        [HttpPost("{username}/{group}")]
        public async Task<IActionResult> AddUserToGroup(string username, string group)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = $"Your account has been locked. Please contact an administrator at {_adminOptions.AdminEmailAddress} for assistance." });
            }
            if (username == user.UserName)
            {
                return Json(new { error = "You cannot add yourself to a group." });
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (group == CustomRoles.SiteAdmin)
            {
                return Json(new { error = "You cannot add anyone to the Site Administrator group." });
            }
            else if (group == CustomRoles.Admin)
            {
                if (!roles.Contains(CustomRoles.SiteAdmin))
                {
                    return Json(new { error = "Only the Site Administrator may add users to the Administrator group." });
                }
            }
            // Admins can add users to any non-admin group, regardless of their own membership.
            else if (!roles.Contains(CustomRoles.Admin))
            {
                var claims = await _userManager.GetClaimsAsync(user);
                if (!claims.Contains(new Claim(CustomClaimTypes.PermissionGroupManager, group)))
                {
                    return Json(new { error = "Only the group's manager may add users to a group." });
                }
            }

            var groupRole = await _roleManager.FindByNameAsync(group);
            if (groupRole == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var targetUser = await _userManager.FindByNameAsync(username);
            if (targetUser == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            await _userManager.AddToRoleAsync(targetUser, groupRole.Name);
            return Ok();
        }

        [HttpPost("{dataType}")]
        public async Task<IActionResult> HideDataFromAll(string dataType, string operation, string id)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = $"Your account has been locked. Please contact an administrator at {_adminOptions.AdminEmailAddress} for assistance." });
            }
            var roles = await _userManager.GetRolesAsync(user);
            // Only Admins can hide data from all.
            if (!roles.Contains(CustomRoles.Admin))
            {
                return Json(new { error = "Only Administrators can hide data which has been shared with all users." });
            }

            if (string.IsNullOrEmpty(dataType))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            dataType = dataType.ToInitialCaps();
            // Entity isn't used, but is parsed to enure it's valid.
            var entity = _context.Model.GetEntityTypes().FirstOrDefault(e => e.Name.Substring(e.Name.LastIndexOf('.') + 1) == dataType);
            if (entity == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            operation = $"permission/data/{operation.ToLower()}";
            var allRole = await _roleManager.FindByNameAsync(CustomRoles.AllUsers);
            Claim claim = null;
            // Hide a data type, rather than a particular item.
            if (string.IsNullOrEmpty(id))
            {
                claim = new Claim(operation, dataType);
            }
            else
            {
                claim = new Claim(operation, $"{dataType}{{{id}}}");
            }
            await _roleManager.RemoveClaimAsync(allRole, claim);
            // Remove implies claims.
            if (claim.Type == CustomClaimTypes.PermissionDataView)
            {
                await _roleManager.RemoveClaimAsync(allRole, new Claim(CustomClaimTypes.PermissionDataEdit, claim.Value));
                await _roleManager.RemoveClaimAsync(allRole, new Claim(CustomClaimTypes.PermissionDataAdd, claim.Value));
            }
            else if (claim.Type == CustomClaimTypes.PermissionDataEdit)
            {
                await _roleManager.RemoveClaimAsync(allRole, new Claim(CustomClaimTypes.PermissionDataAdd, claim.Value));
            }
            return Ok();
        }

        [HttpPost("{group}/{dataType}")]
        public async Task<IActionResult> HideDataFromGroup(string group, string dataType, string operation, string id)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = $"Your account has been locked. Please contact an administrator at {_adminOptions.AdminEmailAddress} for assistance." });
            }
            var roles = await _userManager.GetRolesAsync(user);
            // Only admins can hide a data type, rather than a particular item
            if (string.IsNullOrEmpty(id) && !roles.Contains(CustomRoles.Admin))
            {
                return Json(new { error = "Only Administrators can hide data types." });
            }
            // Admins can hide data from any group, regardless of their own membership.
            if (!roles.Contains(CustomRoles.Admin))
            {
                var claims = await _userManager.GetClaimsAsync(user);
                if (!claims.Contains(new Claim(CustomClaimTypes.PermissionGroupManager, group))
                    && !claims.Contains(new Claim(CustomClaimTypes.PermissionDataOwner, $"{dataType}{{{id}}}")))
                {
                    return Json(new { error = "Only the group's manager or the original owner of the data may hide data which has been shared with a group." });
                }
            }

            var groupRole = await _roleManager.FindByNameAsync(group);
            if (groupRole == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (!TryGetClaim(dataType, operation, id, out Claim claim))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            await _roleManager.RemoveClaimAsync(groupRole, claim);
            // Remove implied permissions.
            if (claim.Type == CustomClaimTypes.PermissionDataView)
            {
                await _roleManager.RemoveClaimAsync(groupRole, new Claim(CustomClaimTypes.PermissionDataEdit, claim.Value));
                await _roleManager.RemoveClaimAsync(groupRole, new Claim(CustomClaimTypes.PermissionDataAdd, claim.Value));
            }
            if (claim.Type == CustomClaimTypes.PermissionDataEdit)
            {
                await _roleManager.RemoveClaimAsync(groupRole, new Claim(CustomClaimTypes.PermissionDataAdd, claim.Value));
            }
            return Ok();
        }

        [HttpPost("{username}/{dataType}")]
        public async Task<IActionResult> HideDataFromUser(string username, string dataType, string operation, string id)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = $"Your account has been locked. Please contact an administrator at {_adminOptions.AdminEmailAddress} for assistance." });
            }
            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            // Only admins can hide a data type, rather than a particular item
            if (string.IsNullOrEmpty(id) && !roles.Contains(CustomRoles.Admin))
            {
                return Json(new { error = "Only Administrators can hide data types." });
            }
            if (!roles.Contains(CustomRoles.Admin)
                && !claims.Contains(new Claim(CustomClaimTypes.PermissionDataOwner, $"{dataType}{{{id}}}")))
            {
                return Json(new { error = "Only the original owner of the data may hide data which has been shared with another user." });
            }

            var targetUser = await _userManager.FindByNameAsync(username);
            if (targetUser == null)
            {
                return Json(new { error = "That user could not be found." });
            }
            if (!TryGetClaim(dataType, operation, id, out Claim claim))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            List<Claim> removedClaims = new List<Claim> { claim };
            // Remove implied permissions.
            if (claim.Type == CustomClaimTypes.PermissionDataView)
            {
                removedClaims.Add(new Claim(CustomClaimTypes.PermissionDataEdit, claim.Value));
                removedClaims.Add(new Claim(CustomClaimTypes.PermissionDataAdd, claim.Value));
            }
            else if (claim.Type == CustomClaimTypes.PermissionDataEdit)
            {
                removedClaims.Add(new Claim(CustomClaimTypes.PermissionDataAdd, claim.Value));
            }
            await _userManager.RemoveClaimsAsync(targetUser, removedClaims);
            return Ok();
        }

        internal static bool IsAuthorized(IList<Claim> claims, string dataType, string claimType = CustomClaimTypes.PermissionDataView, string id = null)
        {
            // First authorization for all data is checked.
            if (claims.Any(c => c.Type == CustomClaimTypes.PermissionDataAll && c.Value == CustomClaimTypes.PermissionAll))
            {
                return true;
            }

            // If not authorized for all data, authorization for the specific operation on all data is checked.
            // In the absence of a specific operation, the default action is View.
            // View permission is implied by Edit permission.
            if (claims.Any(c => c.Value == CustomClaimTypes.PermissionAll && c.Type == claimType))
            {
                return true;
            }

            // If not authorized for the operation on all data, authorization for the specific data type is checked.
            if (claims.Any(c => c.Value == dataType
                && (c.Type == CustomClaimTypes.PermissionDataAll || c.Type == claimType)))
            {
                return true;
            }

            // If not authorized for the operation on the data type and an id is provided, the specific item is checked.
            if (!string.IsNullOrEmpty(id))
            {
                // Authorization for either all operations or the specific operation is checked.
                if (claims.Any(c => c.Value == $"{dataType}{{{id}}}"
                    && (c.Type == CustomClaimTypes.PermissionDataAll || c.Type == claimType)))
                {
                    return true;
                }
            }
            // If no id is provided, only View is allowed (to permit data table display, even if no items can be listed).
            else if (claimType == CustomClaimTypes.PermissionDataView)
            {
                return true;
            }

            // No authorizations found.
            return false;
        }

        [HttpPost("{username}/{group}")]
        public async Task<IActionResult> RemoveUserFromGroup(string username, string group)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = $"Your account has been locked. Please contact an administrator at {_adminOptions.AdminEmailAddress} for assistance." });
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (group == CustomRoles.SiteAdmin)
            {
                if (roles.Contains(CustomRoles.SiteAdmin))
                {
                    return Json(new { error = "You cannot remove yourself from the Site Administrator role in this way. Transfer the role to another Administrator instead." });
                }
                else
                {
                    return Json(new { error = "You cannot remove the Site Administrator." });
                }
            }
            else if (group == CustomRoles.Admin)
            {
                if (!roles.Contains(CustomRoles.SiteAdmin))
                {
                    return Json(new { error = "Only the Site Administrator may remove users from the Administrator group." });
                }
            }
            // Admins can remove users from any non-admin group, regardless of their own membership.
            else if (!roles.Contains(CustomRoles.Admin))
            {
                var claims = await _userManager.GetClaimsAsync(user);
                if (!claims.Contains(new Claim(CustomClaimTypes.PermissionGroupManager, group)))
                {
                    return Json(new { error = "Only the group's manager may remove users from a group." });
                }
            }

            var groupRole = await _roleManager.FindByNameAsync(group);
            if (groupRole == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var targetUser = await _userManager.FindByNameAsync(username);
            if (targetUser == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            await _userManager.RemoveFromRoleAsync(targetUser, groupRole.Name);
            return Ok();
        }

        [HttpPost("{dataType}")]
        public async Task<IActionResult> ShareDataWithAll(string dataType, string operation, string id)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = $"Your account has been locked. Please contact an administrator at {_adminOptions.AdminEmailAddress} for assistance." });
            }
            var roles = await _userManager.GetRolesAsync(user);
            // Only Admins can share data with all.
            if (!roles.Contains(CustomRoles.Admin))
            {
                return Json(new { error = "Only Administrators can share data with all users." });
            }
            operation = operation.ToLower();
            if (operation != "view" && operation != "edit")
            {
                return Json(new { error = "Only view or edit permission can be shared." });
            }

            if (string.IsNullOrEmpty(dataType))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            dataType = dataType.ToInitialCaps();
            // Entity isn't used, but is parsed to enure it's valid.
            var entity = _context.Model.GetEntityTypes().FirstOrDefault(e => e.Name.Substring(e.Name.LastIndexOf('.') + 1) == dataType);
            if (entity == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            operation = $"permission/data/{operation}";
            var allRole = await _roleManager.FindByNameAsync(CustomRoles.AllUsers);
            var roleClaims = await _roleManager.GetClaimsAsync(allRole);
            Claim claim = null;
            // Share data type, rather than an item.
            if (string.IsNullOrEmpty(id))
            {
                claim = new Claim(operation, dataType);
            }
            else
            {
                // The Guid isn't needed, but is parsed to ensure it's valid.
                if (!Guid.TryParse(id, out Guid guid))
                {
                    return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
                }
                claim = new Claim(operation, $"{dataType}{{{id}}}");
            }
            await _roleManager.AddClaimAsync(allRole, claim);
            // Add implies claims.
            if (claim.Type == CustomClaimTypes.PermissionDataAdd)
            {
                await _roleManager.AddClaimAsync(allRole, new Claim(CustomClaimTypes.PermissionDataEdit, claim.Value));
                await _roleManager.AddClaimAsync(allRole, new Claim(CustomClaimTypes.PermissionDataView, claim.Value));
            }
            else if (claim.Type == CustomClaimTypes.PermissionDataEdit)
            {
                await _roleManager.AddClaimAsync(allRole, new Claim(CustomClaimTypes.PermissionDataView, claim.Value));
            }
            return Ok();
        }

        [HttpPost("{group}/{dataType}")]
        public async Task<IActionResult> ShareDataWithGroup(string group, string dataType, string operation, string id)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = $"Your account has been locked. Please contact an administrator at {_adminOptions.AdminEmailAddress} for assistance." });
            }
            var groupRole = await _roleManager.FindByNameAsync(group);
            if (groupRole == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (!TryGetClaim(dataType, operation, id, out Claim claim))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            var operationLower = operation.ToLower();
            // Only admins can share a data type, rather than a particular item
            if (string.IsNullOrEmpty(id) && !roles.Contains(CustomRoles.Admin))
            {
                return Json(new { error = "Only Administrators can share data types." });
            }
            // Admins can share data with any group as if they owned that data, regardless of their own membership.
            if (roles.Contains(CustomRoles.Admin)
                || claims.Contains(new Claim(CustomClaimTypes.PermissionDataOwner, $"{dataType}{{{id}}}")))
            {
                // Permissions other than view/edit can only be shared for an entire type.
                if (!string.IsNullOrEmpty(id) && operationLower != "view" && operationLower != "edit")
                {
                    return Json(new { error = "Only view or edit permission can be shared." });
                }
            }
            else
            {
                // Managers of groups can re-share data with their group which has been shared with them.
                if (claims.Contains(new Claim(CustomClaimTypes.PermissionGroupManager, group)))
                {
                    // If the manager has edit permission, the manager can also share view permission.
                    if (!claims.Contains(claim) &&
                        (operationLower != "view" || !claims.Contains(new Claim(CustomClaimTypes.PermissionDataEdit, $"{dataType}{{{id}}}"))))
                    {
                        return Json(new { error = "You may only share data permissions with your group that have previously been shared with you." });
                    }
                }
                else
                {
                    return Json(new { error = "Only the group's manager or the original owner of the data may share data with a group." });
                }
            }

            await _roleManager.AddClaimAsync(groupRole, claim);
            // Add implied permissions.
            if (claim.Type == CustomClaimTypes.PermissionDataAdd)
            {
                await _roleManager.AddClaimAsync(groupRole, new Claim(CustomClaimTypes.PermissionDataEdit, claim.Value));
                await _roleManager.AddClaimAsync(groupRole, new Claim(CustomClaimTypes.PermissionDataView, claim.Value));
            }
            else if (claim.Type == CustomClaimTypes.PermissionDataEdit)
            {
                await _roleManager.AddClaimAsync(groupRole, new Claim(CustomClaimTypes.PermissionDataView, claim.Value));
            }
            return Ok();
        }

        [HttpPost("{username}/{dataType}")]
        public async Task<IActionResult> ShareDataWithUser(string username, string dataType, string operation, string id)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = $"Your account has been locked. Please contact an administrator at {_adminOptions.AdminEmailAddress} for assistance." });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            var operationLower = operation.ToLower();
            // Only admins can share a data type, rather than a particular item
            if (string.IsNullOrEmpty(id) && !roles.Contains(CustomRoles.Admin))
            {
                return Json(new { error = "Only Administrators can share data types." });
            }
            // Admins can share data with any user as if they owned that data.
            if (roles.Contains(CustomRoles.Admin)
                || claims.Contains(new Claim(CustomClaimTypes.PermissionDataOwner, $"{dataType}{{{id}}}")))
            {
                // Permissions other than view/edit can only be shared for an entire type.
                if (!string.IsNullOrEmpty(id) && operationLower != "view" && operationLower != "edit")
                {
                    return Json(new { error = "Only view or edit permission can be shared." });
                }
            }
            else
            {
                return Json(new { error = "Only the original owner of a data item may share it." });
            }

            var targetUser = await _userManager.FindByNameAsync(username);
            if (targetUser == null)
            {
                return Json(new { error = "That user could not be found." });
            }
            if (!TryGetClaim(dataType, operation, id, out Claim claim))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            List<Claim> newClaims = new List<Claim> { claim };
            // Add implied permissions.
            if (claim.Type == CustomClaimTypes.PermissionDataAdd)
            {
                newClaims.Add(new Claim(CustomClaimTypes.PermissionDataEdit, claim.Value));
                newClaims.Add(new Claim(CustomClaimTypes.PermissionDataView, claim.Value));
            }
            else if (claim.Type == CustomClaimTypes.PermissionDataEdit)
            {
                newClaims.Add(new Claim(CustomClaimTypes.PermissionDataView, claim.Value));
            }
            await _userManager.AddClaimsAsync(targetUser, newClaims);
            return Ok();
        }

        [HttpPost("{group}")]
        public async Task<IActionResult> StartNewGroup(string group)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = $"Your account has been locked. Please contact an administrator at {_adminOptions.AdminEmailAddress} for assistance." });
            }
            if (group.ToLower().Contains("administrator"))
            {
                return Json(new { error = "To avoid confusion with the official Administrator group, group names may not contain 'administrator.'" });
            }
            var groupRole = await _roleManager.FindByNameAsync(group);
            if (groupRole != null)
            {
                return Json(new { error = "A group with that name already exists." });
            }
            var role = new IdentityRole(group);
            await _roleManager.CreateAsync(role);
            await _userManager.AddToRoleAsync(user, group);
            await _userManager.AddClaimAsync(user, new Claim(CustomClaimTypes.PermissionGroupManager, group));
            return Ok();
        }

        [HttpPost("{username}/{group}")]
        public async Task<IActionResult> TransferManagerToUser(string username, string group)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = $"Your account has been locked. Please contact an administrator at {_adminOptions.AdminEmailAddress} for assistance." });
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (group == CustomRoles.SiteAdmin || group == CustomRoles.Admin)
            {
                if (roles.Contains(CustomRoles.SiteAdmin))
                {
                    return Json(new { error = "You cannot transfer the Site Administrator role in this way. There is a special action for doing so." });
                }
                else
                {
                    return Json(new { error = "The Administrator group does not have a manager." });
                }
            }
            // Admins can transfer the manager role of any non-admin group, regardless of membership.
            else if (!roles.Contains(CustomRoles.Admin))
            {
                var claims = await _userManager.GetClaimsAsync(user);
                if (!claims.Contains(new Claim(CustomClaimTypes.PermissionGroupManager, group)))
                {
                    return Json(new { error = "Only the group's manager may transfer the membership role." });
                }
            }

            var groupRole = await _roleManager.FindByNameAsync(group);
            if (groupRole == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var targetUser = await _userManager.FindByNameAsync(username);
            if (targetUser == null)
            {
                return Json(new { error = "That user could not be found." });
            }
            var member = await _userManager.IsInRoleAsync(targetUser, group);
            if (!member)
            {
                // An admin may transfer the membership role of a group to anyone, even a user who
                // was not previously a member of that group. Doing so automatically adds the user to
                // the group.
                if (roles.Contains(CustomRoles.Admin))
                {
                    await _userManager.AddToRoleAsync(targetUser, group);
                }
                // The manager may only transfer the membership role to an existing member of the group.
                else
                {
                    return Json(new { error = "You may only transfer the membership role to a member of the group." });
                }
            }
            await _userManager.AddClaimAsync(targetUser, new Claim(CustomClaimTypes.PermissionGroupManager, group));
            await _userManager.RemoveClaimAsync(user, new Claim(CustomClaimTypes.PermissionGroupManager, group));
            return Ok();
        }

        [HttpPost("{username}")]
        public async Task<IActionResult> TransferSiteAdminToUser(string username)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = $"Your account has been locked. Please contact an administrator at {_adminOptions.AdminEmailAddress} for assistance." });
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(CustomRoles.SiteAdmin))
            {
                return Json(new { error = "Only the current Site Administrator may transfer the Site Administrator role." });
            }

            var targetUser = await _userManager.FindByNameAsync(username);
            if (targetUser == null)
            {
                return Json(new { error = "That user could not be found." });
            }
            await _userManager.AddToRoleAsync(targetUser, CustomRoles.SiteAdmin);
            await _userManager.RemoveFromRoleAsync(user, CustomRoles.SiteAdmin);
            return Ok();
        }

        private bool TryGetClaim(string dataType, string operation, string id, out Claim claim)
        {
            claim = null;
            if (string.IsNullOrEmpty(dataType))
            {
                return false;
            }
            dataType = dataType.ToInitialCaps();
            // Entity isn't used, but is parsed to enure it's valid.
            var entity = _context.Model.GetEntityTypes().FirstOrDefault(e => e.Name.Substring(e.Name.LastIndexOf('.') + 1) == dataType);
            if (entity == null)
            {
                return false;
            }

            // Permission on the data type itself, rather than an item.
            if (string.IsNullOrEmpty(id))
            {
                // All permissions on the data type.
                if (string.IsNullOrEmpty(operation))
                {
                    claim = new Claim(CustomClaimTypes.PermissionDataAll, dataType);
                    return true;
                }
                // Permission for a specific operation on the data type.
                else
                {
                    claim = new Claim($"permission/data/{operation.ToLower()}", dataType);
                    return true;
                }
            }
            // Permission on a specific item.
            else
            {
                // The Guid isn't used, but it is parsed to ensure it's valid.
                if (!Guid.TryParse(id, out Guid guid))
                {
                    return false;
                }
                // All permissions on the item.
                if (string.IsNullOrEmpty(operation))
                {
                    claim = new Claim(CustomClaimTypes.PermissionDataAll, $"{dataType}{{{id}}}");
                    return true;
                }
                // Permission for a specific operation on the item.
                else
                {
                    claim = new Claim(operation, $"{dataType}{{{id}}}");
                    return true;
                }
            }
        }
    }
}
