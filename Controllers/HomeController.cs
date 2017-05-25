using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MVCCoreVue.Services;

namespace MVCCoreVue.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(string forwardUrl = "")
        {
            ViewData["ForwardUrl"] = forwardUrl;
            return View();
        }

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
