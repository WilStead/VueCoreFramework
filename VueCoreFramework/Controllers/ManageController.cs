using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VueCoreFramework.Models;
using VueCoreFramework.Models.ViewModels;
using VueCoreFramework.Services;
using System.Security.Claims;
using System;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using VueCoreFramework.Data;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace VueCoreFramework.Controllers
{
    /// <summary>
    /// An MVC controller for handling user management tasks.
    /// </summary>
    [Authorize]
    public class ManageController : Controller
    {
        private readonly AdminOptions _adminOptions;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ManageController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Initializes a new instance of <see cref="ManageController"/>.
        /// </summary>
        public ManageController(
            IOptions<AdminOptions> adminOptions,
            ApplicationDbContext context,
            IEmailSender emailSender,
            ILogger<ManageController> logger,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _adminOptions = adminOptions.Value;
            _context = context;
            _emailSender = emailSender;
            _logger = logger;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        /// <summary>
        /// Called to initiate an email address change for a user.
        /// </summary>
        /// <param name="model">A <see cref="ManageUserViewModel"/> used to transfer task data.</param>
        /// <returns>A <see cref="ManageUserViewModel"/> used to transfer task data.</returns>
        [HttpPost("api/[controller]/[action]")]
        public async Task<ManageUserViewModel> ChangeEmail([FromBody]ManageUserViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (user == null)
            {
                model.Errors.Add(ErrorMessages.InvalidUserError);
                return model;
            }
            if (user.AdminLocked)
            {
                model.Errors.Add(ErrorMessages.LockedAccount(_adminOptions.AdminEmailAddress));
                return model;
            }
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                model.Errors.Add(ErrorMessages.DuplicateEmailError);
                return model;
            }
            if (user.LastEmailChange > DateTime.Now.Subtract(TimeSpan.FromDays(1)))
            {
                model.Errors.Add(ErrorMessages.ChangeEmailLimitError);
                return model;
            }

            user.NewEmail = model.Email;
            await _userManager.UpdateAsync(user);

            // Generate an email with a callback URL pointing to the 'RestoreEmail' action in the
            // 'Account' controller, so the current user can undo this change, if it was a mistake,
            // or from an unauthorized source.
            var confirmCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var restoreCallbackUrl = Url.Action(nameof(AccountController.RestoreEmail), "Account", new { userId = user.Id, code = confirmCode }, protocol: HttpContext.Request.Scheme);
            await _emailSender.SendEmailAsync(user.Email, "Confirm your email change",
                $"A request was made to change the email address on your account from this email address to a new one. If this was a mistake, please click this link to reject the requested change: <a href='{restoreCallbackUrl}'>link</a>");

            // Generate an email with a callback URL pointing to the 'ChangeEmail' action in the
            // 'Account' controller, which will confirm the change by validating that the newly
            // requested email belongs to the user.
            var changeCode = await _userManager.GenerateChangeEmailTokenAsync(user, user.NewEmail);
            var changeCallbackUrl = Url.Action(nameof(AccountController.ChangeEmail), "Account", new { userId = user.Id, code = changeCode }, protocol: HttpContext.Request.Scheme);
            await _emailSender.SendEmailAsync(user.NewEmail, "Confirm your email change",
                $"Please confirm your email address change by clicking this link: <a href='{changeCallbackUrl}'>link</a>");

            _logger.LogInformation(LogEvent.EMAIL_CHANGE_REQUEST, "Email change request received, from {OLDEMAIL} to {NEWEMAIL}.", user.Email, user.NewEmail);

            return model;
        }

        /// <summary>
        /// Called to initiate a password change for a user.
        /// </summary>
        /// <param name="model">A <see cref="ManageUserViewModel"/> used to transfer task data.</param>
        /// <returns>A <see cref="ManageUserViewModel"/> used to transfer task data.</returns>
        [HttpPost("api/[controller]/[action]")]
        public async Task<ManageUserViewModel> ChangePassword([FromBody]ManageUserViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (user == null)
            {
                model.Errors.Add(ErrorMessages.InvalidUserError);
                return model;
            }
            if (user.AdminLocked)
            {
                model.Errors.Add(ErrorMessages.LockedAccount(_adminOptions.AdminEmailAddress));
                return model;
            }
            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation(LogEvent.CHANGE_PW, "User {USER} changed their password.", user.Email);
                return model;
            }
            model.Errors.AddRange(result.Errors.Select(e => e.Description));
            return model;
        }

        /// <summary>
        /// Called to initiate a username change for a user.
        /// </summary>
        /// <param name="model">A <see cref="ManageUserViewModel"/> used to transfer task data.</param>
        /// <returns>A <see cref="ManageUserViewModel"/> used to transfer task data.</returns>
        [HttpPost("api/[controller]/[action]")]
        public async Task<ManageUserViewModel> ChangeUsername([FromBody]ManageUserViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (user == null)
            {
                model.Errors.Add(ErrorMessages.InvalidUserError);
                return model;
            }
            if (user.AdminLocked)
            {
                model.Errors.Add(ErrorMessages.LockedAccount(_adminOptions.AdminEmailAddress));
                return model;
            }
            var existingUser = await _userManager.FindByNameAsync(model.Username);
            if (existingUser != null)
            {
                model.Errors.Add(ErrorMessages.DuplicateUsernameError);
                return model;
            }

            var old = user.UserName;
            await _userManager.SetUserNameAsync(user, model.Username);

            _logger.LogInformation(LogEvent.USERNAME_CHANGE, "Username changed for {USER}, from {OLDUSERNAME} to {NEWUSERNAME}.", user.Email, old, user.UserName);

            return model;
        }

        /// <summary>
        /// Called to initiate an account deletion for a user.
        /// </summary>
        /// <returns>
        /// An error if there is a problem; or redirects to the SPA page for account deletion.
        /// </returns>
        [HttpPost("api/[controller]/[action]")]
        public async Task<IActionResult> DeleteAccount(string xferUsername)
        {
            var user = await _userManager.FindByEmailAsync(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (user == null)
            {
                return Json(new { error = ErrorMessages.InvalidUserError });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = ErrorMessages.LockedAccount(_adminOptions.AdminEmailAddress) });
            }
            if (user.LastEmailChange > DateTime.Now.Subtract(TimeSpan.FromDays(1)))
            {
                // Deletion within a day of changing the email is prevented to allow time for a user
                // to recover from an unauthorized email change.
                return Json(new { error = ErrorMessages.DeleteLimitError });
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var restoreCallbackUrl = Url.Action(
                nameof(ManageController.DeleteAccountCallback),
                "Manage",
                new { userId = user.Id, code = code, xferUsername = xferUsername },
                protocol: HttpContext.Request.Scheme);
            var loginLink = Url.Action(nameof(HomeController.Index), "Home", new { forwardUrl = "/login" });
            await _emailSender.SendEmailAsync(user.Email, "Confirm your account deletion",
                $"A request was made to delete your account. If you wish to permanently delete your account, please click this link to confirm: <a href='{restoreCallbackUrl}'>link</a>. If you did not initiate this action, please do not click the link. Instead, you should <a href='{loginLink}'>log into your account</a> and change your password to prevent any further unauthorized use.");
            return Json(new { response = ResponseMessages.Success });
        }

        /// <summary>
        /// The endpoint reached when a user clicks the link in an email sent to confirm an account deletion.
        /// </summary>
        /// <returns>Redirects to the home page.</returns>
        [HttpPost]
        public async Task<IActionResult> DeleteAccountCallback(string userId, string code, string xferUsername)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = ErrorMessages.LockedAccount(_adminOptions.AdminEmailAddress) });
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }

            var claims = await _userManager.GetClaimsAsync(user);
            var managerClaims = claims.Where(c => c.Type == CustomClaimTypes.PermissionGroupManager).ToList();
            var ownerClaims = claims.Where(c => c.Type == CustomClaimTypes.PermissionDataOwner).ToList();

            // Transfer data
            ApplicationUser xferUser = null;
            if (!string.IsNullOrEmpty(xferUsername))
            {
                xferUser = await _userManager.FindByNameAsync(xferUsername);
                if (xferUser == null || xferUser == user)
                {
                    return Json(new { error = ErrorMessages.InvalidTargetUserError });
                }
                await _userManager.AddClaimsAsync(xferUser, managerClaims);
                await _userManager.AddClaimsAsync(xferUser, ownerClaims);
            }
            else
            {
                foreach (var managerClaim in managerClaims)
                {
                    var members = await _userManager.GetUsersInRoleAsync(managerClaim.Value);
                    // If the deleted account is the only one in the group, delete it.
                    if (members.Count <= 1)
                    {
                        var role = await _roleManager.FindByNameAsync(managerClaim.Value);
                        // Delete group messages
                        var groupMessages = _context.Messages.Where(m => m.GroupRecipient == role);
                        _context.RemoveRange(groupMessages);
                        await _context.SaveChangesAsync();
                        await _roleManager.DeleteAsync(role);
                    }
                    // Otherwise, assign the manager role to another member arbitrarily.
                    else
                    {
                        var otherMember = members.FirstOrDefault(m => m != user);
                        await _userManager.AddClaimAsync(otherMember, managerClaim);
                    }
                }
                foreach (var ownerClaim in ownerClaims)
                {
                    List<ApplicationUser> managerShares = new List<ApplicationUser>();
                    foreach (var role in _roleManager.Roles)
                    {
                        var roleClaims = await _roleManager.GetClaimsAsync(role);
                        if (roleClaims.Any(c => c.Value == ownerClaim.Value))
                        {
                            var managerId = _context.UserClaims.FirstOrDefault(c =>
                                c.ClaimType == CustomClaimTypes.PermissionGroupManager && c.ClaimValue == role.Name)?
                                .UserId;
                            var manager = await _userManager.FindByIdAsync(managerId);
                            if (manager != null)
                            {
                                managerShares.Add(manager);
                            }
                        }
                    }

                    var sharedClaimIds = _context.UserClaims.Where(c =>
                        c.ClaimValue == ownerClaim.Value &&
                        (c.ClaimType == CustomClaimTypes.PermissionDataView || c.ClaimType == CustomClaimTypes.PermissionDataEdit))
                        .Select(c => c.UserId);
                    var shares = new List<ApplicationUser>();
                    foreach (var shareId in sharedClaimIds)
                    {
                        shares.Add(await _userManager.FindByIdAsync(shareId));
                    }


                    // Prefer a previously-selected user, if it's within the share group.
                    ApplicationUser candidate = null;
                    if (xferUser == null || (!managerShares.Contains(xferUser) && !shares.Contains(xferUser)))
                    {
                        // Otherwise prefer a manager of a group share over an individual.
                        candidate = managerShares.FirstOrDefault();
                        if (candidate == null)
                        {
                            candidate = shares.FirstOrDefault();
                        }
                    }

                    // If no share was found, delete the item.
                    if (candidate == null)
                    {
                        var index = ownerClaim.Value.IndexOf('{');
                        // If the claim value is improperly formed or outdated, ignore it.
                        if (index != -1)
                        {
                            var dataType = ownerClaim.Value.Substring(0, index);
                            var id = ownerClaim.Value.Substring(index + 1, ownerClaim.Value.Length - index - 2);
                            if (DataController.TryGetRepository(_context, dataType, out IRepository repository))
                            {
                                await repository.RemoveAsync(id);
                            }
                        }
                    }
                    else
                    {
                        xferUser = candidate;
                        await _userManager.AddClaimAsync(xferUser, ownerClaim);
                    }
                }
            }

            // Delete orphaned messages
            var messages = _context.Messages.Where(m => m.Sender == user || m.SingleRecipient == user)
                .Include(m => m.Sender)
                .Include(m => m.SingleRecipient)
                .Include(m => m.GroupRecipient);
            _context.RemoveRange(messages.Where(m => m.GroupRecipient == null
                && (m.Sender == user || m.SingleRecipient == user)));
            await _context.SaveChangesAsync();

            await _userManager.DeleteAsync(user);

            _logger.LogInformation(LogEvent.DELETE_ACCOUNT, "User {USER} has deleted their account.", user.Email);

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        /// <summary>
        /// Called to link a user's external authentication provider account with their site account.
        /// </summary>
        /// <param name="model">A <see cref="ManageUserViewModel"/> used to transfer task data.</param>
        /// <returns>
        /// A <see cref="ManageUserViewModel"/> used to transfer task data in the event of a problem,
        /// or a <see cref="ChallengeResult"/> for the authentication provider.
        /// </returns>
        [HttpPost]
        public IActionResult LinkLogin([FromBody]ManageUserViewModel model)
        {
            var provider = _signInManager.GetExternalAuthenticationSchemes().SingleOrDefault(a => a.DisplayName == model.AuthProvider);
            if (provider == null)
            {
                model.Errors.Add(ErrorMessages.AuthProviderError);
                _logger.LogWarning(LogEvent.EXTERNAL_PROVIDER_NOTFOUND, "Could not find provider {PROVIDER}.", model.AuthProvider);
                return new JsonResult(model);
            }
            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Action(nameof(LinkLoginCallback), "Manage");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider.AuthenticationScheme, redirectUrl, _userManager.GetUserId(User));
            return Challenge(properties, provider.AuthenticationScheme);
        }

        /// <summary>
        /// The endpoint reached when a user returns from an external authentication provider when
        /// attempting to link that account with their site account.
        /// </summary>
        /// <returns>A <see cref="ManageUserViewModel"/> used to transfer task data.</returns>
        [HttpGet]
        public async Task<ManageUserViewModel> LinkLoginCallback()
        {
            var model = new ManageUserViewModel();
            var user = await _userManager.FindByEmailAsync(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (user == null)
            {
                model.Errors.Add(ErrorMessages.AuthProviderError);
                return model;
            }
            if (user.AdminLocked)
            {
                model.Errors.Add(ErrorMessages.LockedAccount(_adminOptions.AdminEmailAddress));
                return model;
            }
            var info = await _signInManager.GetExternalLoginInfoAsync(await _userManager.GetUserIdAsync(user));
            if (info == null)
            {
                model.Errors.Add(ErrorMessages.AuthProviderError);
                return model;
            }
            var result = await _userManager.AddLoginAsync(user, info);
            if (result.Succeeded)
            {
                _logger.LogInformation(LogEvent.ADD_EXTERNAL_LOGIN, "Added {PROVIDER} login for {USER}.", info.LoginProvider, user.Email);
                return model;
            }
            model.Errors.AddRange(result.Errors.Select(e => e.Description));
            return model;
        }

        /// <summary>
        /// Gets a list of the usernames of members who are in groups with the current user.
        /// </summary>
        /// <param name="model">A <see cref="ManageUserViewModel"/> used to transfer task data.</param>
        /// <returns>An error if there is a problem; or the list (as JSON).</returns>
        [HttpGet("api/[controller]/[action]")]
        public async Task<IActionResult> LoadXferUsernames()
        {
            var user = await _userManager.FindByEmailAsync(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (user == null)
            {
                return Json(new { error = ErrorMessages.InvalidUserError });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = ErrorMessages.LockedAccount(_adminOptions.AdminEmailAddress) });
            }

            var roles = await _userManager.GetRolesAsync(user);
            List<ApplicationUser> xferUsers = new List<ApplicationUser>();
            foreach (var role in roles)
            {
                var groupMembers = await _userManager.GetUsersInRoleAsync(role);
                xferUsers = xferUsers.Union(groupMembers).ToList();
            }

            return Json(xferUsers.Select(u => u.UserName).ToArray());
        }

        /// <summary>
        /// Called to lock a user's account. Admin only.
        /// </summary>
        /// <param name="username">The username of the account to lock.</param>
        /// <returns>An error if a problem occurs, or a response indicating success.</returns>
        [HttpPost("api/[controller]/[action]/{username}")]
        public async Task<IActionResult> LockAccount(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var user = await _userManager.FindByEmailAsync(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = ErrorMessages.LockedAccount(_adminOptions.AdminEmailAddress) });
            }

            var admins = await _userManager.GetUsersInRoleAsync(CustomRoles.Admin);
            if (!admins.Contains(user))
            {
                return Json(new { error = ErrorMessages.AdminOnlyError });
            }

            var lockUser = await _userManager.FindByNameAsync(username);
            if (lockUser == null)
            {
                return Json(new { error = ErrorMessages.InvalidTargetUserError });
            }
            if (admins.Contains(lockUser))
            {
                return Json(new { error = ErrorMessages.NotForAdminsError });
            }
            if (lockUser.AdminLocked)
            {
                return Json(new { error = ErrorMessages.AlreadyLockedError });
            }
            lockUser.AdminLocked = true;
            await _userManager.UpdateAsync(lockUser);

            _logger.LogInformation(LogEvent.LOCK_ACCOUNT, "The account belonging to User {USER} has been locked by Admin {ADMIN}.", lockUser.Email, user.Email);

            return Json(new { response = ResponseMessages.Success });
        }

        /// <summary>
        /// Called to remove a user's external authentication provider account from their site account.
        /// </summary>
        /// <param name="model">A <see cref="ManageUserViewModel"/> used to transfer task data.</param>
        /// <returns>A <see cref="ManageUserViewModel"/> used to transfer task data.</returns>
        [HttpPost]
        public async Task<ManageUserViewModel> RemoveLogin([FromBody]ManageUserViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (user == null)
            {
                model.Errors.Add(ErrorMessages.InvalidUserError);
                return model;
            }
            if (user.AdminLocked)
            {
                model.Errors.Add(ErrorMessages.LockedAccount(_adminOptions.AdminEmailAddress));
                return model;
            }
            var userLogins = await _userManager.GetLoginsAsync(user);
            var provider = userLogins.SingleOrDefault(a => a.ProviderDisplayName == model.AuthProvider);
            if (provider == null)
            {
                model.Errors.Add(ErrorMessages.AuthProviderError);
                _logger.LogWarning(LogEvent.EXTERNAL_PROVIDER_NOTFOUND, "Could not find provider {PROVIDER}.", model.AuthProvider);
                return model;
            }
            var result = await _userManager.RemoveLoginAsync(user, provider.LoginProvider, provider.ProviderKey);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation(LogEvent.REMOVE_EXTERNAL_LOGIN, "Removed {PROVIDER} login for {USER}.", provider.LoginProvider, user.Email);
                return model;
            }
            model.Errors.AddRange(result.Errors.Select(e => e.Description));
            return model;
        }

        /// <summary>
        /// Called to set a password for a user (for users who intiially registered with an external
        /// authentication provider, rather than a local site account).
        /// </summary>
        /// <param name="model">A <see cref="ManageUserViewModel"/> used to transfer task data.</param>
        /// <returns>A <see cref="ManageUserViewModel"/> used to transfer task data.</returns>
        [HttpPost("api/[controller]/[action]")]
        public async Task<ManageUserViewModel> SetPassword([FromBody]ManageUserViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (user == null)
            {
                model.Errors.Add(ErrorMessages.InvalidUserError);
                return model;
            }
            if (user.AdminLocked)
            {
                model.Errors.Add(ErrorMessages.LockedAccount(_adminOptions.AdminEmailAddress));
                return model;
            }
            var result = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation(LogEvent.SET_PW, "User {USER} set a password.", user.Email);
                return model;
            }
            model.Errors.AddRange(result.Errors.Select(e => e.Description));
            return model;
        }

        /// <summary>
        /// Called to unlock a user's account. Admin only.
        /// </summary>
        /// <param name="username">The username of the account to unlock.</param>
        /// <returns>An error if a problem occurs, or a response indicating success.</returns>
        [HttpPost("api/[controller]/[action]/{username}")]
        public async Task<IActionResult> UnlockAccount(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return Json(new { error = ErrorMessages.InvalidTargetUserError });
            }
            var user = await _userManager.FindByEmailAsync(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (user == null)
            {
                return Json(new { error = ErrorMessages.InvalidUserError });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = ErrorMessages.LockedAccount(_adminOptions.AdminEmailAddress) });
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(CustomRoles.Admin))
            {
                return Json(new { error = ErrorMessages.AdminOnlyError });
            }

            var lockUser = await _userManager.FindByNameAsync(username);
            if (lockUser == null)
            {
                return Json(new { error = ErrorMessages.InvalidTargetUserError });
            }
            if (!lockUser.AdminLocked)
            {
                return Json(new { error = ErrorMessages.AlreadyUnlockedError });
            }
            lockUser.AdminLocked = false;
            await _userManager.UpdateAsync(lockUser);

            _logger.LogInformation(LogEvent.UNLOCK_ACCOUNT, "The account belonging to User {USER} has been unlocked by Admin {ADMIN}.", lockUser.Email, user.Email);

            return Json(new { response = ResponseMessages.Success });
        }
    }
}
