using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VueCoreFramework.API.ViewModels;
using VueCoreFramework.Core.Configuration;
using VueCoreFramework.Core.Data;
using VueCoreFramework.Core.Data.Identity;
using VueCoreFramework.Core.Extensions;
using VueCoreFramework.Core.Messages;
using VueCoreFramework.Core.Models;

namespace VueCoreFramework.Controllers
{
    /// <summary>
    /// An API controller for handling sharing tasks.
    /// </summary>
    [Authorize]
    public class ShareController : Controller
    {
        private readonly AdminOptions _adminOptions;
        private readonly ApplicationDbContext _context;
        private readonly IStringLocalizer<ErrorMessages> _errorLocalizer;
        private readonly IStringLocalizer<EmailMessages> _emailLocalizer;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Initializes a new instance of <see cref="ShareController"/>.
        /// </summary>
        public ShareController(
            IOptions<AdminOptions> adminOptions,
            ApplicationDbContext context,
            IStringLocalizer<ErrorMessages> errorLocalizer,
            IStringLocalizer<EmailMessages> emailLocalizer,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _adminOptions = adminOptions.Value;
            _context = context;
            _errorLocalizer = errorLocalizer;
            _emailLocalizer = emailLocalizer;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        /// <summary>
        /// Called to retrieve the list of users and groups with whom the given data is current shared.
        /// </summary>
        /// <param name="dataType">The type of data being shared.</param>
        /// <param name="id">Optionally, the primary key of a specific item being shared.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">A list of <see cref="ShareViewModel"/>s.</response>
        [HttpGet("{dataType}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, object>), 200)]
        public async Task<IActionResult> GetCurrentShares(string dataType, string id = null)
        {
            if (string.IsNullOrEmpty(dataType))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
            }
            var user = await _userManager.FindByIdAsync(HttpContext.User.FindFirstValue(JwtClaimTypes.Subject));
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
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

        /// <summary>
        /// Called to find a group name which starts with the given input. Only groups the current
        /// user has access to will be returned.
        /// </summary>
        /// <param name="input">A search string.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="404">Not found.</response>
        /// <response code="200">The first matched name.</response>
        [HttpGet("{input}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 200)]
        public async Task<IActionResult> GetShareableGroupCompletion(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(HttpContext.User.FindFirstValue(JwtClaimTypes.Subject));
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
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
                return Ok(potentialClaim.Value);
            }

            // Next try groups to which the user belongs.
            var potentialValue = roles.FirstOrDefault(r =>
                r.StartsWith(input)
                && r != CustomRoles.Admin && r != CustomRoles.SiteAdmin && r != CustomRoles.AllUsers);

            if (!string.IsNullOrEmpty(potentialValue))
            {
                return Ok(potentialValue);
            }

            // Admins can share with any group, so finally try everything.
            if (roles.Any(r => r == CustomRoles.Admin))
            {
                var potentialRole = _context.Roles.FirstOrDefault(r => r.Name.StartsWith(input));
                if (potentialRole != null)
                {
                    return Ok(potentialRole.Name);
                }
            }

            return NotFound();
        }

        /// <summary>
        /// Called to find a username which starts with the given input. Only names of users the
        /// current user has access to will be returned.
        /// </summary>
        /// <param name="input">A search string.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="404">Not found.</response>
        /// <response code="200">The first matched name.</response>
        [HttpGet("{input}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 200)]
        public async Task<IActionResult> GetShareableUsernameCompletion(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(HttpContext.User.FindFirstValue(JwtClaimTypes.Subject));
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
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
                    return Ok(potentialMember.UserName);
                }
            }

            // Next try members of groups to which the user belongs.
            foreach (var group in roles.Where(r => r != CustomRoles.SiteAdmin && r != CustomRoles.AllUsers))
            {
                var members = await _userManager.GetUsersInRoleAsync(group);
                var potentialMember = members.FirstOrDefault(u => u != user && u.UserName.StartsWith(input));
                if (potentialMember != null)
                {
                    return Ok(potentialMember.UserName);
                }
            }

            // Admins can share with anyone, so finally try everything.
            if (roles.Any(r => r == CustomRoles.Admin))
            {
                var potentialUser = _context.Users.FirstOrDefault(u => u.UserName.StartsWith(input));
                if (potentialUser != null)
                {
                    return Ok(potentialUser.UserName);
                }
            }

            return NotFound();
        }

        /// <summary>
        /// Called to retrieve the list of users with whom the current user may share.
        /// </summary>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">The list of users.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 200)]
        public async Task<IActionResult> GetShareableGroupMembers()
        {
            var user = await _userManager.FindByIdAsync(HttpContext.User.FindFirstValue(JwtClaimTypes.Subject));
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
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

        /// <summary>
        /// Called to retrieve a subset of the list of groups with whom the current user may share.
        /// </summary>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">The list of group names.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 200)]
        public async Task<IActionResult> GetShareableGroupSubset()
        {
            var user = await _userManager.FindByIdAsync(HttpContext.User.FindFirstValue(JwtClaimTypes.Subject));
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
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

        /// <summary>
        /// Called to stop the given data from being shared with all users.
        /// </summary>
        /// <param name="dataType">The type of data being shared.</param>
        /// <param name="operation">Optionally, an operation whose permission is being removed.</param>
        /// <param name="id">Optionally, the primary key of a specific item being shared.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost("{dataType}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> HideDataFromAll(string dataType, string operation, string id)
        {
            if (string.IsNullOrEmpty(dataType))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
            }
            var user = await _userManager.FindByIdAsync(HttpContext.User.FindFirstValue(JwtClaimTypes.Subject));
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            var roles = await _userManager.GetRolesAsync(user);
            // Only Admins can hide data from all.
            if (!roles.Contains(CustomRoles.Admin))
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.AdminOnlyError]);
            }

            dataType = dataType.ToInitialCaps();
            // Entity isn't used, but is parsed to enure it's valid.
            var entity = _context.Model.GetEntityTypes().FirstOrDefault(e => e.Name.Substring(e.Name.LastIndexOf('.') + 1) == dataType);
            if (entity == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
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
            return Ok();
        }

        /// <summary>
        /// Called to stop the given data from being shared with the given group.
        /// </summary>
        /// <param name="group">The name of the group with whom the data is being shared.</param>
        /// <param name="dataType">The type of data being shared.</param>
        /// <param name="operation">Optionally, an operation whose permission is being removed.</param>
        /// <param name="id">Optionally, the primary key of a specific item being shared.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost("{group}/{dataType}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> HideDataFromGroup(string group, string dataType, string operation, string id)
        {
            var user = await _userManager.FindByIdAsync(HttpContext.User.FindFirstValue(JwtClaimTypes.Subject));
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            var roles = await _userManager.GetRolesAsync(user);
            // Only admins can hide a data type, rather than a particular item
            if (string.IsNullOrEmpty(id) && !roles.Contains(CustomRoles.Admin))
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.AdminOnlyError]);
            }
            // Admins can hide data from any group, regardless of their own membership.
            if (!roles.Contains(CustomRoles.Admin))
            {
                var claims = await _userManager.GetClaimsAsync(user);
                if (!claims.Any(c => c.Type == CustomClaimTypes.PermissionGroupManager && c.Value == group)
                    && !claims.Any(c => c.Type == CustomClaimTypes.PermissionDataOwner && c.Value == $"{dataType}{{{id}}}"))
                {
                    return StatusCode(403, _errorLocalizer[ErrorMessages.ManagerOrOwnerOnlyError]);
                }
            }

            var groupRole = await _roleManager.FindByNameAsync(group);
            if (groupRole == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidTargetGroupError]);
            }
            if (!TryGetClaim(dataType, operation, id, out Claim claim))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.DataError]);
            }
            foreach (var impliedClaim in GetImpliedClaimsForRemove(claim))
            {
                await _roleManager.RemoveClaimAsync(groupRole, impliedClaim);
            }
            return Ok();
        }

        /// <summary>
        /// Called to stop the given data from being shared with the given user.
        /// </summary>
        /// <param name="username">The name of the user with whom the data is being shared.</param>
        /// <param name="dataType">The type of data being shared.</param>
        /// <param name="operation">Optionally, an operation whose permission is being removed.</param>
        /// <param name="id">Optionally, the primary key of a specific item being shared.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost("{username}/{dataType}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> HideDataFromUser(string username, string dataType, string operation, string id)
        {
            var user = await _userManager.FindByIdAsync(HttpContext.User.FindFirstValue(JwtClaimTypes.Subject));
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            // Only admins can hide a data type, rather than a particular item
            if (string.IsNullOrEmpty(id) && !roles.Contains(CustomRoles.Admin))
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.AdminOnlyError]);
            }
            if (!roles.Contains(CustomRoles.Admin)
                && !claims.Any(c => c.Type == CustomClaimTypes.PermissionDataOwner && c.Value == $"{dataType}{{{id}}}"))
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.OwnerOnlyError]);
            }

            var targetUser = await _userManager.FindByNameAsync(username);
            if (targetUser == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidTargetUserError]);
            }
            if (!TryGetClaim(dataType, operation, id, out Claim claim))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.DataError]);
            }
            await _userManager.RemoveClaimsAsync(targetUser, GetImpliedClaimsForRemove(claim));
            return Ok();
        }

        /// <summary>
        /// Called to share the given data with all users.
        /// </summary>
        /// <param name="dataType">The type of data to be shared.</param>
        /// <param name="operation">Optionally, an operation for which to grant permission.</param>
        /// <param name="id">Optionally, the primary key of a specific item to be shared.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost("{dataType}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ShareDataWithAll(string dataType, string operation, string id)
        {
            if (string.IsNullOrEmpty(dataType))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
            }
            var user = await _userManager.FindByIdAsync(HttpContext.User.FindFirstValue(JwtClaimTypes.Subject));
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            var roles = await _userManager.GetRolesAsync(user);
            // Only Admins can share data with all.
            if (!roles.Contains(CustomRoles.Admin))
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.AdminOnlyError]);
            }
            if (operation != CustomClaimTypes.PermissionDataView
                && operation != CustomClaimTypes.PermissionDataEdit)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.ViewEditOnlyError]);
            }

            dataType = dataType.ToInitialCaps();
            // Entity isn't used, but is parsed to enure it's valid.
            var entity = _context.Model.GetEntityTypes().FirstOrDefault(e => e.Name.Substring(e.Name.LastIndexOf('.') + 1) == dataType);
            if (entity == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
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
            return Ok();
        }

        /// <summary>
        /// Called to share the given data with the given group.
        /// </summary>
        /// <param name="group">The name of the group with whom to share the data.</param>
        /// <param name="dataType">The type of data to be shared.</param>
        /// <param name="operation">Optionally, an operation for which to grant permission.</param>
        /// <param name="id">Optionally, the primary key of a specific item to be shared.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost("{group}/{dataType}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ShareDataWithGroup(string group, string dataType, string operation, string id)
        {
            var user = await _userManager.FindByIdAsync(HttpContext.User.FindFirstValue(JwtClaimTypes.Subject));
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            var groupRole = await _roleManager.FindByNameAsync(group);
            if (groupRole == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidTargetGroupError]);
            }
            if (!TryGetClaim(dataType, operation, id, out Claim claim))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.DataError]);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            // Only admins can share a data type, rather than a particular item
            if (string.IsNullOrEmpty(id) && !roles.Contains(CustomRoles.Admin))
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.AdminOnlyError]);
            }
            // Admins can share data with any group as if they owned that data, regardless of their own membership.
            if (roles.Contains(CustomRoles.Admin)
                || claims.Any(c => c.Type == CustomClaimTypes.PermissionDataOwner && c.Value == $"{dataType}{{{id}}}"))
            {
                // Permissions other than view/edit can only be shared for an entire type.
                if (!string.IsNullOrEmpty(id)
                    && operation != CustomClaimTypes.PermissionDataView
                    && operation != CustomClaimTypes.PermissionDataEdit)
                {
                    return BadRequest(_errorLocalizer[ErrorMessages.ViewEditOnlyError]);
                }
            }
            else
            {
                // Managers of groups can re-share data with their group which has been shared with them.
                if (claims.Any(c => c.Type == CustomClaimTypes.PermissionGroupManager && c.Value == group))
                {
                    if (!Authorization.PermissionIncludesTarget(
                        Authorization.GetAuthorization(claims, dataType, claim.Type, id),
                        claim.Type))
                    {
                        return StatusCode(403, _errorLocalizer[ErrorMessages.ManagerOnlySharedError]);
                    }
                }
                else
                {
                    return StatusCode(403, _errorLocalizer[ErrorMessages.ManagerOrOwnerOnlyError]);
                }
            }

            await _roleManager.AddClaimAsync(groupRole, claim);
            return Ok();
        }

        /// <summary>
        /// Called to share the given data with the given user.
        /// </summary>
        /// <param name="username">The name of the user with whom to share the data.</param>
        /// <param name="dataType">The type of data to be shared.</param>
        /// <param name="operation">Optionally, an operation for which to grant permission.</param>
        /// <param name="id">Optionally, the primary key of a specific item to be shared.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost("{username}/{dataType}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ShareDataWithUser(string username, string dataType, string operation, string id)
        {
            var user = await _userManager.FindByIdAsync(HttpContext.User.FindFirstValue(JwtClaimTypes.Subject));
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            // Only admins can share a data type, rather than a particular item
            if (string.IsNullOrEmpty(id) && !roles.Contains(CustomRoles.Admin))
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.AdminOnlyError]);
            }
            // Admins can share data with any user as if they owned that data.
            if (roles.Contains(CustomRoles.Admin)
                || claims.Any(c => c.Type == CustomClaimTypes.PermissionDataOwner && c.Value == $"{dataType}{{{id}}}"))
            {
                // Permissions other than view/edit can only be shared for an entire type.
                if (!string.IsNullOrEmpty(id)
                    && operation != CustomClaimTypes.PermissionDataView
                    && operation != CustomClaimTypes.PermissionDataEdit)
                {
                    return BadRequest(_errorLocalizer[ErrorMessages.ViewEditOnlyError]);
                }
            }
            else
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.OwnerOnlyError]);
            }

            var targetUser = await _userManager.FindByNameAsync(username);
            if (targetUser == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidTargetUserError]);
            }
            if (!TryGetClaim(dataType, operation, id, out Claim claim))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.DataError]);
            }
            await _userManager.AddClaimAsync(targetUser, claim);
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
