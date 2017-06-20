using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VueCoreFramework.Services;

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
        /// Shows a generic error page, in the event that an internal error occurs at a stage which
        /// prevents even loading the SPA (which has its own error pages).
        /// </summary>
        public IActionResult Error()
        {
            var exc = HttpContext.Features.Get<IExceptionHandlerFeature>();

            if (exc != null)
            {
                _logger.LogError(LogEvent.INTERNAL_ERROR, exc.Error, "An internal error occurred.");
            }

            return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/500" });
        }
    }
}
