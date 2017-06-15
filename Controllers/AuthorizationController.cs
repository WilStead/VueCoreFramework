using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MVCCoreVue.Data;
using MVCCoreVue.Data.Attributes;
using MVCCoreVue.Extensions;
using MVCCoreVue.Models;
using MVCCoreVue.Services;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MVCCoreVue.Controllers
{
    /// <summary>
    /// An MVC controller for handling user authorization tasks.
    /// </summary>
    [Route("api/[controller]/[action]")]
    public class AuthorizationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly TokenProviderOptions _tokenOptions;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Initializes a new instance of <see cref="AuthorizationController"/>.
        /// </summary>
        public AuthorizationController(
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IOptions<TokenProviderOptions> tokenOptions)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _tokenOptions = tokenOptions.Value;
        }

        /// <summary>
        /// Called to authorize the current user for a particular operation, or simply to
        /// authenticate the user if no oepration is specified.
        /// </summary>
        /// <param name="dataType">
        /// An optional name indicating the type of data involved in the current operation.
        /// </param>
        /// <param name="operation">An optional operation being performed.</param>
        /// <param name="id">An optional primary key for the item involved in the current operation.</param>
        /// <returns>
        /// An <see cref="AuthorizationViewModel"/> indicating whether the current user is authorized
        /// (or authenticated).
        /// </returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Authorize(string dataType = null, string operation = "View", string id = null)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.AdminLocked)
            {
                return Json(new AuthorizationViewModel { Authorization = AuthorizationViewModel.Unauthorized });
            }

            var token = AccountController.GetLoginToken(email, user, _userManager, _tokenOptions);

            // If no specific data is being requested, just being a recognized user is sufficient authorization.
            if (string.IsNullOrEmpty(dataType))
            {
                return Json(new AuthorizationViewModel { Email = user.Email, Token = token, Authorization = AuthorizationViewModel.Authorized });
            }
            dataType = dataType.ToInitialCaps();

            var claims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                claims = claims.Concat(roleClaims).ToList();
            }

            // First authorization for all data is checked.
            if (claims.Any(c => c.Type == CustomClaimTypes.PermissionDataAll && c.Value == CustomClaimTypes.PermissionAll))
            {
                return Json(new AuthorizationViewModel { Email = user.Email, Token = token, Authorization = AuthorizationViewModel.Authorized });
            }

            // If not authorized for all data, authorization for the specific operation on all data is checked.
            // In the absence of a specific operation, the default action is View.
            var claimType = operation.ToInitialCaps();
            if (claims.Any(c => c.Type == claimType && c.Value == CustomClaimTypes.PermissionAll))
            {
                return Json(new AuthorizationViewModel { Email = user.Email, Token = token, Authorization = AuthorizationViewModel.Authorized });
            }

            // If not authorized for the operation on all data, authorization for the specific data type is checked.
            if (string.IsNullOrEmpty(dataType))
            {
                return Json(new AuthorizationViewModel { Email = user.Email, Token = token, Authorization = AuthorizationViewModel.Unauthorized });
            }
            var entity = _context.Model.GetEntityTypes().FirstOrDefault(e => e.Name.Substring(e.Name.LastIndexOf('.') + 1) == dataType);
            if (entity == null)
            {
                return Json(new AuthorizationViewModel { Email = user.Email, Token = token, Authorization = AuthorizationViewModel.Unauthorized });
            }
            var type = entity.ClrType;
            if (type == null)
            {
                return Json(new AuthorizationViewModel { Email = user.Email, Token = token, Authorization = AuthorizationViewModel.Unauthorized });
            }
            // First check whether the datatype requires no permissions.
            var attrDefaultPermission = type.GetTypeInfo().GetCustomAttribute<DefaultPermissionAttribute>();
            if (attrDefaultPermission?.HasDefaultAllPermissions == true)
            {
                return Json(new AuthorizationViewModel { Email = user.Email, Token = token, Authorization = AuthorizationViewModel.Authorized });
            }
            // If not, see if it has default authorization for the specific operation
            if (attrDefaultPermission?.DefaultPermissions?.Split(';').Any(s => s == claimType) == true)
            {
                return Json(new AuthorizationViewModel { Email = user.Email, Token = token, Authorization = AuthorizationViewModel.Authorized });
            }

            // If not, authorization for either all operations, or the specific operation is checked.
            var claimValue = dataType.ToInitialCaps();
            if (claims.Any(c => c.Value == claimValue
                && (c.Type == CustomClaimTypes.PermissionDataAll || c.Type == claimType)))
            {
                return Json(new AuthorizationViewModel { Email = user.Email, Token = token, Authorization = AuthorizationViewModel.Authorized });
            }

            // If not authorized for the operation on the data type and an id is provided, the specific item is checked.
            if (!string.IsNullOrEmpty(id))
            {
                // First, the item is checked to see if all operations or the specific operation is allowed for all users.
                if (Guid.TryParse(id, out Guid guid) && DataController.TryGetRepository(_context, dataType, out IRepository repository))
                {
                    var item = await repository.FindItemAsync(guid);
                    if (item?.AllPermissions?.Equals(CustomClaimTypes.PermissionDataAll) == true
                        || item?.AllPermissions?.Split(';').Any(s => s == claimType) == true)
                    {
                        return Json(new AuthorizationViewModel { Email = user.Email, Token = token, Authorization = AuthorizationViewModel.Authorized });
                    }
                }

                // If not, authorization for either all operations or the specific operation is checked.
                if (claims.Any(c => c.Value == id
                    && (c.Type == CustomClaimTypes.PermissionDataAll || c.Type == claimType)))
                {
                    return Json(new AuthorizationViewModel { Email = user.Email, Token = token, Authorization = AuthorizationViewModel.Authorized });
                }
            }

            // No authorizations found.
            return Json(new AuthorizationViewModel { Email = user.Email, Token = token, Authorization = AuthorizationViewModel.Unauthorized });
        }
    }
}
