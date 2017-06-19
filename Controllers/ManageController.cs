using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MVCCoreVue.Models;
using MVCCoreVue.Models.ManageViewModels;
using MVCCoreVue.Services;
using System.Security.Claims;
using System;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MVCCoreVue.Data;
using System.Collections.Generic;

namespace MVCCoreVue.Controllers
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
                model.Errors.Add("An error has occurred.");
                return model;
            }
            if (user.AdminLocked)
            {
                model.Errors.Add($"Your account has been locked. Please contact an administrator at {_adminOptions.AdminEmailAddress} for assistance.");
                return model;
            }
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                model.Errors.Add("An account with this email already exists. If you've forgotten your password, please use the link on the login page.");
                return model;
            }
            if (user.LastEmailChange > DateTime.Now.Subtract(TimeSpan.FromDays(1)))
            {
                model.Errors.Add("You may not change the email on your account more than once per day.");
                return model;
            }

            user.NewEmail = model.Email;

            // Generate an email with a callback URL pointing to the 'RestoreEmail' action in the
            // 'Account' controller, so the current user can undo this change, if it was a mistake,
            // or from an unauthorized source.
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var restoreCallbackUrl = Url.Action(nameof(AccountController.RestoreEmail), "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
            await _emailSender.SendEmailAsync(user.Email, "Confirm your email change",
                $"A request was made to change the email address on your account from this email address to a new one. If this was a mistake, please click this link to reject the requested change: <a href='{restoreCallbackUrl}'>link</a>");

            // Generate an email with a callback URL pointing to the 'ChangeEmail' action in the
            // 'Account' controller, which will confirm the change by validating that the newly
            // requested email belongs to the user.
            var changeCallbackUrl = Url.Action(nameof(AccountController.ChangeEmail), "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
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
                model.Errors.Add("An error has occurred.");
                return model;
            }
            if (user.AdminLocked)
            {
                model.Errors.Add($"Your account has been locked. Please contact an administrator at {_adminOptions.AdminEmailAddress} for assistance.");
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
                model.Errors.Add("An error has occurred.");
                return model;
            }
            if (user.AdminLocked)
            {
                model.Errors.Add($"Your account has been locked. Please contact an administrator at {_adminOptions.AdminEmailAddress} for assistance.");
                return model;
            }
            var existingUser = await _userManager.FindByNameAsync(model.Username);
            if (existingUser != null)
            {
                model.Errors.Add("This username is already in use.");
                return model;
            }

            var old = user.UserName;
            user.UserName = model.Username;

            _logger.LogInformation(LogEvent.USERNAME_CHANGE, "Username changed for {USER}, from {OLDUSERNAME} to {NEWUSERNAME}.", user.Email, old, user.UserName);

            return model;
        }

        /// <summary>
        /// Called to delete a user account.
        /// </summary>
        /// <param name="model">A <see cref="ManageUserViewModel"/> used to transfer task data.</param>
        /// <returns>A <see cref="ManageUserViewModel"/> used to transfer task data.</returns>
        [HttpPost("api/[controller]/[action]/{xferUsername?}")]
        public async Task<IActionResult> DeleteAccount(string xferUsername)
        {
            var user = await _userManager.FindByEmailAsync(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (user == null)
            {
                return Json(new { error = "An error has occurred. Your account was not deleted." });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = $"Your account has been locked. Please contact an administrator at {_adminOptions.AdminEmailAddress} for assistance." });
            }

            var ownerClaims = await _userManager.GetClaimsAsync(user);
            ownerClaims = ownerClaims.Where(c => c.Type == CustomClaimTypes.PermissionDataOwner).ToList();

            ApplicationUser xferUser = null;
            if (!string.IsNullOrEmpty(xferUsername))
            {
                xferUser = await _userManager.FindByNameAsync(xferUsername);
                if (xferUser == null)
                {
                    return Json(new { error = "There was a problem with the account you specified for data transfer. Your account was not deleted." });
                }
                await _userManager.AddClaimsAsync(xferUser, ownerClaims);
            }
            else
            {
                foreach (var ownerClaim in ownerClaims)
                {
                    List<ApplicationUser> managerShares = new List<ApplicationUser>();
                    foreach (var role in _roleManager.Roles)
                    {
                        var roleClaims = await _roleManager.GetClaimsAsync(role);
                        if (roleClaims.Any(c => c.Value == ownerClaim.Value))
                        {
                            var managers = await _userManager.GetUsersForClaimAsync(new Claim(CustomClaimTypes.PermissionGroupManager, role.Name));
                            var manager = managers.FirstOrDefault();
                            if (manager != null)
                            {
                                managerShares.Add(manager);
                            }
                        }
                    }

                    var shares = await _userManager.GetUsersForClaimAsync(new Claim(CustomClaimTypes.PermissionDataView, ownerClaim.Value));
                    shares = shares.Concat(await _userManager.GetUsersForClaimAsync(new Claim(CustomClaimTypes.PermissionDataEdit, ownerClaim.Value))).ToList();


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
                            if (Guid.TryParse(id, out Guid guid) && DataController.TryGetRepository(_context, dataType, out IRepository repository))
                            {
                                await repository.RemoveAsync(guid);
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

            _logger.LogInformation(LogEvent.DELETE_ACCOUNT, "User {USER} has deleted their account.", user.Email);

            return Ok();
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
                model.Errors.Add("There was a problem authorizing with that provider.");
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
                model.Errors.Add("There was a problem authorizing with that provider.");
                return model;
            }
            if (user.AdminLocked)
            {
                model.Errors.Add($"Your account has been locked. Please contact an administrator at {_adminOptions.AdminEmailAddress} for assistance.");
                return model;
            }
            var info = await _signInManager.GetExternalLoginInfoAsync(await _userManager.GetUserIdAsync(user));
            if (info == null)
            {
                model.Errors.Add("There was a problem authorizing with that provider.");
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
                return Json(new { error = "An error has occurred." });
            }
            if (user.AdminLocked)
            {
                return Json(new { error = $"Your account has been locked. Please contact an administrator at {_adminOptions.AdminEmailAddress} for assistance." });
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

        [HttpPost("{username}")]
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
                return Json(new { error = $"Your account has been locked. Please contact an administrator at {_adminOptions.AdminEmailAddress} for assistance." });
            }

            var admins = await _userManager.GetUsersInRoleAsync(CustomRoles.Admin);
            if (!admins.Contains(user))
            {
                return Json(new { error = "Only an administrator can lock user accounts." });
            }

            var lockUser = await _userManager.FindByNameAsync(username);
            if (lockUser == null)
            {
                return Json(new { error = "There was a problem with the account you specified." });
            }
            if (admins.Contains(lockUser))
            {
                return Json(new { error = "Admin accounts can't be locked." });
            }
            if (lockUser.AdminLocked)
            {
                return Json(new { error = "The account you specified is already locked." });
            }
            lockUser.AdminLocked = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation(LogEvent.LOCK_ACCOUNT, "The account belonging to User {USER} has been locked by Admin {ADMIN}.", lockUser.Email, user.Email);

            return Ok();
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
                model.Errors.Add("There was a problem with your request.");
                return model;
            }
            if (user.AdminLocked)
            {
                model.Errors.Add($"Your account has been locked. Please contact an administrator at {_adminOptions.AdminEmailAddress} for assistance.");
                return model;
            }
            var userLogins = await _userManager.GetLoginsAsync(user);
            var provider = userLogins.SingleOrDefault(a => a.ProviderDisplayName == model.AuthProvider);
            if (provider == null)
            {
                model.Errors.Add("There was a problem with your request.");
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
                model.Errors.Add("An error has occurred.");
                return model;
            }
            if (user.AdminLocked)
            {
                model.Errors.Add($"Your account has been locked. Please contact an administrator at {_adminOptions.AdminEmailAddress} for assistance.");
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

        [HttpPost("{username}")]
        public async Task<IActionResult> UnlockAccount(string username)
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
                return Json(new { error = $"Your account has been locked. Please contact an administrator at {_adminOptions.AdminEmailAddress} for assistance." });
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(CustomRoles.Admin))
            {
                return Json(new { error = "Only an administrator can unlock user accounts." });
            }

            var lockUser = await _userManager.FindByNameAsync(username);
            if (lockUser == null)
            {
                return Json(new { error = "There was a problem with the account you specified." });
            }
            if (!lockUser.AdminLocked)
            {
                return Json(new { error = "The account you specified is not locked." });
            }
            lockUser.AdminLocked = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation(LogEvent.UNLOCK_ACCOUNT, "The account belonging to User {USER} has been unlocked by Admin {ADMIN}.", lockUser.Email, user.Email);

            return Ok();
        }
    }
}
