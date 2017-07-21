using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VueCoreFramework.Core.Configuration;

namespace VueCoreFramework.API.Controllers
{
    /// <summary>
    /// The main MVC controller for the API server.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly URLOptions _urls;

        /// <summary>
        /// Initializes a new instance of <see cref="HomeController"/>.
        /// </summary>
        /// <param name="urls">Provides the URLs for the different hosts which form the application.</param>
        public HomeController(IOptions<URLOptions> urls)
        {
            _urls = urls.Value;
        }

        /// <summary>
        /// If a user navigates to the root of the API server, the user is automatically redirected
        /// to the main SPA application's homepage.
        /// </summary>
        [HttpGet("[controller]/[action]")]
        [HttpGet("/")]
        public IActionResult Index(string forwardUrl = "")
        {
            return Redirect($"{_urls.ClientURL}Home/Index?forwardUrl={forwardUrl}");
        }
    }
}
