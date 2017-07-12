using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VueCoreFramework.Data;
using VueCoreFramework.Data.Attributes;
using VueCoreFramework.Extensions;
using VueCoreFramework.Models;
using VueCoreFramework.Models.ViewModels;
using VueCoreFramework.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace VueCoreFramework.Controllers
{
    /// <summary>
    /// An MVC controller for handling data manipulation tasks.
    /// </summary>
    [Authorize]
    [Route("api/[controller]/{dataType}/[action]")]
    public class DataController : Controller
    {
        private readonly AdminOptions _adminOptions;
        private readonly ApplicationDbContext _context;
        private readonly IStringLocalizer<ErrorMessages> _errorLocalizer;
        private readonly ILogger<AccountController> _logger;
        private readonly IStringLocalizer<ResponseMessages> _responseLocalizer;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Initializes a new instance of <see cref="DataController"/>.
        /// </summary>
        public DataController(
            IOptions<AdminOptions> adminOptions,
            ApplicationDbContext context,
            IStringLocalizer<ErrorMessages> localizer,
            ILogger<AccountController> logger,
            IStringLocalizer<ResponseMessages> responseLocalizer,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _adminOptions = adminOptions.Value;
            _context = context;
            _errorLocalizer = localizer;
            _logger = logger;
            _responseLocalizer = responseLocalizer;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        /// <summary>
        /// Called to create a new instance of <paramref name="dataType"/> and add it to the <see
        /// cref="ApplicationDbContext"/> instance.
        /// </summary>
        /// <param name="dataType">The type of entity to add.</param>
        /// <param name="childProp">
        /// An optional navigation property which will be set on the new object.
        /// </param>
        /// <param name="parentId">
        /// The primary key of the entity which will be set on the <paramref name="childProp"/> property.
        /// </param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">A ViewModel representing the newly added item.</response>
        [HttpPost("{childProp?}/{parentId?}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, object>), 200)]
        public async Task<IActionResult> Add(string dataType, string childProp, string parentId, string culture)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
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
            if (AuthorizationController.GetAuthorization(claims, dataType, CustomClaimTypes.PermissionDataAdd) == AuthorizationViewModel.Unauthorized)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.NoPermission, _errorLocalizer[ErrorMessages.PermissionAction_AddNew]]);
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
            }
            PropertyInfo pInfo = null;
            if (!string.IsNullOrEmpty(parentId) && !string.IsNullOrEmpty(childProp))
            {
                pInfo = repository.GetType()
                    .GenericTypeArguments
                    .FirstOrDefault()
                    .GetTypeInfo()
                    .GetProperty(childProp.ToInitialCaps());
            }
            try
            {
                var newItem = await repository.AddAsync(pInfo, parentId, culture);
                var claimValue = $"{dataType}{{{newItem[newItem[repository.PrimaryKeyVMProperty] as string]}}}";
                await _userManager.AddClaimsAsync(user, new Claim[] {
                    new Claim(CustomClaimTypes.PermissionDataOwner, claimValue),
                    new Claim(CustomClaimTypes.PermissionDataAll, claimValue)
                });
                return Json(newItem);
            }
            catch
            {
                return BadRequest(_errorLocalizer[ErrorMessages.SaveItemError]);
            }
        }

        /// <summary>
        /// Called to add an assortment of child entities to a parent entity under the given
        /// navigation property.
        /// </summary>
        /// <param name="dataType">The type of entities to add.</param>
        /// <param name="id">The primary key of the parent entity.</param>
        /// <param name="childProp">The navigation property to which the children will be added.</param>
        /// <param name="childIds">The primary keys of the child entities which will be added.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost("{id}/{childProp}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> AddChildrenToCollection(string dataType, string id, string childProp, [FromBody]string[] childIds)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingIdError]);
            }
            if (string.IsNullOrEmpty(childProp))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingDataError]);
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
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
            if (AuthorizationController.GetAuthorization(claims, dataType, CustomClaimTypes.PermissionDataEdit, id) == AuthorizationViewModel.Unauthorized)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.NoPermission, _errorLocalizer[ErrorMessages.PermissionAction_EditItem]]);
            }
            var pInfo = repository.GetType()
                .GenericTypeArguments
                .FirstOrDefault()
                .GetTypeInfo()
                .GetProperty(childProp.ToInitialCaps());
            if (pInfo == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingDataError]);
            }
            try
            {
                await repository.AddChildrenToCollectionAsync(id, pInfo, childIds);
                return Ok();
            }
            catch
            {
                return BadRequest(_errorLocalizer[ErrorMessages.DataError]);
            }
        }

        /// <summary>
        /// Called to duplicate an entity in the <see cref="ApplicationDbContext"/>. Returns a
        /// ViewModel representing the new copy.
        /// </summary>
        /// <param name="dataType">The type of entity to find.</param>
        /// <param name="id">The primary key of the entity to be found.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">A ViewModel representing the newly added item.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, object>), 200)]
        public async Task<IActionResult> Duplicate(string dataType, string id, string culture)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingIdError]);
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
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
            if (AuthorizationController.GetAuthorization(claims, dataType, CustomClaimTypes.PermissionDataAdd, id) == AuthorizationViewModel.Unauthorized)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.NoPermission, _errorLocalizer[ErrorMessages.PermissionAction_AddNew]]);
            }
            object newItem = null;
            try
            {
                newItem = await repository.DuplicateAsync(id, culture);
            }
            catch
            {
                return BadRequest(_errorLocalizer[ErrorMessages.DataError]);
            }
            if (newItem == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.DataError]);
            }
            return Json(newItem);
        }

        /// <summary>
        /// Called to find an entity with the given primary key value and return a ViewModel for that
        /// entity. If no entity is found, an empty ViewModel is returned (not null).
        /// </summary>
        /// <param name="dataType">The type of entity to find.</param>
        /// <param name="id">The primary key of the entity to be found.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="404">No such item.</response>
        /// <response code="200">A ViewModel representing the found item.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(IDictionary<string, object>), 200)]
        public async Task<IActionResult> Find(string dataType, string id, string culture)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingIdError]);
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
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
            if (AuthorizationController.GetAuthorization(claims, dataType, CustomClaimTypes.PermissionDataView, id) == AuthorizationViewModel.Unauthorized)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.NoPermission, _errorLocalizer[ErrorMessages.PermissionAction_ViewItem]]);
            }
            object item = null;
            try
            {
                item = await repository.FindAsync(id, culture);
            }
            catch
            {
                return BadRequest(_errorLocalizer[ErrorMessages.DataError]);
            }
            if (item == null)
            {
                return NotFound();
            }
            return Json(item);
        }

        /// <summary>
        /// Called to retrieve ViewModels representing all the entities in the <see
        /// cref="ApplicationDbContext"/>'s set.
        /// </summary>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">A ViewModel representing the found item.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, object>), 200)]
        public async Task<IActionResult> GetAll(string dataType, string culture)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
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
            if (AuthorizationController.GetAuthorization(claims, dataType, CustomClaimTypes.PermissionDataView) == AuthorizationViewModel.Unauthorized)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.NoPermission, _errorLocalizer[ErrorMessages.PermissionAction_ViewItems]]);
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
            }
            return Json(await repository.GetAllAsync(culture));
        }

        /// <summary>
        /// Called to retrieve all the primary keys of the entities in a given relationship.
        /// </summary>
        /// <param name="dataType">The type of the parent entity.</param>
        /// <param name="id">The primary key of the parent entity.</param>
        /// <param name="childProp">The navigation property of the relationship on the parent entity.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">A list of child primary keys.</response>
        [HttpGet("{id}/{childProp}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 200)]
        public async Task<IActionResult> GetAllChildIds(
            string dataType,
            string id,
            string childProp)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingIdError]);
            }
            if (string.IsNullOrEmpty(childProp))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingDataError]);
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
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
            if (AuthorizationController.GetAuthorization(claims, dataType, CustomClaimTypes.PermissionDataView) == AuthorizationViewModel.Unauthorized)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.NoPermission, _errorLocalizer[ErrorMessages.PermissionAction_ViewItems]]);
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
            }
            var pInfo = repository.GetType()
                .GenericTypeArguments
                .FirstOrDefault()
                .GetTypeInfo()
                .GetProperty(childProp.ToInitialCaps());
            if (pInfo == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingDataError]);
            }
            try
            {
                var childIds = await repository.GetAllChildIdsAsync(id, pInfo);
                return Json(childIds);
            }
            catch
            {
                return BadRequest(_errorLocalizer[ErrorMessages.DataError]);
            }
        }

        /// <summary>
        /// Called to retrieve the primary key of a child entity in the given relationship.
        /// </summary>
        /// <param name="dataType">The type of the parent entity.</param>
        /// <param name="id">The primary key of the parent entity.</param>
        /// <param name="childProp">The navigation property of the relationship.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">The child's primary key.</response>
        [HttpGet("{id}/{childProp}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 200)]
        public async Task<IActionResult> GetChildId(string dataType, string id, string childProp)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingIdError]);
            }
            if (string.IsNullOrEmpty(childProp))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingDataError]);
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
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
            if (AuthorizationController.GetAuthorization(claims, dataType, CustomClaimTypes.PermissionDataView, id) == AuthorizationViewModel.Unauthorized)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.NoPermission, _errorLocalizer[ErrorMessages.PermissionAction_ViewItem]]);
            }
            var pInfo = repository.GetType()
                .GenericTypeArguments
                .FirstOrDefault()
                .GetTypeInfo()
                .GetProperty(childProp.ToInitialCaps());
            if (pInfo == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingDataError]);
            }
            try
            {
                var childId = await repository.GetChildIdAsync(id, pInfo);
                return Ok(childId);
            }
            catch
            {
                return BadRequest(_errorLocalizer[ErrorMessages.DataError]);
            }
        }

        /// <summary>
        /// Called to retrieve a page of child entities in a given relationship.
        /// </summary>
        /// <param name="dataType">The type of the parent entity.</param>
        /// <param name="id">The primary key of the parent entity.</param>
        /// <param name="childProp">The navigation property of the relationship on the parent entity.</param>
        /// <param name="search">
        /// An optional search term which will filter the results. Any string or numeric property
        /// with matching text will be included.
        /// </param>
        /// <param name="sortBy">
        /// An optional property name which will be used to sort the items before calculating the
        /// page contents.
        /// </param>
        /// <param name="descending">
        /// Indicates whether the sort is descending; if false, the sort is ascending.
        /// </param>
        /// <param name="page">The page number requested.</param>
        /// <param name="rowsPerPage">The number of items per page.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">
        /// A list of ViewModels representing the child objects on the requested page.
        /// </response>
        [HttpGet("{id}/{childProp}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, object>), 200)]
        public async Task<IActionResult> GetChildPage(
            string dataType,
            string id,
            string childProp,
            string search,
            string sortBy,
            bool descending,
            int page,
            int rowsPerPage,
            string culture)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingIdError]);
            }
            if (string.IsNullOrEmpty(childProp))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingDataError]);
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
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
            if (AuthorizationController.GetAuthorization(claims, dataType, CustomClaimTypes.PermissionDataView, id) == AuthorizationViewModel.Unauthorized)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.NoPermission, _errorLocalizer[ErrorMessages.PermissionAction_ViewItem]]);
            }
            var pInfo = repository.GetType()
                .GenericTypeArguments
                .FirstOrDefault()
                .GetTypeInfo()
                .GetProperty(childProp.ToInitialCaps());
            if (pInfo == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingDataError]);
            }
            var childType = repository.EntityType.FindNavigation(pInfo).GetTargetType().ClrType;
            if (AuthorizationController.GetAuthorization(claims, childType.Name, CustomClaimTypes.PermissionDataView) == AuthorizationViewModel.Unauthorized)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.NoPermission, _errorLocalizer[ErrorMessages.PermissionAction_ViewItems]]);
            }
            try
            {
                var results = await repository.GetChildPageAsync(id, pInfo, search, sortBy, descending, page, rowsPerPage, claims, culture);
                return Json(results);
            }
            catch
            {
                return BadRequest(_errorLocalizer[ErrorMessages.DataError]);
            }
        }

        /// <summary>
        /// Called to retrieve the total number of child entities in the given relationship.
        /// </summary>
        /// <param name="dataType">The type of the parent entity.</param>
        /// <param name="id">The primary key of the parent entity.</param>
        /// <param name="childProp">The navigation property of the relationship on the parent entity.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">The total.</response>
        [HttpGet("{id}/{childProp}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, long>), 200)]
        public async Task<IActionResult> GetChildTotal(string dataType, string id, string childProp)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingIdError]);
            }
            if (string.IsNullOrEmpty(childProp))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingDataError]);
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
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
            if (AuthorizationController.GetAuthorization(claims, dataType, CustomClaimTypes.PermissionDataView, id) == AuthorizationViewModel.Unauthorized)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.NoPermission, _errorLocalizer[ErrorMessages.PermissionAction_ViewItem]]);
            }
            var pInfo = repository.GetType()
                .GenericTypeArguments
                .FirstOrDefault()
                .GetTypeInfo()
                .GetProperty(childProp.ToInitialCaps());
            if (pInfo == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingDataError]);
            }
            try
            {
                return Ok(await repository.GetChildTotalAsync(id, pInfo));
            }
            catch
            {
                return BadRequest(_errorLocalizer[ErrorMessages.DataError]);
            }
        }

        /// <summary>
        /// Called to retrieve a list of all entities which are not MenuClass types.
        /// </summary>
        /// <response code="400">Bad request.</response>
        /// <response code="200">The list of type info.</response>
        [AllowAnonymous]
        [HttpGet("/api/[controller]/[action]")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, object>), 200)]
        public IActionResult GetChildTypes()
        {
            try
            {
                var types = _context.Model.GetEntityTypes()
                    .Where(e =>
                        e.Name != nameof(_context.Logs)
                        && e.Name != nameof(_context.Messages)
                        && e.Name != nameof(_context.RoleClaims)
                        && e.Name != nameof(_context.Roles)
                        && e.Name != nameof(_context.UserClaims)
                        && e.Name != nameof(_context.UserLogins)
                        && e.Name != nameof(_context.UserRoles)
                        && e.Name != nameof(_context.Users)
                        && e.Name != nameof(_context.UserTokens))
                    .Select(t => t.ClrType);
                IDictionary<string, dynamic> classes = new Dictionary<string, dynamic>();
                foreach (var type in types)
                {
                    var attr = type.GetTypeInfo().GetCustomAttribute<MenuClassAttribute>();
                    if (attr == null)
                    {
                        var childAttr = type.GetTypeInfo().GetCustomAttribute<DataClassAttribute>();
                        if (childAttr == null)
                        {
                            // False fontAwesome used as a placeholder property since an empty one
                            // isn't serialized, and the key must be present.
                            classes.Add(type.Name, new { fontAwesome = false });
                        }
                        else
                        {
                            classes.Add(type.Name,
                                new
                                {
                                    iconClass = childAttr.IconClass,
                                    fontAwesome = childAttr.FontAwesome,
                                    dashboardTableContent = childAttr.DashboardTableContent,
                                    dashboardFormContent = childAttr.DashboardFormContent
                                });
                        }
                    }
                }
                return Json(classes);
            }
            catch
            {
                return BadRequest(_errorLocalizer[ErrorMessages.DataError]);
            }
        }

        /// <summary>
        /// Called to retrieve a list of <see cref="FieldDefinition"/>s for the given data type.
        /// </summary>
        /// <response code="400">Bad request.</response>
        /// <response code="200">The list of <see cref="FieldDefinition"/>s.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, object>), 200)]
        public IActionResult GetFieldDefinitions(string dataType)
        {
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
            }
            try
            {
                return Json(repository.FieldDefinitions);
            }
            catch
            {
                return BadRequest(_errorLocalizer[ErrorMessages.DataError]);
            }
        }

        /// <summary>
        /// Called to retrieve the set of entities with the given paging parameters.
        /// </summary>
        /// <param name="dataType">The type of the entity.</param>
        /// <param name="search">
        /// An optional search term which will filter the results. Any string or numeric property
        /// with matching text will be included.
        /// </param>
        /// <param name="sortBy">
        /// An optional property name which will be used to sort the items before calculating the
        /// page contents.
        /// </param>
        /// <param name="descending">
        /// Indicates whether the sort is descending; if false, the sort is ascending.
        /// </param>
        /// <param name="page">The page number requested.</param>
        /// <param name="rowsPerPage">The number of items per page.</param>
        /// <param name="except">
        /// The primary keys of items which should be excluded from the results before calculating
        /// the page contents.
        /// </param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">
        /// A list of ViewModels representing the entities on the requested page.
        /// </response>
        [HttpPost]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, object>), 200)]
        public async Task<IActionResult> GetPage(
            string dataType,
            string search,
            string sortBy,
            bool descending,
            int page,
            int rowsPerPage,
            [FromBody]string[] except,
            string culture)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
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
            if (AuthorizationController.GetAuthorization(claims, dataType, CustomClaimTypes.PermissionDataView) == AuthorizationViewModel.Unauthorized)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.NoPermission, _errorLocalizer[ErrorMessages.PermissionAction_ViewItems]]);
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
            }
            try
            {
                return Json(await repository.GetPageAsync(search, sortBy, descending, page, rowsPerPage, except, claims, culture));
            }
            catch
            {
                return BadRequest(_errorLocalizer[ErrorMessages.DataError]);
            }
        }

        /// <summary>
        /// Called to retrieve the total number of entities of the given data type.
        /// </summary>
        /// <param name="dataType">The type of the entity.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">The total.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, long>), 200)]
        public async Task<IActionResult> GetTotal(string dataType)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
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
            if (AuthorizationController.GetAuthorization(claims, dataType, CustomClaimTypes.PermissionDataView) == AuthorizationViewModel.Unauthorized)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.NoPermission, _errorLocalizer[ErrorMessages.PermissionAction_ViewItems]]);
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
            }
            return Ok(await repository.GetTotalAsync());
        }

        /// <summary>
        /// Called to retrieve a list of all entities which are MenuClass types.
        /// </summary>
        /// <response code="400">Bad request.</response>
        /// <response code="200">The list of type info.</response>
        [AllowAnonymous]
        [HttpGet("/api/[controller]/[action]")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, object>), 200)]
        public IActionResult GetTypes()
        {
            try
            {
                var types = _context.Model.GetEntityTypes()
                    .Where(e =>
                        e.Name != nameof(_context.Logs)
                        && e.Name != nameof(_context.Messages)
                        && e.Name != nameof(_context.RoleClaims)
                        && e.Name != nameof(_context.Roles)
                        && e.Name != nameof(_context.UserClaims)
                        && e.Name != nameof(_context.UserLogins)
                        && e.Name != nameof(_context.UserRoles)
                        && e.Name != nameof(_context.Users)
                        && e.Name != nameof(_context.UserTokens))
                    .Select(t => t.ClrType);
                IDictionary<string, dynamic> classes = new Dictionary<string, dynamic>();
                foreach (var type in types)
                {
                    var attr = type.GetTypeInfo().GetCustomAttribute<MenuClassAttribute>();
                    if (attr != null)
                    {
                        classes.Add(type.Name,
                            new
                            {
                                category = string.IsNullOrEmpty(attr.Category) ? "/" : attr.Category,
                                iconClass = attr.IconClass,
                                fontAwesome = attr.FontAwesome,
                                dashboardTableContent = attr.DashboardTableContent,
                                dashboardFormContent = attr.DashboardFormContent
                            });
                    }
                }
                return Json(classes);
            }
            catch
            {
                return BadRequest(_errorLocalizer[ErrorMessages.DataError]);
            }
        }

        /// <summary>
        /// Called to remove an entity from the <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <param name="dataType">The type of entity to remove.</param>
        /// <param name="id">The primary key of the entity to remove.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost("{id}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Remove(string dataType, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingIdError]);
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
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
            if (AuthorizationController.GetAuthorization(claims, dataType, CustomClaimTypes.PermissionDataAll, id) == AuthorizationViewModel.Unauthorized)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.NoPermission, _errorLocalizer[ErrorMessages.PermissionAction_RemoveItem]]);
            }
            try
            {
                await repository.RemoveAsync(id);
                _context.UserClaims.RemoveRange(_context.UserClaims.Where(c => c.ClaimValue == $"{dataType}{{{id}}}"));
                _context.RoleClaims.RemoveRange(_context.RoleClaims.Where(c => c.ClaimValue == $"{dataType}{{{id}}}"));
                await _context.SaveChangesAsync();
            }
            catch
            {
                return BadRequest(_errorLocalizer[ErrorMessages.RemoveItemError]);
            }
            return Ok();
        }

        /// <summary>
        /// Called to remove an assortment of child entities from a parent entity under the given
        /// navigation property.
        /// </summary>
        /// <param name="dataType">The type of the parent entity.</param>
        /// <param name="id">The primary key of the parent entity.</param>
        /// <param name="childProp">The navigation property from which the children will be removed.</param>
        /// <param name="childIds">The primary keys of the child entities which will be removed.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost("{id}/{childProp}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> RemoveChildrenFromCollection(string dataType, string id, string childProp, [FromBody]string[] childIds)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingIdError]);
            }
            if (string.IsNullOrEmpty(childProp))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingDataError]);
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
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
            if (AuthorizationController.GetAuthorization(claims, dataType, CustomClaimTypes.PermissionDataEdit, id) == AuthorizationViewModel.Unauthorized)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.NoPermission, _errorLocalizer[ErrorMessages.PermissionAction_EditItem]]);
            }
            var pInfo = repository.GetType()
                .GenericTypeArguments
                .FirstOrDefault()
                .GetTypeInfo()
                .GetProperty(childProp.ToInitialCaps());
            if (pInfo == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingDataError]);
            }
            try
            {
                await repository.RemoveChildrenFromCollectionAsync(id, pInfo, childIds);
                return Ok();
            }
            catch
            {
                return BadRequest(_errorLocalizer[ErrorMessages.DataError]);
            }
        }

        /// <summary>
        /// Called to terminate a relationship between two entities. If the child entity is made an
        /// orphan by the removal and is not a MenuClass object, it is then removed from the <see
        /// cref="ApplicationDbContext"/> entirely.
        /// </summary>
        /// <param name="dataType">The type of the child entity.</param>
        /// <param name="id">The primary key of the child entity whose relationship is being severed.</param>
        /// <param name="childProp">The navigation property of the relationship being severed.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost("{id}/{childProp}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> RemoveFromParent(string dataType, string id, string childProp)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingIdError]);
            }
            if (string.IsNullOrEmpty(childProp))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingDataError]);
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
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
            if (AuthorizationController.GetAuthorization(claims, dataType, CustomClaimTypes.PermissionDataEdit, id) == AuthorizationViewModel.Unauthorized)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.NoPermission, _errorLocalizer[ErrorMessages.PermissionAction_EditItem]]);
            }
            var pInfo = repository.GetType()
                .GenericTypeArguments
                .FirstOrDefault()
                .GetTypeInfo()
                .GetProperty(childProp.ToInitialCaps());
            if (pInfo == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingDataError]);
            }
            try
            {
                var removed = await repository.RemoveFromParentAsync(id, pInfo);
                if (removed)
                {
                    _context.UserClaims.RemoveRange(_context.UserClaims.Where(c => c.ClaimValue == $"{dataType}{{{id}}}"));
                    _context.RoleClaims.RemoveRange(_context.RoleClaims.Where(c => c.ClaimValue == $"{dataType}{{{id}}}"));
                    await _context.SaveChangesAsync();
                }
                return Ok();
            }
            catch
            {
                return BadRequest(_errorLocalizer[ErrorMessages.RemoveItemError]);
            }
        }

        /// <summary>
        /// Called to remove a collection of entities from the <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <param name="dataType">The type of entity to remove.</param>
        /// <param name="ids">The primary keys of the entities to remove.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> RemoveRange(string dataType, [FromBody]List<string> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingIdError]);
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
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
            foreach (var id in ids)
            {
                if (AuthorizationController.GetAuthorization(claims, dataType, CustomClaimTypes.PermissionDataAll, id) == AuthorizationViewModel.Unauthorized)
                {
                    return StatusCode(403, _errorLocalizer[ErrorMessages.NoPermission, _errorLocalizer[ErrorMessages.PermissionAction_RemoveItems]]);
                }
            }
            try
            {
                await repository.RemoveRangeAsync(ids);
                _context.UserClaims.RemoveRange(_context.UserClaims.Where(c => ids.Any(id => c.ClaimValue == $"{dataType}{{{id}}}")));
                _context.RoleClaims.RemoveRange(_context.RoleClaims.Where(c => ids.Any(id => c.ClaimValue == $"{dataType}{{{id}}}")));
                await _context.SaveChangesAsync();
            }
            catch
            {
                return BadRequest(_errorLocalizer[ErrorMessages.RemoveItemsError]);
            }
            return Ok();
        }

        /// <summary>
        /// Called to terminate a relationship for multiple entities. If any child entity is made an
        /// orphan by the removal and is not a MenuClass object, it is then removed from the <see
        /// cref="ApplicationDbContext"/> entirely.
        /// </summary>
        /// <param name="dataType">The type of the child entity.</param>
        /// <param name="childProp">The navigation property of the relationship being severed.</param>
        /// <param name="ids">
        /// The primary keys of child entities whose relationships are being severed.
        /// </param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost("{id}/{childProp}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> RemoveRangeFromParent(string dataType, string childProp, [FromBody]List<string> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingIdError]);
            }
            if (string.IsNullOrEmpty(childProp))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingDataError]);
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
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
            List<Guid> guids = new List<Guid>();
            foreach (var id in ids)
            {
                if (AuthorizationController.GetAuthorization(claims, dataType, CustomClaimTypes.PermissionDataEdit, id) == AuthorizationViewModel.Unauthorized)
                {
                    return StatusCode(403, _errorLocalizer[ErrorMessages.NoPermission, _errorLocalizer[ErrorMessages.PermissionAction_EditItems]]);
                }
            }
            var pInfo = repository.GetType()
                .GenericTypeArguments
                .FirstOrDefault()
                .GetTypeInfo()
                .GetProperty(childProp.ToInitialCaps());
            if (pInfo == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingDataError]);
            }
            try
            {
                var removedIds = await repository.RemoveRangeFromParentAsync(ids, pInfo);
                _context.UserClaims.RemoveRange(_context.UserClaims.Where(c => removedIds.Any(id => c.ClaimValue == $"{dataType}{{{id}}}")));
                _context.RoleClaims.RemoveRange(_context.RoleClaims.Where(c => removedIds.Any(id => c.ClaimValue == $"{dataType}{{{id}}}")));
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return BadRequest(_errorLocalizer[ErrorMessages.RemoveItemsError]);
            }
        }

        /// <summary>
        /// Called to create a relationship between two entities, replacing another entity which was
        /// previously in that relationship with another one. If the replaced entity is made an
        /// orphan by the removal and is not a MenuClass object, it is then removed from the <see
        /// cref="ApplicationDbContext"/> entirely.
        /// </summary>
        /// <param name="dataType">The type of the parent entity.</param>
        /// <param name="parentId">The primary key of the parent entity in the relationship.</param>
        /// <param name="newChildId">
        /// The primary key of the new child entity entering into the relationship.
        /// </param>
        /// <param name="childProp">The navigation property of the relationship on the child entity.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost("{parentId}/{newChildId}/{childProp}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ReplaceChild(string dataType, string parentId, string newChildId, string childProp)
        {
            if (string.IsNullOrEmpty(parentId) || string.IsNullOrEmpty(newChildId))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingIdError]);
            }
            if (string.IsNullOrEmpty(childProp))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingDataError]);
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
            }
            var pInfo = repository.GetType()
                .GenericTypeArguments
                .FirstOrDefault()
                .GetTypeInfo()
                .GetProperty(childProp.ToInitialCaps());
            if (pInfo == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingDataError]);
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
            if (AuthorizationController.GetAuthorization(claims, dataType, CustomClaimTypes.PermissionDataEdit, parentId) == AuthorizationViewModel.Unauthorized)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.NoPermission, _errorLocalizer[ErrorMessages.PermissionAction_EditItem]]);
            }
            try
            {
                var replacedId = await repository.ReplaceChildAsync(parentId, newChildId, pInfo);
                if (!string.IsNullOrEmpty(replacedId))
                {
                    _context.UserClaims.RemoveRange(_context.UserClaims.Where(c => c.ClaimValue == $"{dataType}{{{replacedId}}}"));
                    _context.RoleClaims.RemoveRange(_context.RoleClaims.Where(c => c.ClaimValue == $"{dataType}{{{replacedId}}}"));
                    await _context.SaveChangesAsync();
                }
                return Ok();
            }
            catch
            {
                return BadRequest(_errorLocalizer[ErrorMessages.AddItemError]);
            }
        }

        /// <summary>
        /// Called to create a relationship between two entities, replacing another entity which was
        /// previously in that relationship with a new entity. If the replaced entity is made an
        /// orphan by the removal and is not a MenuClass object, it is then removed from the <see
        /// cref="ApplicationDbContext"/> entirely.
        /// </summary>
        /// <param name="dataType">The type of the parent entity.</param>
        /// <param name="parentId">The primary key of the parent entity in the relationship.</param>
        /// <param name="childProp">The navigation property of the relationship on the child entity.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">A ViewModel representing the newly added item.</response>
        [HttpPost("{parentId}/{childProp}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, object>), 200)]
        public async Task<IActionResult> ReplaceChildWithNew(string dataType, string parentId, string childProp, string culture)
        {
            if (string.IsNullOrEmpty(parentId))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingIdError]);
            }
            if (string.IsNullOrEmpty(childProp))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingDataError]);
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
            }
            var pInfo = repository.GetType()
                .GenericTypeArguments
                .FirstOrDefault()
                .GetTypeInfo()
                .GetProperty(childProp.ToInitialCaps());
            if (pInfo == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MissingDataError]);
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
            if (AuthorizationController.GetAuthorization(claims, dataType, CustomClaimTypes.PermissionDataEdit, parentId) == AuthorizationViewModel.Unauthorized)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.NoPermission, _errorLocalizer[ErrorMessages.PermissionAction_EditItem]]);
            }
            try
            {
                var (newItem, replacedId) = await repository.ReplaceChildWithNewAsync(parentId, pInfo, culture);
                var claimValue = $"{dataType}{{{newItem[newItem[repository.PrimaryKeyVMProperty] as string]}}}";
                await _userManager.AddClaimsAsync(user, new Claim[] {
                    new Claim(CustomClaimTypes.PermissionDataOwner, claimValue),
                    new Claim(CustomClaimTypes.PermissionDataAll, claimValue)
                });
                if (!string.IsNullOrEmpty(replacedId))
                {
                    _context.UserClaims.RemoveRange(_context.UserClaims.Where(c => c.ClaimValue == $"{dataType}{{{replacedId}}}"));
                    _context.RoleClaims.RemoveRange(_context.RoleClaims.Where(c => c.ClaimValue == $"{dataType}{{{replacedId}}}"));
                    await _context.SaveChangesAsync();
                }
                return Json(newItem);
            }
            catch
            {
                return BadRequest(_errorLocalizer[ErrorMessages.AddItemError]);
            }
        }

        internal static bool TryGetRepository(ApplicationDbContext context, string dataType, out IRepository repository)
        {
            repository = null;
            if (string.IsNullOrEmpty(dataType))
            {
                return false;
            }
            var entity = context.Model.GetEntityTypes().FirstOrDefault(e => e.Name.Substring(e.Name.LastIndexOf('.') + 1) == dataType);
            if (entity == null)
            {
                return false;
            }
            var type = entity.ClrType;
            if (type == null)
            {
                return false;
            }
            repository = context.GetRepositoryForType(type);
            return true;
        }

        private bool TryResolveObject(string dataType, JObject item, out object obj, out Type type)
        {
            obj = null;
            type = null;
            if (item == null)
            {
                return false;
            }
            if (string.IsNullOrEmpty(dataType))
            {
                return false;
            }
            var entity = _context.Model.GetEntityTypes().FirstOrDefault(e => e.Name.Substring(e.Name.LastIndexOf('.') + 1) == dataType);
            if (entity == null)
            {
                return false;
            }
            type = entity.ClrType;
            try
            {
                obj = item.ToObject(type);
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Called to update an entity in the <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <param name="dataType">The type of entity to update.</param>
        /// <param name="item">The item to update.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">A ViewModel representing the updated item.</response>
        [HttpPost]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, object>), 200)]
        public async Task<IActionResult> Update(string dataType, string culture, [FromBody]JObject item)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidUserError]);
            }
            if (user.AdminLocked)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.LockedAccount, _adminOptions.AdminEmailAddress]);
            }
            if (!TryResolveObject(dataType, item, out object obj, out Type type))
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidDataTypeError]);
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
            IRepository repository = _context.GetRepositoryForType(type);
            var id = repository.PrimaryKey.PropertyInfo.GetValue(obj).ToString();
            if (AuthorizationController.GetAuthorization(claims, dataType, CustomClaimTypes.PermissionDataEdit, id) == AuthorizationViewModel.Unauthorized)
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.NoPermission, _errorLocalizer[ErrorMessages.PermissionAction_EditItem]]);
            }
            try
            {
                return base.Json(await repository.UpdateAsync(obj, culture));
            }
            catch
            {
                return BadRequest(_errorLocalizer[ErrorMessages.SaveItemError]);
            }
        }
    }
}
