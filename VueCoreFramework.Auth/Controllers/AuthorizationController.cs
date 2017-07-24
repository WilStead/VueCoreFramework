using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VueCoreFramework.Auth.ViewModels;
using VueCoreFramework.Core.Data.Identity;
using VueCoreFramework.Core.Models;

namespace VueCoreFramework.Auth.Controllers
{
    /// <summary>
    /// An MVC controller for handling user authorization tasks.
    /// </summary>
    public class AuthorizationController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Initializes a new instance of <see cref="AuthorizationController"/>.
        /// </summary>
        public AuthorizationController(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        /// <summary>
        /// Called to authorize the current user for a particular operation.
        /// </summary>
        /// <param name="dataType">
        /// The type of data involved in the current operation.
        /// </param>
        /// <param name="operation">An optional operation being performed.</param>
        /// <param name="id">An optional primary key for the item involved in the current operation.</param>
        /// <response code="200">
        /// An <see cref="AuthorizationViewModel"/> indicating whether the current user is authorized.
        /// </response>
        [HttpGet("[controller]/[action]/{dataType}")]
        [ProducesResponseType(typeof(AuthorizationViewModel), 200)]
        public async Task<AuthorizationViewModel> Authorize(string dataType, string operation = "view", string id = null)
        {
            var user = await _userManager.FindByIdAsync(HttpContext.User.FindFirstValue(JwtClaimTypes.Subject));
            if (user == null || user.AdminLocked)
            {
                return new AuthorizationViewModel { Authorization = Authorization.login };
            }

            var roles = await _userManager.GetRolesAsync(user);
            roles.Add(CustomRoles.AllUsers);
            var claims = await _userManager.GetClaimsAsync(user);
            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                claims = claims.Concat(roleClaims).ToList();
            }
            var isAdmin = roles.Any(r => r == CustomRoles.Admin);

            operation = $"permission/data/{operation}";
            var vm = new AuthorizationViewModel
            {
                Authorization = Authorization.GetAuthorization(claims, dataType, operation, id)
            };

            // Admins can share/hide any data, and managers can share/hide any data they have
            // authorization for with their own group. Other users can only share/hide data they own.
            if (vm.Authorization != Authorization.unauthorized)
            {
                if (isAdmin
                    || (!string.IsNullOrEmpty(id)
                    && claims.Any(c => c.Type == CustomClaimTypes.PermissionDataOwner && c.Value == $"{dataType}{{{id}}}")))
                {
                    vm.CanShare = Authorization.shareAny;
                }
                else if (!string.IsNullOrEmpty(id) && claims.Any(c => c.Type == CustomClaimTypes.PermissionGroupManager))
                {
                    vm.CanShare = Authorization.shareGroup;
                }
            }

            return vm;
        }
    }
}
