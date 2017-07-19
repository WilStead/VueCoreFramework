using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VueCoreFramework.Core.Configuration;
using VueCoreFramework.Core.Models;

namespace VueCoreFramework.Auth.Controllers
{
    /// <summary>
    /// The main MVC controller for the auth server.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        /// <summary>
        /// Initializes a new instance of <see cref="HomeController"/>.
        /// </summary>
        public HomeController(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        /// <summary>
        /// If an error is encountered by the auth server which requires a page display, the user is
        /// redirected to the main SPA application's error page.
        /// </summary>
        public IActionResult Error()
        {
            return Redirect($"{URLs.ClientURL}Home/Index?forwardUrl={new PathString("/error/500").ToString()}");
        }

        /// <summary>
        /// If a user navigates to the root of the auth server, the user is automatically redirected
        /// to the main SPA application's homepage.
        /// </summary>
        public IActionResult Index(string forwardUrl = "")
        {
            return Redirect($"{URLs.ClientURL}Home/Index?forwardUrl={forwardUrl}");
        }

        /// <summary>
        /// An endpoint which both logs out the user and redirects back to the SPA homepage.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.Authentication.SignOutAsync("Cookies");
            await HttpContext.Authentication.SignOutAsync("oidc");
            await _signInManager.SignOutAsync();
            return Redirect($"{URLs.ClientURL}Home/Index");
        }
    }
}
