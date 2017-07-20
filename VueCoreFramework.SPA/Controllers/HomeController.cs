using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using VueCoreFramework.Core.Messages;

namespace VueCoreFramework.Controllers
{
    /// <summary>
    /// The primary MVC controller for the site.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly RequestLocalizationOptions _localizationOptions;
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="HomeController"/>.
        /// </summary>
        public HomeController(
            IOptions<RequestLocalizationOptions> localizationOptions,
            ILogger<HomeController> logger)
        {
            _localizationOptions = localizationOptions.Value;
            _logger = logger;
        }

        /// <summary>
        /// Shows a generic error page, in the event that an internal error occurs at a stage which
        /// prevents even loading the SPA (which has its own error pages).
        /// </summary>
        public IActionResult Error(int? errorId = null)
        {
            var exc = HttpContext.Features.Get<IExceptionHandlerFeature>();

            if (exc != null)
            {
                _logger.LogError(LogEvent.INTERNAL_ERROR, exc.Error, "An internal error occurred.");
            }

            return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = $"/error/{errorId ?? 500}" });
        }

        /// <summary>
        /// Called to retrieve a list of the supported cultures.
        /// </summary>
        /// <returns>A list of the supported cultures.</returns>
        [HttpGet]
        public JsonResult GetCultures()
            => Json(_localizationOptions.SupportedCultures.Select(c => c.Name));

        /// <summary>
        /// The primary endpoint for the site. Displays the SPA.
        /// </summary>
        /// <param name="forwardUrl">
        /// An optional redirect URL which may be used to load a specific page within the SPA.
        /// </param>
        public IActionResult Index(string forwardUrl = "")
        {
            ViewData["ForwardUrl"] = forwardUrl;
            return View();
        }

        /// <summary>
        /// Automatically redirects to the login page of the SPA.
        /// </summary>
        /// <param name="returnUrl">
        /// An optional redirect URL which will be passed to the login page.
        /// </param>
        public IActionResult Login(string returnUrl = "")
        {
            ViewData["ForwardUrl"] = "login";
            ViewData["ReturnUrl"] = returnUrl;
            return View(nameof(Index));
        }

        /// <summary>
        /// Called to sign out the current user.
        /// </summary>
        /// <response code="200">OK</response>
        [HttpPost("Account/[action]")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.Authentication.SignOutAsync("Cookies");
            await HttpContext.Authentication.SignOutAsync("oidc");
            return Ok();
        }

        /// <summary>
        /// Callback URL for IdentityServer OpenID Connect.
        /// </summary>
        [Route("oidc/callback")]
        public IActionResult OidcCallback()
        {
            return View();
        }

        /// <summary>
        /// Callback URL for IdentityServer OpenID Connect.
        /// </summary>
        [Route("oidc/callback_silent")]
        public IActionResult OidcSilentCallback()
        {
            return View();
        }
    }
}
