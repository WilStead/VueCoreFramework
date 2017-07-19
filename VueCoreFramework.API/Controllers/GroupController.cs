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
using VueCoreFramework.Core.Messages;
using VueCoreFramework.Core.Models;
using VueCoreFramework.Core.Services;

namespace VueCoreFramework.API.Controllers
{
    /// <summary>
    /// An API controller for handling group membership tasks.
    /// </summary>
    [Authorize]
    public class GroupController : Controller
    {
        private readonly AdminOptions _adminOptions;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IStringLocalizer<ErrorMessages> _errorLocalizer;
        private readonly IStringLocalizer<EmailMessages> _emailLocalizer;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Initializes a new instance of <see cref="GroupController"/>.
        /// </summary>
        public GroupController(
            IOptions<AdminOptions> adminOptions,
            ApplicationDbContext context,
            IEmailSender emailSender,
            IStringLocalizer<ErrorMessages> errorLocalizer,
            IStringLocalizer<EmailMessages> emailLocalizer,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _adminOptions = adminOptions.Value;
            _context = context;
            _emailSender = emailSender;
            _errorLocalizer = errorLocalizer;
            _emailLocalizer = emailLocalizer;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        /// <summary>
        /// Called to get information about the given group.
        /// </summary>
        /// <param name="group">The name of the group to retrieve.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="404">Not found.</response>
        /// <response code="200">A GroupViewModel representing the found item.</response>
        [HttpPost("{group}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(IDictionary<string, object>), 200)]
        public async Task<IActionResult> GetGroup(string group)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.Email).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (group == CustomRoles.SiteAdmin)
            {
                if (roles.Contains(CustomRoles.SiteAdmin))
                {
                    return BadRequest(_errorLocalizer[ErrorMessages.SiteAdminSingularError]);
                }
                else
                {
                    return StatusCode(403, _errorLocalizer[ErrorMessages.SiteAdminOnlyError]);
                }
            }
            else if (group == CustomRoles.Admin)
            {
                if (!roles.Contains(CustomRoles.SiteAdmin))
                {
                    return StatusCode(403, _errorLocalizer[ErrorMessages.SiteAdminOnlyError]);
                }
            }
            else if (group == CustomRoles.AllUsers)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.AllUsersRequiredError]);
            }
            // Only Admins can retrieve information about a group by name.
            else if (!roles.Contains(CustomRoles.Admin))
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.AdminOnlyError]);
            }

            var groupRole = await _roleManager.FindByNameAsync(group);
            if (groupRole == null)
            {
                return NotFound();
            }
            var managerId = _context.UserClaims.FirstOrDefault(c =>
                c.ClaimType == CustomClaimTypes.PermissionGroupManager && c.ClaimValue == groupRole.Name)?
                .UserId;
            var manager = await _userManager.FindByIdAsync(managerId);
            var members = await _userManager.GetUsersInRoleAsync(groupRole.Name);
            return Json(new GroupViewModel
            {
                Name = groupRole.Name,
                Manager = manager?.UserName,
                Members = members.Select(m => m.UserName).ToList()
            });
        }

        /// <summary>
        /// Called to find all groups to which the current user belongs.
        /// </summary>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">A list of <see cref="GroupViewModel"/>s.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, object>), 200)]
        public async Task<IActionResult> GetGroupMemberships()
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.Email).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }

            var groups = await _userManager.GetRolesAsync(user);
            // Do not include the site admin or all users special roles.
            groups = groups.Where(g => g != CustomRoles.SiteAdmin && g != CustomRoles.AllUsers).ToList();

            List<GroupViewModel> vms = new List<GroupViewModel>();
            foreach (var group in groups)
            {
                string managerName = null;
                if (group == CustomRoles.Admin)
                {
                    var siteAdmin = await _userManager.GetUsersInRoleAsync(CustomRoles.SiteAdmin);
                    managerName = siteAdmin.FirstOrDefault().UserName;
                }
                else
                {
                    var managerId = _context.UserClaims.FirstOrDefault(c =>
                        c.ClaimType == CustomClaimTypes.PermissionGroupManager && c.ClaimValue == group)?
                        .UserId;
                    var manager = await _userManager.FindByIdAsync(managerId);
                    managerName = manager?.UserName;
                }
                var members = await _userManager.GetUsersInRoleAsync(group);
                vms.Add(new GroupViewModel
                {
                    Name = group,
                    Manager = managerName,
                    Members = members.Select(m => m.UserName).ToList()
                });
            }
            return Json(vms);
        }

        /// <summary>
        /// Called to invite a user to a group.
        /// </summary>
        /// <param name="username">
        /// The username of the user to invite to the group.
        /// </param>
        /// <param name="group">The name of the group to which the user will be invited.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost("{username}/{group}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> InviteUserToGroup(string username, string group)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.Email).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            if (username == user.UserName)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.SelfGroupAddError]);
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (group == CustomRoles.SiteAdmin)
            {
                if (roles.Contains(CustomRoles.SiteAdmin))
                {
                    return BadRequest(_errorLocalizer[ErrorMessages.SiteAdminSingularError]);
                }
                else
                {
                    return StatusCode(403, _errorLocalizer[ErrorMessages.SiteAdminOnlyError]);
                }
            }
            else if (group == CustomRoles.Admin)
            {
                if (!roles.Contains(CustomRoles.SiteAdmin))
                {
                    return StatusCode(403, _errorLocalizer[ErrorMessages.SiteAdminOnlyError]);
                }
            }
            else if (group == CustomRoles.AllUsers)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.AllUsersRequiredError]);
            }
            // Admins can invite users to any non-admin group, regardless of their own membership.
            else if (!roles.Contains(CustomRoles.Admin))
            {
                var claims = await _userManager.GetClaimsAsync(user);
                if (!claims.Any(c => c.Type == CustomClaimTypes.PermissionGroupManager && c.Value == group))
                {
                    return StatusCode(403, _errorLocalizer[ErrorMessages.ManagerOnlyError]);
                }
            }

            var groupRole = await _roleManager.FindByNameAsync(group);
            if (groupRole == null)
            {
                return Json(new { error = _errorLocalizer[ErrorMessages.InvalidTargetGroupError] });
            }
            var targetUser = await _userManager.FindByNameAsync(username);
            if (targetUser == null)
            {
                if (roles.Contains(CustomRoles.Admin))
                {
                    return BadRequest(_errorLocalizer[ErrorMessages.InvalidTargetUserError]);
                }
                // Non-admins are not permitted to know the identities of other users who are not
                // members of common groups. Therefore, indicate success despite there being no such
                // member, to avoid exposing a way to determine valid usernames.
                else
                {
                    return Ok();
                }
            }

            // Generate an email with a callback URL pointing to the 'AddUserToGroup' action.
            var confirmCode = await _userManager.GenerateEmailConfirmationTokenAsync(targetUser);
            var acceptCallbackUrl =
                Url.Action(
                    "AddUserToGroup",
                    "Group",
                    new { userId = targetUser.Id, groupId = groupRole.Id, code = confirmCode },
                    protocol: HttpContext.Request.Scheme,
                    host: URLs.ClientURL);
            await _emailSender.SendEmailAsync(targetUser.Email, _emailLocalizer[EmailMessages.GroupInviteSubject],
                $"{_emailLocalizer[EmailMessages.GroupInviteBody, group]} <a href='{acceptCallbackUrl}'>link</a>");

            return Ok();
        }

        /// <summary>
        /// Called to remove the current user from the given group.
        /// </summary>
        /// <param name="group">The name of the group to leave.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost("{group}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> LeaveGroup(string group)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.Email).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            if (group == CustomRoles.SiteAdmin)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.SiteAdminSingularError]);
            }
            else if (group == CustomRoles.AllUsers)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.AllUsersRequiredError]);
            }
            var groupRole = await _roleManager.FindByNameAsync(group);
            if (groupRole == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidTargetGroupError]);
            }
            var managerId = _context.UserClaims.FirstOrDefault(c =>
                c.ClaimType == CustomClaimTypes.PermissionGroupManager && c.ClaimValue == group)?
                .UserId;
            if (managerId == user.Id)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MustHaveManagerError]);
            }
            await _userManager.RemoveFromRoleAsync(user, group);
            return Ok();
        }

        /// <summary>
        /// Called to remove the given group.
        /// </summary>
        /// <param name="group">The name of the group to remove.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost("{group}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> RemoveGroup(string group)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.Email).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (group == CustomRoles.SiteAdmin)
            {
                if (roles.Contains(CustomRoles.SiteAdmin))
                {
                    return BadRequest(_errorLocalizer[ErrorMessages.SiteAdminSingularError]);
                }
                else
                {
                    return StatusCode(403, _errorLocalizer[ErrorMessages.SiteAdminOnlyError]);
                }
            }
            else if (group == CustomRoles.Admin)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.AdminRequiredError]);
            }
            else if (group == CustomRoles.AllUsers)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.AllUsersRequiredError]);
            }
            // Admins can delete any non-admin group, regardless of their own membership.
            else if (!roles.Contains(CustomRoles.Admin))
            {
                var claims = await _userManager.GetClaimsAsync(user);
                if (!claims.Any(c => c.Type == CustomClaimTypes.PermissionGroupManager && c.Value == group))
                {
                    return BadRequest(_errorLocalizer[ErrorMessages.ManagerOnlyError]);
                }
            }

            var groupRole = await _roleManager.FindByNameAsync(group);
            if (groupRole == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidTargetGroupError]);
            }
            // Delete group messages
            var groupMessages = _context.Messages.Where(m => m.GroupRecipient == groupRole);
            _context.RemoveRange(groupMessages);
            await _context.SaveChangesAsync();
            await _roleManager.DeleteAsync(groupRole);
            return Ok();
        }

        /// <summary>
        /// Called to remove the given user from the given group.
        /// </summary>
        /// <param name="username">The name of the user to remove from the group.</param>
        /// <param name="group">The group from which to remove the user.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost("{username}/{group}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> RemoveUserFromGroup(string username, string group)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.Email).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (group == CustomRoles.SiteAdmin)
            {
                if (roles.Contains(CustomRoles.SiteAdmin))
                {
                    return BadRequest(_errorLocalizer[ErrorMessages.SiteAdminSingularError]);
                }
                else
                {
                    return StatusCode(403, _errorLocalizer[ErrorMessages.SiteAdminOnlyError]);
                }
            }
            else if (group == CustomRoles.Admin)
            {
                if (!roles.Contains(CustomRoles.SiteAdmin))
                {
                    return StatusCode(403, _errorLocalizer[ErrorMessages.SiteAdminOnlyError]);
                }
            }
            else if (group == CustomRoles.AllUsers)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.AllUsersRequiredError]);
            }
            // Admins can remove users from any non-admin group, regardless of their own membership.
            else if (!roles.Contains(CustomRoles.Admin))
            {
                var claims = await _userManager.GetClaimsAsync(user);
                if (!claims.Any(c => c.Type == CustomClaimTypes.PermissionGroupManager && c.Value == group))
                {
                    return StatusCode(403, _errorLocalizer[ErrorMessages.ManagerOnlyError]);
                }
            }

            var groupRole = await _roleManager.FindByNameAsync(group);
            if (groupRole == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidTargetGroupError]);
            }
            var targetUser = await _userManager.FindByNameAsync(username);
            if (targetUser == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidTargetUserError]);
            }
            await _userManager.RemoveFromRoleAsync(targetUser, groupRole.Name);
            return Ok();
        }

        /// <summary>
        /// Called to create a new group with the given name, with the current user as its manager.
        /// </summary>
        /// <param name="group">The name of the group to create.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost("{group}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> StartNewGroup(string group)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.Email).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            var lowerGroup = group.ToLower();
            if (lowerGroup.StartsWith("admin") || lowerGroup.EndsWith("admin") || lowerGroup.Contains("administrator"))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.OnlyAdminCanBeAdminError]);
            }
            if (lowerGroup == "system")
            {
                return BadRequest(_errorLocalizer[ErrorMessages.CannotBeSystemError]);
            }
            if (lowerGroup == "true" || lowerGroup == "false")
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidNameError]);
            }
            var groupRole = await _roleManager.FindByNameAsync(group);
            if (groupRole != null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.DuplicateGroupNameError]);
            }
            var role = new IdentityRole(group);
            await _roleManager.CreateAsync(role);
            await _userManager.AddToRoleAsync(user, group);
            await _userManager.AddClaimAsync(user, new Claim(CustomClaimTypes.PermissionGroupManager, group));
            return Ok();
        }

        /// <summary>
        /// Called to transfer management of the given group to the given user.
        /// </summary>
        /// <param name="username">The name of the user who is to be the new manager.</param>
        /// <param name="group">The name of the group whose manager is to change.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost("{username}/{group}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> TransferManagerToUser(string username, string group)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.Email).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (group == CustomRoles.SiteAdmin || group == CustomRoles.Admin)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.AdminNoManagerError]);
            }
            // Admins can transfer the manager role of any non-admin group, regardless of membership.
            else if (!roles.Contains(CustomRoles.Admin))
            {
                var claims = await _userManager.GetClaimsAsync(user);
                if (!claims.Any(c => c.Type == CustomClaimTypes.PermissionGroupManager && c.Value == group))
                {
                    return StatusCode(403, _errorLocalizer[ErrorMessages.ManagerOnlyError]);
                }
            }

            var groupRole = await _roleManager.FindByNameAsync(group);
            if (groupRole == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidTargetGroupError]);
            }
            var targetUser = await _userManager.FindByNameAsync(username);
            if (targetUser == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidTargetUserError]);
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
                    return BadRequest(_errorLocalizer[ErrorMessages.GroupMemberOnlyError]);
                }
            }
            _context.UserClaims.Remove(_context.UserClaims.FirstOrDefault(c => c.ClaimType == CustomClaimTypes.PermissionGroupManager && c.ClaimValue == group));
            await _userManager.AddClaimAsync(targetUser, new Claim(CustomClaimTypes.PermissionGroupManager, group));

            _context.Messages.Add(new Message
            {
                Content = $"**{targetUser.UserName}** has been made the manager of **{group}**.",
                IsSystemMessage = true,
                GroupRecipient = groupRole,
                GroupRecipientName = groupRole.Name
            });
            await _context.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Called to transfer the site admin role to the given user.
        /// </summary>
        /// <param name="username">The name of the user who is to be the new site admin.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost("{username}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> TransferSiteAdminToUser(string username)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.Email).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(CustomRoles.SiteAdmin))
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.SiteAdminOnlyError]);
            }

            var targetUser = await _userManager.FindByNameAsync(username);
            if (targetUser == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidTargetUserError]);
            }
            await _userManager.AddToRoleAsync(targetUser, CustomRoles.SiteAdmin);
            await _userManager.RemoveFromRoleAsync(user, CustomRoles.SiteAdmin);

            _context.Messages.Add(new Message
            {
                Content = $"You have been made the new site administrator. Please contact the former site administrator (**{user.UserName}**) with any questions about this role.",
                IsSystemMessage = true,
                SingleRecipient = targetUser,
                SingleRecipientName = targetUser.UserName
            });
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
