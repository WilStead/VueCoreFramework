using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VueCoreFramework.Core.Messages;

namespace VueCoreFramework.Controllers
{
    /// <summary>
    /// The primary MVC controller for the site.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="HomeController"/>.
        /// </summary>
        public HomeController(ILogger<HomeController> logger)
        {
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
        /// Callback URL for IdentityServer OpenID Connect.
        /// </summary>
        [HttpGet("oidc/callback")]
        public IActionResult OidcCallback()
        {
            return View();
        }
    }
}
