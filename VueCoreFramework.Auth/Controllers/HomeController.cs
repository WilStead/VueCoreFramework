using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
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
        private readonly URLOptions _urls;

        /// <summary>
        /// Initializes a new instance of <see cref="HomeController"/>.
        /// </summary>
        public HomeController(SignInManager<ApplicationUser> signInManager, IOptions<URLOptions> urls)
        {
            _signInManager = signInManager;
            _urls = urls.Value;
        }

        /// <summary>
        /// If an error is encountered by the auth server which requires a page display, the user is
        /// redirected to the main SPA application's error page.
        /// </summary>
        public IActionResult Error()
        {
            return Redirect($"{_urls.ClientURL}Home/Index?forwardUrl={new PathString("/error/500").ToString()}");
        }

        /// <summary>
        /// If a user navigates to the root of the auth server, the user is automatically redirected
        /// to the main SPA application's homepage.
        /// </summary>
        public IActionResult Index(string forwardUrl = "", string returnUrl = "")
        {
            forwardUrl = UrlEncoder.Default.Encode(forwardUrl);
            returnUrl = UrlEncoder.Default.Encode(_urls.AuthURL.TrimEnd('/') + returnUrl);
            return Redirect($"{_urls.ClientURL}Home/Index?forwardUrl={forwardUrl}&returnUrl={returnUrl}");
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
            return Redirect($"{_urls.ClientURL}Home/Index");
        }
    }
}
