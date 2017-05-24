using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MVCCoreVue.Models;
using MVCCoreVue.Models.AccountViewModels;
using MVCCoreVue.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MVCCoreVue.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AccountController> _logger;
        private readonly TokenProviderOptions _options;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger,
            IOptions<TokenProviderOptions> options)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _options = options.Value;
        }

        [Authorize]
        [HttpGet]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Authorize()
        {
            var user = await _userManager.FindByEmailAsync(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (user != null) return Json(new AuthorizationViewModel { Email = user.Email, Authorization = "authorized" });
            else return Json(new AuthorizationViewModel { Authorization = "unauthorized" });
        }

        [HttpPost]
        [Route("api/Account/Login")]
        public async Task<LoginViewModel> Login([FromBody]LoginViewModel model)
        {
            // Require the user to have a confirmed email before they can log in.
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                model.Errors.Add("Invalid login attempt.");
                return model;
            }
            //else
            //{
            //    if (!await _userManager.IsEmailConfirmedAsync(user))
            //    {
            //        model.Errors.Add("You must have a confirmed email to log in.");
            //        return model;
            //    }
            //}

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                model.Errors.Add("Invalid login attempt.");
                return model;
            }

            var now = DateTime.UtcNow;
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, model.Email),
                new Claim(ClaimTypes.NameIdentifier, model.Email),
                new Claim(ClaimTypes.Name, model.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, (now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds.ToString(), ClaimValueTypes.Integer64)
            };
            var roles = await _userManager.GetRolesAsync(user);
            if (roles != null)
            {
                claims.AddRange(roles.Select(r => new Claim("role", r)));
            }
            var jwt = new JwtSecurityToken(
                claims: claims,
                notBefore: now,
                expires: now.Add(_options.Expiration),
                signingCredentials: _options.SigningCredentials);
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            model.Token = encodedJwt;

            _logger.LogInformation(1, "User logged in.");
            model.Redirect = true;
            return model;
        }

        [HttpPost]
        [Route("api/[controller]/[action]")]
        public async Task<RegisterViewModel> Register([FromBody]RegisterViewModel model)
        {
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=532713
                // Send an email with this link
                //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                //var callbackUrl = Url.Action(nameof(ConfirmEmail), "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                //await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
                //    $"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a>");

                // Comment out following line to prevent a new user automatically logged on.
                //await _signInManager.SignInAsync(user, isPersistent: false);

                _logger.LogInformation(3, "User created a new account with password.");

                model.Redirect = true;
            }
            else
            {
                model.Errors.AddRange(result.Errors.Select(e => e.Description));
            }

            return model;
        }

        [HttpPost]
        [Route("api/[controller]/[action]")]
        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation(4, "User logged out.");
        }

        [Authorize]
        [HttpGet]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> HasPassword()
        {
            var user = await _userManager.FindByEmailAsync(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (await _userManager.HasPasswordAsync(user)) return Json(new { response = "yes" });
            else return Json(new { response = "no" });
        }

        [Authorize]
        [HttpGet]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> HasPendingEmailChange()
        {
            var user = await _userManager.FindByEmailAsync(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (user == null || string.IsNullOrEmpty(user.NewEmail)) return Json(new { response = "no" });
            else return Json(new { response = "yes" });
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        //
        // GET: /Account/ExternalLoginCallback
        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                _logger.LogInformation(5, "User logged in with {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation(6, "User created an account using {Name} provider.", info.LoginProvider);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        // GET: /Account/ConfirmEmail
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded && !string.IsNullOrEmpty(user.NewEmail))
            {
                user.Email = user.NewEmail;
                user.NewEmail = null;
            }
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [HttpPost]
        [Route("api/[controller]/[action]")]
        public async Task ForgotPassword([FromBody]LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return;
            }

            // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=532713
            // Send an email with this link
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action(nameof(ResetPassword), "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
            await _emailSender.SendEmailAsync(model.Email, "Reset Password",
                $"Please reset your password by clicking here: <a href='{callbackUrl}'>{callbackUrl}</a>");
            return;
        }

        //
        // GET: /Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string code = null)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // GET /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
