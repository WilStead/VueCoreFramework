using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VueCoreFramework.Data;
using VueCoreFramework.Extensions;
using VueCoreFramework.Models;
using VueCoreFramework.Services;
using System;
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
            vm.ManagedGroups = claims.Where(c => c.Type == CustomClaimTypes.PermissionGroupManager).Select(c => c.Value).ToList();

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
        public async Task<IActionResult> Authorize(string dataType, string operation = CustomClaimTypes.PermissionDataView, string id = null)
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
            vm.ManagedGroups = claims.Where(c => c.Type == CustomClaimTypes.PermissionGroupManager).Select(c => c.Value).ToList();

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
                else if (!string.IsNullOrEmpty(id) && vm.ManagedGroups.Count > 0)
                {
                    vm.CanShare = AuthorizationViewModel.ShareGroup;
                }
            }

            return Json(vm);
        }

        [HttpPost("{username}/{group}")]
        public async Task<IActionResult> AddUserToGroup(string username, string group)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(new { error = ErrorMessages.InvalidUserError });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = ErrorMessages.LockedAccount(_adminOptions.AdminEmailAddress) });
            }
            if (username == user.UserName)
            {
                return Json(new { error = ErrorMessages.SelfGroupAddError });
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (group == CustomRoles.SiteAdmin)
            {
                if (roles.Contains(CustomRoles.SiteAdmin))
                {
                    return Json(new { error = ErrorMessages.SiteAdminSingularError });
                }
                else
                {
                    return Json(new { error = ErrorMessages.SiteAdminOnlyError });
                }
            }
            else if (group == CustomRoles.Admin)
            {
                if (!roles.Contains(CustomRoles.SiteAdmin))
                {
                    return Json(new { error = ErrorMessages.SiteAdminOnlyError });
                }
            }
            // Admins can add users to any non-admin group, regardless of their own membership.
            else if (!roles.Contains(CustomRoles.Admin))
            {
                var claims = await _userManager.GetClaimsAsync(user);
                if (!claims.Contains(new Claim(CustomClaimTypes.PermissionGroupManager, group)))
                {
                    return Json(new { error = ErrorMessages.ManagerOnlyError });
                }
            }

            var groupRole = await _roleManager.FindByNameAsync(group);
            if (groupRole == null)
            {
                return Json(new { error = ErrorMessages.InvalidTargetGroupError });
            }
            var targetUser = await _userManager.FindByNameAsync(username);
            if (targetUser == null)
            {
                return Json(new { error = ErrorMessages.InvalidTargetUserError });
            }
            await _userManager.AddToRoleAsync(targetUser, groupRole.Name);
            return Json(new { Response = ResponseMessages.Success });
        }

        internal static string GetAuthorization(IList<Claim> claims, string dataType, string claimType = CustomClaimTypes.PermissionDataView, string id = null)
        {
            // First authorization for all data is checked.
            if (claims.Any(c => c.Type == CustomClaimTypes.PermissionDataAll && c.Value == CustomClaimTypes.PermissionAll))
            {
                return CustomClaimTypes.PermissionDataAll;
            }

            // If not authorized for all data, authorization for the specific operation on all data is checked.
            // In the absence of a specific operation, the default action is View.
            var claim = GetHighestClaimForValue(claims, CustomClaimTypes.PermissionAll);
            if (claim != null && PermissionIncludesTarget(claim.Type, claimType))
            {
                return claim.Type;
            }

            // If not authorized for the operation on all data, authorization for the specific data type is checked.
            claim = GetHighestClaimForValue(claims, dataType);
            if (claim != null && PermissionIncludesTarget(claim.Type, claimType))
            {
                return claim.Type;
            }

            // If not authorized for the operation on the data type and an id is provided, the specific item is checked.
            if (!string.IsNullOrEmpty(id))
            {
                // Authorization for either all operations or the specific operation is checked.
                claim = GetHighestClaimForValue(claims, $"{dataType}{{{id}}}");
                if (claim != null && PermissionIncludesTarget(claim.Type, claimType))
                {
                    return claim.Type;
                }
            }
            // If no id is provided, only View is allowed (to permit data table display, even if no items can be listed).
            else if (claimType == CustomClaimTypes.PermissionDataView)
            {
                return CustomClaimTypes.PermissionDataView;
            }

            // No authorizations found.
            return AuthorizationViewModel.Unauthorized;
        }

        [HttpGet("{dataType}")]
        public async Task<IActionResult> GetCurrentShares(string dataType, string id = null)
        {
            if (string.IsNullOrEmpty(dataType))
            {
                return Json(new { error = ErrorMessages.InvalidDataTypeError });
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.AdminLocked)
            {
                return Json(new { error = ErrorMessages.InvalidUserError });
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

            var shares = new List<ShareViewModel>();
            // Admins can control all sharing, and all users can control sharing on their owned items.
            if (roles.Any(r => r == CustomRoles.Admin)
                || (!string.IsNullOrEmpty(id)
                && claims.Any(c => c.Type == CustomClaimTypes.PermissionDataOwner
                && c.Value == $"{dataType}{{{id}}}")))
            {
                foreach (var claim in _context.UserClaims
                    .Where(c => c.ClaimValue == (string.IsNullOrEmpty(id) ? $"{dataType}" : $"{dataType}{{{id}}}")))
                {
                    var shareUser = await _userManager.FindByIdAsync(claim.UserId);
                    shares.Add(new ShareViewModel
                    {
                        Type = "user",
                        Name = shareUser.UserName,
                        Level = claim.ClaimType
                    });
                }
                foreach (var claim in _context.RoleClaims
                    .Where(c => c.ClaimValue == (string.IsNullOrEmpty(id) ? $"{dataType}" : $"{dataType}{{{id}}}")))
                {
                    var shareRole = await _roleManager.FindByIdAsync(claim.RoleId);
                    shares.Add(new ShareViewModel
                    {
                        Type = "group",
                        Name = shareRole.Name,
                        Level = claim.ClaimType
                    });
                }
            }
            else
            {
                // Managers can control all sharing for their group, even if they don't own it.
                foreach (var group in claims.Where(c =>
                    c.Type == CustomClaimTypes.PermissionGroupManager).Select(c => c.Value))
                {
                    var role = await _roleManager.FindByNameAsync(group);
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    foreach (var claim in roleClaims.Where(c =>
                        c.Value == (string.IsNullOrEmpty(id) ? $"{dataType}" : $"{dataType}{{{id}}}")))
                    {
                        shares.Add(new ShareViewModel
                        {
                            Type = "group",
                            Name = role.Name,
                            Level = claim.Type
                        });
                    }
                }
            }
            return Json(shares);
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

        private static IList<Claim> GetImpliedClaimsForRemove(Claim claim)
        {
            if (claim.Type == CustomClaimTypes.PermissionDataView)
            {
                return new List<Claim> {
                    claim,
                    new Claim(CustomClaimTypes.PermissionDataEdit, claim.Value),
                    new Claim(CustomClaimTypes.PermissionDataAdd, claim.Value),
                    new Claim(CustomClaimTypes.PermissionDataAll, claim.Value)
                };
            }
            else if (claim.Type == CustomClaimTypes.PermissionDataEdit)
            {
                return new List<Claim> {
                    claim,
                    new Claim(CustomClaimTypes.PermissionDataAdd, claim.Value),
                    new Claim(CustomClaimTypes.PermissionDataAll, claim.Value)
                };
            }
            else if (claim.Type == CustomClaimTypes.PermissionDataAdd)
            {
                return new List<Claim> {
                    claim,
                    new Claim(CustomClaimTypes.PermissionDataAll, claim.Value)
                };
            }
            else return new List<Claim> { claim };
        }

        [HttpGet("{input}")]
        public async Task<IActionResult> GetShareableGroupCompletion(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return Json(new { response = "" });
            }

            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.AdminLocked)
            {
                return Json(new { error = ErrorMessages.InvalidUserError });
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

            // First try groups the user manages.
            var potentialClaim = claims.FirstOrDefault(c =>
                c.Type == CustomClaimTypes.PermissionGroupManager && c.Value.StartsWith(input));
            if (potentialClaim != null)
            {
                return Json(new { response = potentialClaim.Value });
            }

            // Next try groups to which the user belongs.
            var potentialValue = roles.FirstOrDefault(r =>
                r.StartsWith(input)
                && r != CustomRoles.Admin && r != CustomRoles.SiteAdmin && r != CustomRoles.AllUsers);

            if (!string.IsNullOrEmpty(potentialValue))
            {
                return Json(new { response = potentialValue });
            }

            // Admins can share with any group, so finally try everything.
            if (roles.Any(r => r == CustomRoles.Admin))
            {
                var potentialRole = _context.Roles.FirstOrDefault(r => r.Name.StartsWith(input));
                if (potentialRole != null)
                {
                    return Json(new { response = potentialRole.Name });
                }
            }

            return Json(new { response = "" });
        }

        [HttpGet("{input}")]
        public async Task<IActionResult> GetShareableUsernameCompletion(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return Json(new { response = "" });
            }

            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.AdminLocked)
            {
                return Json(new { error = ErrorMessages.InvalidUserError });
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

            // First try members of groups the user manages.
            foreach (var group in claims.Where(c => c.Type == CustomClaimTypes.PermissionGroupManager).Select(c => c.Value))
            {
                var members = await _userManager.GetUsersInRoleAsync(group);
                var potentialMember = members.FirstOrDefault(u => u != user && u.UserName.StartsWith(input));
                if (potentialMember != null)
                {
                    return Json(new { response = potentialMember.UserName });
                }
            }

            // Next try members of groups to which the user belongs.
            foreach (var group in roles.Where(r => r != CustomRoles.SiteAdmin && r != CustomRoles.AllUsers))
            {
                var members = await _userManager.GetUsersInRoleAsync(group);
                var potentialMember = members.FirstOrDefault(u => u != user && u.UserName.StartsWith(input));
                if (potentialMember != null)
                {
                    return Json(new { response = potentialMember.UserName });
                }
            }

            // Admins can share with anyone, so finally try everything.
            if (roles.Any(r => r == CustomRoles.Admin))
            {
                var potentialUser = _context.Users.FirstOrDefault(u => u.UserName.StartsWith(input));
                if (potentialUser != null)
                {
                    return Json(new { response = potentialUser.UserName });
                }
            }

            return Json(new { response = "" });
        }

        [HttpGet]
        public async Task<IActionResult> GetShareableGroupMembers()
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.AdminLocked)
            {
                return Json(new { error = ErrorMessages.InvalidUserError });
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

            var members = new List<string>();
            // Add members of any groups the user manages, or of which the user is a member.
            var managedGroups = claims.Where(c =>
                c.Type == CustomClaimTypes.PermissionGroupManager).Select(c => c.Value);
            foreach (var group in managedGroups.Concat(roles.Where(r => !managedGroups.Contains(r))))
            {
                var groupMembers = await _userManager.GetUsersInRoleAsync(group);
                members.AddRange(groupMembers.Where(m => m != user).Select(m => m.UserName));
            }

            return Json(members);
        }

        [HttpGet]
        public async Task<IActionResult> GetShareableGroupSubset()
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.AdminLocked)
            {
                return Json(new { error = ErrorMessages.InvalidUserError });
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

            // First add any groups the user manages.
            var managedGroups = claims.Where(c => c.Type == CustomClaimTypes.PermissionGroupManager).Take(5).Select(c => c.Value).ToList();

            // If the target number has not yet been reached, also add any groups to which the user belongs.
            if (managedGroups.Count < 5)
            {
                managedGroups = managedGroups.Concat(roles.Where(r =>
                    !managedGroups.Contains(r) && r != CustomRoles.Admin
                    && r != CustomRoles.SiteAdmin && r != CustomRoles.AllUsers)
                    .Take(5 - managedGroups.Count)).ToList();
            }

            // Admins can share with any group, so if the target still hasn't been reached it can be
            // filled with arbitrarily selected ones.
            if (managedGroups.Count < 5 && roles.Any(r => r == CustomRoles.Admin))
            {
                managedGroups = managedGroups.Concat(_context.Roles.Where(r =>
                    !managedGroups.Contains(r.Name) && r.Name != CustomRoles.Admin
                    && r.Name != CustomRoles.SiteAdmin && r.Name != CustomRoles.AllUsers)
                    .Take(5 - managedGroups.Count).Select(r => r.Name)).ToList();
            }

            return Json(managedGroups);
        }

        [HttpPost("{dataType}")]
        public async Task<IActionResult> HideDataFromAll(string dataType, string operation, string id)
        {
            if (string.IsNullOrEmpty(dataType))
            {
                return Json(new { error = ErrorMessages.InvalidDataTypeError });
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(new { error = ErrorMessages.InvalidUserError });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = ErrorMessages.LockedAccount(_adminOptions.AdminEmailAddress) });
            }
            var roles = await _userManager.GetRolesAsync(user);
            // Only Admins can hide data from all.
            if (!roles.Contains(CustomRoles.Admin))
            {
                return Json(new { error = ErrorMessages.AdminOnlyError });
            }

            dataType = dataType.ToInitialCaps();
            // Entity isn't used, but is parsed to enure it's valid.
            var entity = _context.Model.GetEntityTypes().FirstOrDefault(e => e.Name.Substring(e.Name.LastIndexOf('.') + 1) == dataType);
            if (entity == null)
            {
                return Json(new { error = ErrorMessages.InvalidDataTypeError });
            }
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
            foreach (var impliedClaim in GetImpliedClaimsForRemove(claim))
            {
                await _roleManager.RemoveClaimAsync(allRole, impliedClaim);
            }
            return Json(new { Response = ResponseMessages.Success });
        }

        [HttpPost("{group}/{dataType}")]
        public async Task<IActionResult> HideDataFromGroup(string group, string dataType, string operation, string id)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(new { error = ErrorMessages.InvalidUserError });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = ErrorMessages.LockedAccount(_adminOptions.AdminEmailAddress) });
            }
            var roles = await _userManager.GetRolesAsync(user);
            // Only admins can hide a data type, rather than a particular item
            if (string.IsNullOrEmpty(id) && !roles.Contains(CustomRoles.Admin))
            {
                return Json(new { error = ErrorMessages.AdminOnlyError });
            }
            // Admins can hide data from any group, regardless of their own membership.
            if (!roles.Contains(CustomRoles.Admin))
            {
                var claims = await _userManager.GetClaimsAsync(user);
                if (!claims.Contains(new Claim(CustomClaimTypes.PermissionGroupManager, group))
                    && !claims.Contains(new Claim(CustomClaimTypes.PermissionDataOwner, $"{dataType}{{{id}}}")))
                {
                    return Json(new { error = ErrorMessages.ManagerOrOwnerOnlyError });
                }
            }

            var groupRole = await _roleManager.FindByNameAsync(group);
            if (groupRole == null)
            {
                return Json(new { error = ErrorMessages.InvalidTargetGroupError });
            }
            if (!TryGetClaim(dataType, operation, id, out Claim claim))
            {
                return Json(new { error = ErrorMessages.DataError });
            }
            foreach (var impliedClaim in GetImpliedClaimsForRemove(claim))
            {
                await _roleManager.RemoveClaimAsync(groupRole, impliedClaim);
            }
            return Json(new { Response = ResponseMessages.Success });
        }

        [HttpPost("{username}/{dataType}")]
        public async Task<IActionResult> HideDataFromUser(string username, string dataType, string operation, string id)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(new { error = ErrorMessages.InvalidUserError });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = ErrorMessages.LockedAccount(_adminOptions.AdminEmailAddress) });
            }
            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            // Only admins can hide a data type, rather than a particular item
            if (string.IsNullOrEmpty(id) && !roles.Contains(CustomRoles.Admin))
            {
                return Json(new { error = ErrorMessages.AdminOnlyError });
            }
            if (!roles.Contains(CustomRoles.Admin)
                && !claims.Contains(new Claim(CustomClaimTypes.PermissionDataOwner, $"{dataType}{{{id}}}")))
            {
                return Json(new { error = ErrorMessages.OwnerOnlyError });
            }

            var targetUser = await _userManager.FindByNameAsync(username);
            if (targetUser == null)
            {
                return Json(new { error = ErrorMessages.InvalidTargetUserError });
            }
            if (!TryGetClaim(dataType, operation, id, out Claim claim))
            {
                return Json(new { error = ErrorMessages.DataError });
            }
            await _userManager.RemoveClaimsAsync(targetUser, GetImpliedClaimsForRemove(claim));
            return Json(new { Response = ResponseMessages.Success });
        }

        private static bool PermissionIncludesTarget(string permission, string targetPermission)
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

        [HttpPost("{username}/{group}")]
        public async Task<IActionResult> RemoveUserFromGroup(string username, string group)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(new { error = ErrorMessages.InvalidUserError });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = ErrorMessages.LockedAccount(_adminOptions.AdminEmailAddress) });
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (group == CustomRoles.SiteAdmin)
            {
                if (roles.Contains(CustomRoles.SiteAdmin))
                {
                    return Json(new { error = ErrorMessages.SiteAdminSingularError });
                }
                else
                {
                    return Json(new { error = ErrorMessages.SiteAdminOnlyError });
                }
            }
            else if (group == CustomRoles.Admin)
            {
                if (!roles.Contains(CustomRoles.SiteAdmin))
                {
                    return Json(new { error = ErrorMessages.SiteAdminOnlyError });
                }
            }
            // Admins can remove users from any non-admin group, regardless of their own membership.
            else if (!roles.Contains(CustomRoles.Admin))
            {
                var claims = await _userManager.GetClaimsAsync(user);
                if (!claims.Contains(new Claim(CustomClaimTypes.PermissionGroupManager, group)))
                {
                    return Json(new { error = ErrorMessages.ManagerOnlyError });
                }
            }

            var groupRole = await _roleManager.FindByNameAsync(group);
            if (groupRole == null)
            {
                return Json(new { error = ErrorMessages.InvalidTargetGroupError });
            }
            var targetUser = await _userManager.FindByNameAsync(username);
            if (targetUser == null)
            {
                return Json(new { error = ErrorMessages.InvalidTargetUserError });
            }
            await _userManager.RemoveFromRoleAsync(targetUser, groupRole.Name);
            return Json(new { Response = ResponseMessages.Success });
        }

        [HttpPost("{dataType}")]
        public async Task<IActionResult> ShareDataWithAll(string dataType, string operation, string id)
        {
            if (string.IsNullOrEmpty(dataType))
            {
                return Json(new { error = ErrorMessages.InvalidDataTypeError });
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(new { error = ErrorMessages.InvalidUserError });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = ErrorMessages.LockedAccount(_adminOptions.AdminEmailAddress) });
            }
            var roles = await _userManager.GetRolesAsync(user);
            // Only Admins can share data with all.
            if (!roles.Contains(CustomRoles.Admin))
            {
                return Json(new { error = ErrorMessages.AdminOnlyError });
            }
            if (operation != CustomClaimTypes.PermissionDataView
                && operation != CustomClaimTypes.PermissionDataEdit)
            {
                return Json(new { error = ErrorMessages.ViewEditOnlyError });
            }

            dataType = dataType.ToInitialCaps();
            // Entity isn't used, but is parsed to enure it's valid.
            var entity = _context.Model.GetEntityTypes().FirstOrDefault(e => e.Name.Substring(e.Name.LastIndexOf('.') + 1) == dataType);
            if (entity == null)
            {
                return Json(new { error = ErrorMessages.InvalidDataTypeError });
            }
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
                claim = new Claim(operation, $"{dataType}{{{id}}}");
            }
            await _roleManager.AddClaimAsync(allRole, claim);
            return Json(new { Response = ResponseMessages.Success });
        }

        [HttpPost("{group}/{dataType}")]
        public async Task<IActionResult> ShareDataWithGroup(string group, string dataType, string operation, string id)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(new { error = ErrorMessages.InvalidUserError });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = ErrorMessages.LockedAccount(_adminOptions.AdminEmailAddress) });
            }
            var groupRole = await _roleManager.FindByNameAsync(group);
            if (groupRole == null)
            {
                return Json(new { error = ErrorMessages.InvalidTargetGroupError });
            }
            if (!TryGetClaim(dataType, operation, id, out Claim claim))
            {
                return Json(new { error = ErrorMessages.DataError });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            // Only admins can share a data type, rather than a particular item
            if (string.IsNullOrEmpty(id) && !roles.Contains(CustomRoles.Admin))
            {
                return Json(new { error = ErrorMessages.AdminOnlyError });
            }
            // Admins can share data with any group as if they owned that data, regardless of their own membership.
            if (roles.Contains(CustomRoles.Admin)
                || claims.Contains(new Claim(CustomClaimTypes.PermissionDataOwner, $"{dataType}{{{id}}}")))
            {
                // Permissions other than view/edit can only be shared for an entire type.
                if (!string.IsNullOrEmpty(id)
                    && operation != CustomClaimTypes.PermissionDataView
                    && operation != CustomClaimTypes.PermissionDataEdit)
                {
                    return Json(new { error = ErrorMessages.ViewEditOnlyError });
                }
            }
            else
            {
                // Managers of groups can re-share data with their group which has been shared with them.
                if (claims.Contains(new Claim(CustomClaimTypes.PermissionGroupManager, group)))
                {
                    // If the manager has edit permission, the manager can also share view permission.
                    if (!claims.Contains(claim) &&
                        (operation != CustomClaimTypes.PermissionDataView
                        || !claims.Contains(new Claim(CustomClaimTypes.PermissionDataEdit, $"{dataType}{{{id}}}"))))
                    {
                        return Json(new { error = ErrorMessages.ManagerOnlySharedError });
                    }
                }
                else
                {
                    return Json(new { error = ErrorMessages.ManagerOrOwnerOnlyError });
                }
            }

            await _roleManager.AddClaimAsync(groupRole, claim);
            return Json(new { Response = ResponseMessages.Success });
        }

        [HttpPost("{username}/{dataType}")]
        public async Task<IActionResult> ShareDataWithUser(string username, string dataType, string operation, string id)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(new { error = ErrorMessages.InvalidUserError });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = ErrorMessages.LockedAccount(_adminOptions.AdminEmailAddress) });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            // Only admins can share a data type, rather than a particular item
            if (string.IsNullOrEmpty(id) && !roles.Contains(CustomRoles.Admin))
            {
                return Json(new { error = ErrorMessages.AdminOnlyError });
            }
            // Admins can share data with any user as if they owned that data.
            if (roles.Contains(CustomRoles.Admin)
                || claims.Contains(new Claim(CustomClaimTypes.PermissionDataOwner, $"{dataType}{{{id}}}")))
            {
                // Permissions other than view/edit can only be shared for an entire type.
                if (!string.IsNullOrEmpty(id)
                    && operation != CustomClaimTypes.PermissionDataView
                    && operation != CustomClaimTypes.PermissionDataEdit)
                {
                    return Json(new { error = ErrorMessages.ViewEditOnlyError });
                }
            }
            else
            {
                return Json(new { error = ErrorMessages.OwnerOnlyError });
            }

            var targetUser = await _userManager.FindByNameAsync(username);
            if (targetUser == null)
            {
                return Json(new { error = ErrorMessages.InvalidTargetUserError });
            }
            if (!TryGetClaim(dataType, operation, id, out Claim claim))
            {
                return Json(new { error = ErrorMessages.DataError });
            }
            await _userManager.AddClaimAsync(targetUser, claim);
            return Json(new { Response = ResponseMessages.Success });
        }

        [HttpPost("{group}")]
        public async Task<IActionResult> StartNewGroup(string group)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(new { error = ErrorMessages.InvalidUserError });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = ErrorMessages.LockedAccount(_adminOptions.AdminEmailAddress) });
            }
            if (group.ToLower().Contains("administrator"))
            {
                return Json(new { error = ErrorMessages.OnlyAdminCanBeAdminError });
            }
            var groupRole = await _roleManager.FindByNameAsync(group);
            if (groupRole != null)
            {
                return Json(new { error = ErrorMessages.DuplicateGroupNameError });
            }
            var role = new IdentityRole(group);
            await _roleManager.CreateAsync(role);
            await _userManager.AddToRoleAsync(user, group);
            await _userManager.AddClaimAsync(user, new Claim(CustomClaimTypes.PermissionGroupManager, group));
            return Json(new { Response = ResponseMessages.Success });
        }

        [HttpPost("{username}/{group}")]
        public async Task<IActionResult> TransferManagerToUser(string username, string group)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(new { error = ErrorMessages.InvalidUserError });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = ErrorMessages.LockedAccount(_adminOptions.AdminEmailAddress) });
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (group == CustomRoles.SiteAdmin || group == CustomRoles.Admin)
            {
                return Json(new { error = ErrorMessages.AdminNoManagerError });
            }
            // Admins can transfer the manager role of any non-admin group, regardless of membership.
            else if (!roles.Contains(CustomRoles.Admin))
            {
                var claims = await _userManager.GetClaimsAsync(user);
                if (!claims.Contains(new Claim(CustomClaimTypes.PermissionGroupManager, group)))
                {
                    return Json(new { error = ErrorMessages.ManagerOnlyError });
                }
            }

            var groupRole = await _roleManager.FindByNameAsync(group);
            if (groupRole == null)
            {
                return Json(new { error = ErrorMessages.InvalidTargetGroupError });
            }
            var targetUser = await _userManager.FindByNameAsync(username);
            if (targetUser == null)
            {
                return Json(new { error = ErrorMessages.InvalidTargetUserError });
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
                    return Json(new { error = ErrorMessages.GroupMemberOnlyError });
                }
            }
            await _userManager.AddClaimAsync(targetUser, new Claim(CustomClaimTypes.PermissionGroupManager, group));
            await _userManager.RemoveClaimAsync(user, new Claim(CustomClaimTypes.PermissionGroupManager, group));
            return Json(new { Response = ResponseMessages.Success });
        }

        [HttpPost("{username}")]
        public async Task<IActionResult> TransferSiteAdminToUser(string username)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(new { error = ErrorMessages.InvalidUserError });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = ErrorMessages.LockedAccount(_adminOptions.AdminEmailAddress) });
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(CustomRoles.SiteAdmin))
            {
                return Json(new { error = ErrorMessages.SiteAdminOnlyError });
            }

            var targetUser = await _userManager.FindByNameAsync(username);
            if (targetUser == null)
            {
                return Json(new { error = ErrorMessages.InvalidTargetUserError });
            }
            await _userManager.AddToRoleAsync(targetUser, CustomRoles.SiteAdmin);
            await _userManager.RemoveFromRoleAsync(user, CustomRoles.SiteAdmin);
            return Json(new { Response = ResponseMessages.Success });
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
                    claim = new Claim(operation, dataType);
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
