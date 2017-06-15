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

namespace MVCCoreVue.Controllers
{
    /// <summary>
    /// An MVC controller for handling user management tasks.
    /// </summary>
    [Authorize]
    public class ManageController : Controller
    {
        private readonly AdminOptions _adminOptions;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ManageController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Initializes a new instance of <see cref="ManageController"/>.
        /// </summary>
        public ManageController(
            IOptions<AdminOptions> adminOptions,
            IEmailSender emailSender,
            ILogger<ManageController> logger,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _adminOptions = adminOptions.Value;
            _emailSender = emailSender;
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        /// <summary>
        /// Called to initiate an email address change for a user.
        /// </summary>
        /// <param name="model">A <see cref="ManageUserViewModel"/> used to transfer task data.</param>
        /// <returns>A <see cref="ManageUserViewModel"/> used to transfer task data.</returns>
        [HttpPost]
        [Route("api/[controller]/[action]")]
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
        [HttpPost]
        [Route("api/[controller]/[action]")]
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
        [HttpPost]
        [Route("api/[controller]/[action]")]
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
    }
}
