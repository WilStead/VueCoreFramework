using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VueCoreFramework.Auth.ViewModels;
using VueCoreFramework.Core.Configuration;
using VueCoreFramework.Core.Data;
using VueCoreFramework.Core.Data.Identity;
using VueCoreFramework.Core.Messages;
using VueCoreFramework.Core.Models;
using VueCoreFramework.Core.Services;
using VueCoreFramework.Sample.Data;

namespace VueCoreFramework.Auth.Controllers
{
    /// <summary>
    /// An MVC controller for handling user management tasks.
    /// </summary>
    [Authorize]
    public class ManageController : Controller
    {
        private readonly AdminOptions _adminOptions;
        private readonly Sample.Data.ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IStringLocalizer<ErrorMessages> _errorLocalizer;
        private readonly RequestLocalizationOptions _localizationOptions;
        private readonly ILogger<ManageController> _logger;
        private readonly IStringLocalizer<EmailMessages> _emailLocalizer;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Initializes a new instance of <see cref="ManageController"/>.
        /// </summary>
        public ManageController(
            IOptions<AdminOptions> adminOptions,
            Sample.Data.ApplicationDbContext context,
            IEmailSender emailSender,
            IStringLocalizer<ErrorMessages> errorLocalizer,
            IOptions<RequestLocalizationOptions> localizationOptions,
            ILogger<ManageController> logger,
            IStringLocalizer<EmailMessages> emailLocalizer,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _adminOptions = adminOptions.Value;
            _context = context;
            _emailSender = emailSender;
            _errorLocalizer = errorLocalizer;
            _localizationOptions = localizationOptions.Value;
            _logger = logger;
            _emailLocalizer = emailLocalizer;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        /// <summary>
        /// Called to initiate an email address change for a user.
        /// </summary>
        /// <param name="model">A <see cref="ManageUserViewModel"/> used to transfer task data.</param>
        /// <response code="403">Locked account.</response>
        /// <response code="400">Bad request.</response>
        /// <response code="200">Success.</response>
        [HttpPost]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ChangeEmail([FromBody]ManageUserViewModel model)
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
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.DuplicateEmailError]);
            }
            if (user.LastEmailChange > DateTime.Now.Subtract(TimeSpan.FromDays(1)))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.ChangeEmailLimitError]);
            }

            user.NewEmail = model.Email;
            await _userManager.UpdateAsync(user);

            // Generate an email with a callback URL pointing to the 'RestoreEmail' action in the
            // 'Account' controller, so the current user can undo this change, if it was a mistake,
            // or from an unauthorized source.
            var confirmCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var restoreCallbackUrl = Url.Action(
                action: nameof(AccountController.RestoreEmail),
                controller: "Account",
                values: new { userId = user.Id, code = confirmCode },
                protocol: HttpContext.Request.Scheme);
            await _emailSender.SendEmailAsync(user.Email, _emailLocalizer[EmailMessages.ConfirmEmailChangeSubject],
                $"{_emailLocalizer[EmailMessages.ConfirmEmailChangeCancelBody]} <a href='{restoreCallbackUrl}'>link</a>");

            // Generate an email with a callback URL pointing to the 'ChangeEmail' action in the
            // 'Account' controller, which will confirm the change by validating that the newly
            // requested email belongs to the user.
            var changeCode = await _userManager.GenerateChangeEmailTokenAsync(user, user.NewEmail);
            var changeCallbackUrl = Url.Action(
                action: nameof(AccountController.ChangeEmail),
                controller: "Account",
                values: new { userId = user.Id, code = changeCode },
                protocol: HttpContext.Request.Scheme);
            await _emailSender.SendEmailAsync(user.NewEmail, _emailLocalizer[EmailMessages.ConfirmEmailChangeSubject],
                $"{_emailLocalizer[EmailMessages.ConfirmEmailChangeSubject]} <a href='{changeCallbackUrl}'>link</a>");

            _logger.LogInformation(LogEvent.EMAIL_CHANGE_REQUEST, "Email change request received, from {OLDEMAIL} to {NEWEMAIL}.", user.Email, user.NewEmail);

            return Ok();
        }

        /// <summary>
        /// Called to initiate a password change for a user.
        /// </summary>
        /// <param name="model">A <see cref="ManageUserViewModel"/> used to transfer task data.</param>
        /// <response code="403">Locked account.</response>
        /// <response code="400">Bad request.</response>
        /// <response code="200">Success.</response>
        [HttpPost]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ChangePassword([FromBody]ManageUserViewModel model)
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
            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation(LogEvent.CHANGE_PW, "User {USER} changed their password.", user.Email);
                return Ok();
            }
            else
            {
                return BadRequest(string.Join(";", result.Errors.Select(e => e.Description)));
            }
        }

        /// <summary>
        /// Called to initiate a username change for a user.
        /// </summary>
        /// <param name="model">A <see cref="ManageUserViewModel"/> used to transfer task data.</param>
        /// <response code="403">Locked account.</response>
        /// <response code="400">Bad request.</response>
        /// <response code="200">Success.</response>
        [HttpPost]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ChangeUsername([FromBody]ManageUserViewModel model)
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
            var existingUser = await _userManager.FindByNameAsync(model.Username);
            if (existingUser != null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.DuplicateUsernameError]);
            }

            var old = user.UserName;
            await _userManager.SetUserNameAsync(user, model.Username);

            _logger.LogInformation(LogEvent.USERNAME_CHANGE, "Username changed for {USER}, from {OLDUSERNAME} to {NEWUSERNAME}.", user.Email, old, user.UserName);

            return Ok();
        }

        /// <summary>
        /// Called to initiate an account deletion for a user.
        /// </summary>
        /// <response code="403">Locked account.</response>
        /// <response code="400">Bad request.</response>
        /// <response code="200">Request received.</response>
        [HttpPost]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DeleteAccount(string xferUsername)
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
            if (user.LastEmailChange > DateTime.Now.Subtract(TimeSpan.FromDays(1)))
            {
                // Deletion within a day of changing the email is prevented to allow time for a user
                // to recover from an unauthorized email change.
                return BadRequest(_errorLocalizer[ErrorMessages.DeleteLimitError]);
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var restoreCallbackUrl = Url.Action(
                action: nameof(DeleteAccountCallback),
                controller: "Manage",
                values: new { userId = user.Id, code = code, xferUsername = xferUsername },
                protocol: HttpContext.Request.Scheme);
            var loginLink = Url.Action(
                action: nameof(HomeController.Index),
                controller: "Home",
                values: new { forwardUrl = "/login" },
                protocol: HttpContext.Request.Scheme);
            await _emailSender.SendEmailAsync(user.Email, _emailLocalizer[EmailMessages.ConfirmAccountDeletionSubject],
                $"{_emailLocalizer[EmailMessages.ConfirmAccountDeletionBody]} <a href='{restoreCallbackUrl}'>link</a>. {_emailLocalizer[EmailMessages.ConfirmAccountDeletionBody2]} <a href='{loginLink}'>login</a>.");
            return Ok();
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
                return Json(new { error = _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress] });
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
                    return Json(new { error = _errorLocalizer[ErrorMessages.InvalidTargetUserError] });
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
                            if (_context.TryGetRepository(dataType, out IRepository repository))
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
        /// <response code="400">Bad request.</response>
        /// <response code="302">Redirect to external authentication provider.</response>
        [HttpPost]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(302)]
        public async Task<IActionResult> LinkLogin([FromBody]ManageUserViewModel model)
        {
            var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
            var provider = schemes.SingleOrDefault(a => a.DisplayName == model.AuthProvider);
            if (provider == null)
            {
                _logger.LogWarning(LogEvent.EXTERNAL_PROVIDER_NOTFOUND, "Could not find provider {PROVIDER}.", model.AuthProvider);
                return BadRequest(_errorLocalizer[ErrorMessages.AuthProviderError]);
            }
            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Action(
                action: nameof(LinkLoginCallback),
                controller: "Manage",
                values: null,
                protocol: HttpContext.Request.Scheme);
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider.Name, redirectUrl, _userManager.GetUserId(User));
            return Challenge(properties, provider.Name);
        }

        /// <summary>
        /// The endpoint reached when a user returns from an external authentication provider when
        /// attempting to link that account with their site account.
        /// </summary>
        /// <response code="400">Bad request.</response>
        /// <response code="200">Success.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        public async Task<IActionResult> LinkLoginCallback()
        {
            var user = await _userManager.FindByIdAsync(HttpContext.User.FindFirstValue(JwtClaimTypes.Subject));
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.AuthProviderError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            var info = await _signInManager.GetExternalLoginInfoAsync(await _userManager.GetUserIdAsync(user));
            if (info == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.AuthProviderError]);
            }
            var result = await _userManager.AddLoginAsync(user, info);
            if (result.Succeeded)
            {
                _logger.LogInformation(LogEvent.ADD_EXTERNAL_LOGIN, "Added {PROVIDER} login for {USER}.", info.LoginProvider, user.Email);
                return Ok();
            }
            else
            {
                return BadRequest(string.Join(";", result.Errors.Select(e => e.Description)));
            }
        }

        /// <summary>
        /// Gets a list of the usernames of members who are in groups with the current user.
        /// </summary>
        /// <response code="403">Locked account.</response>
        /// <response code="200">The list of usernames.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 200)]
        public async Task<IActionResult> LoadXferUsernames()
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
        /// <param name="id">The username of the account to lock.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> LockAccount(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidTargetUserError]);
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

            var admins = await _userManager.GetUsersInRoleAsync(CustomRoles.Admin);
            if (!admins.Contains(user))
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.AdminOnlyError]);
            }

            var lockUser = await _userManager.FindByNameAsync(id);
            if (lockUser == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidTargetUserError]);
            }
            if (admins.Contains(lockUser))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.NotForAdminsError]);
            }
            if (lockUser.AdminLocked)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.AlreadyLockedError]);
            }
            lockUser.AdminLocked = true;
            await _userManager.UpdateAsync(lockUser);

            _logger.LogInformation(LogEvent.LOCK_ACCOUNT, "The account belonging to User {USER} has been locked by Admin {ADMIN}.", lockUser.Email, user.Email);

            return Ok();
        }

        /// <summary>
        /// Called to remove a user's external authentication provider account from their site account.
        /// </summary>
        /// <param name="model">A <see cref="ManageUserViewModel"/> used to transfer task data.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Locked account.</response>
        /// <response code="200">Success.</response>
        [HttpPost]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> RemoveLogin([FromBody]ManageUserViewModel model)
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
            var userLogins = await _userManager.GetLoginsAsync(user);
            var provider = userLogins.SingleOrDefault(a => a.ProviderDisplayName == model.AuthProvider);
            if (provider == null)
            {
                _logger.LogWarning(LogEvent.EXTERNAL_PROVIDER_NOTFOUND, "Could not find provider {PROVIDER}.", model.AuthProvider);
                return BadRequest(_errorLocalizer[ErrorMessages.AuthProviderError]);
            }
            var result = await _userManager.RemoveLoginAsync(user, provider.LoginProvider, provider.ProviderKey);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation(LogEvent.REMOVE_EXTERNAL_LOGIN, "Removed {PROVIDER} login for {USER}.", provider.LoginProvider, user.Email);
                return Ok();
            }
            else
            {
                return BadRequest(string.Join(";", result.Errors.Select(e => e.Description)));
            }
        }

        /// <summary>
        /// Called to set the current user's preferred culture.
        /// </summary>
        /// <param name="culture">The culture code to set for the current user.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Locked account.</response>
        /// <response code="200">Success.</response>
        [HttpPost("{culture}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> SetCulture(string culture)
        {
            if (string.IsNullOrEmpty(culture))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingDataError]);
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

            if (culture == "<default>")
            {
                user.Culture = _localizationOptions.DefaultRequestCulture.Culture.Name;
            }
            else
            {
                user.Culture = culture;
            }
            await _userManager.UpdateAsync(user);

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });

            return Ok();
        }

        /// <summary>
        /// Called to set a password for a user (for users who initially registered with an external
        /// authentication provider, rather than a local site account).
        /// </summary>
        /// <param name="model">A <see cref="ManageUserViewModel"/> used to transfer task data.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Locked account.</response>
        /// <response code="200">Success.</response>
        [HttpPost]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> SetPassword([FromBody]ManageUserViewModel model)
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
            var result = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation(LogEvent.SET_PW, "User {USER} set a password.", user.Email);
                return Ok();
            }
            else
            {
                return BadRequest(string.Join(";", result.Errors.Select(e => e.Description)));
            }
        }

        /// <summary>
        /// Called to unlock a user's account. Admin only.
        /// </summary>
        /// <param name="id">The username of the account to unlock.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> UnlockAccount(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidTargetUserError]);
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
            if (!roles.Contains(CustomRoles.Admin))
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.AdminOnlyError]);
            }

            var lockUser = await _userManager.FindByNameAsync(id);
            if (lockUser == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidTargetUserError]);
            }
            if (!lockUser.AdminLocked)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.AlreadyUnlockedError]);
            }
            lockUser.AdminLocked = false;
            await _userManager.UpdateAsync(lockUser);

            _logger.LogInformation(LogEvent.UNLOCK_ACCOUNT, "The account belonging to User {USER} has been unlocked by Admin {ADMIN}.", lockUser.Email, user.Email);

            return Ok();
        }
    }
}
