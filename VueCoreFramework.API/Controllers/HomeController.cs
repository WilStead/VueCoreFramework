using Microsoft.AspNetCore.Mvc;
using VueCoreFramework.Core.Configuration;

namespace VueCoreFramework.API.Controllers
{
    /// <summary>
    /// The main MVC controller for the API server.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// If a user navigates to the root of the API server, the user is automatically redirected
        /// to the main SPA application's homepage.
        /// </summary>
        [HttpGet("[controller]/[action]")]
        [HttpGet("/")]
        public IActionResult Index(string forwardUrl = "")
        {
            return Redirect($"{URLs.ClientURL}Home/Index?forwardUrl={forwardUrl}");
        }
    }
}
