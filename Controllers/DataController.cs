using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VueCoreFramework.Data;
using VueCoreFramework.Data.Attributes;
using VueCoreFramework.Extensions;
using VueCoreFramework.Models;
using VueCoreFramework.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace VueCoreFramework.Controllers
{
    /// <summary>
    /// An MVC controller for handling data manipulation tasks.
    /// </summary>
    [Authorize]
    [Route("api/[controller]/{dataType}/[action]")]
    public class DataController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Initializes a new instance of <see cref="DataController"/>.
        /// </summary>
        public DataController(
            ILogger<AccountController> logger,
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        /// <summary>
        /// Called to create a new instance of <see cref="T"/> and add it to the <see
        /// cref="ApplicationDbContext"/> instance.
        /// </summary>
        /// <param name="dataType">The type of entity to add.</param>
        /// <param name="childProp">
        /// An optional navigation property which will be set on the new object.
        /// </param>
        /// <param name="parentId">
        /// The primary key of the entity which will be set on the <paramref name="childProp"/> property.
        /// </param>
        /// <returns>
        /// Redirect to an error page in the event of a bad request; an error if there is a problem;
        /// or a ViewModel representing the newly added item (as JSON).
        /// </returns>
        [HttpPost("{childProp}/{parentId}")]
        public async Task<IActionResult> Add(string dataType, string childProp, string parentId)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
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
            if (!AuthorizationController.IsAuthorized(claims, dataType, CustomClaimTypes.PermissionDataAdd))
            {
                return Json(new { error = "You don't have permission to add new items of this type." });
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var hasParent = Guid.TryParse(parentId, out Guid guid);
            PropertyInfo pInfo = null;
            if (hasParent && !string.IsNullOrEmpty(childProp))
            {
                pInfo = repository.GetType()
                    .GenericTypeArguments
                    .FirstOrDefault()
                    .GetTypeInfo()
                    .GetProperty(childProp.ToInitialCaps());
            }
            try
            {
                var newItem = await repository.AddAsync(pInfo, hasParent ? guid : (Guid?)null);
                var claimValue = $"{dataType}{{{newItem["id"]}}}";
                await _userManager.AddClaimsAsync(user, new Claim[] {
                    new Claim(CustomClaimTypes.PermissionDataOwner, claimValue),
                    new Claim(CustomClaimTypes.PermissionDataAll, claimValue)
                });
                return Json(new { data = newItem });
            }
            catch
            {
                return Json(new { error = "Item could not be saved." });
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
        /// <returns>
        /// Redirect to an error page in the event of a bad request; an error if there is a problem;
        /// or an OK result.
        /// </returns>
        [HttpPost("{id}/{childProp}")]
        public async Task<IActionResult> AddChildrenToCollection(string dataType, string id, string childProp, [FromBody]string[] childIds)
        {
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (!Guid.TryParse(id, out Guid guid))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
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
            if (!AuthorizationController.IsAuthorized(claims, dataType, CustomClaimTypes.PermissionDataEdit, id))
            {
                return Json(new { error = "You don't have permission to edit this item." });
            }
            if (string.IsNullOrEmpty(childProp))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            List<Guid> childGuids = new List<Guid>();
            foreach (var childId in childIds)
            {
                if (!Guid.TryParse(childId, out Guid childGuid))
                {
                    return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
                }
                else
                {
                    childGuids.Add(childGuid);
                }
            }
            var pInfo = repository.GetType()
                .GenericTypeArguments
                .FirstOrDefault()
                .GetTypeInfo()
                .GetProperty(childProp.ToInitialCaps());
            if (pInfo == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            try
            {
                await repository.AddChildrenToCollectionAsync(guid, pInfo, childGuids);
                return Ok();
            }
            catch
            {
                return Json(new { error = "Item could not be accessed." });
            }
        }

        /// <summary>
        /// Called to find an entity with the given primary key value and return a ViewModel for that
        /// entity. If no entity is found, an empty ViewModel is returned (not null).
        /// </summary>
        /// <param name="dataType">The type of entity to find.</param>
        /// <param name="id">The primary key of the entity to be found.</param>
        /// <returns>
        /// Redirect to an error page in the event of a bad request; an error if there is a problem;
        /// or a ViewModel representing the item found, or an empty ViewModel if none is found (as JSON).
        /// </returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Find(string dataType, string id)
        {
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (!Guid.TryParse(id, out Guid guid))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
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
            if (!AuthorizationController.IsAuthorized(claims, dataType, CustomClaimTypes.PermissionDataView, id))
            {
                return Json(new { error = "You don't have permission to view this item." });
            }
            object item = null;
            try
            {
                item = await repository.FindAsync(guid);
            }
            catch
            {
                return Json(new { error = "Item could not be accessed." });
            }
            if (item == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/404" });
            }
            return Json(new { data = item });
        }

        /// <summary>
        /// Called to retrieve ViewModels representing all the entities in the <see
        /// cref="ApplicationDbContext"/>'s set.
        /// </summary>
        /// <returns>
        /// Redirect to an error page in the event of a bad request; or ViewModels representing the
        /// items (as JSON).
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetAll(string dataType)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
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
            if (!AuthorizationController.IsAuthorized(claims, dataType, CustomClaimTypes.PermissionDataView))
            {
                return Json(new { error = "You don't have permission to view items of this type." });
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            return Json(repository.GetAll());
        }

        /// <summary>
        /// Called to retrieve all the primary keys of the entities in a given relationship.
        /// </summary>
        /// <param name="dataType">The type of the parent entity.</param>
        /// <param name="id">The primary key of the parent entity.</param>
        /// <param name="childProp">The navigation property of the relationship on the parent entity.</param>
        /// <returns>
        /// Redirect to an error page in the event of a bad request; an error if there is a problem;
        /// or the list of child primary keys (as JSON).
        /// </returns>
        [HttpGet("{id}/{childProp}")]
        public async Task<IActionResult> GetAllChildIds(
            string dataType,
            string id,
            string childProp)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
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
            if (!AuthorizationController.IsAuthorized(claims, dataType, CustomClaimTypes.PermissionDataView))
            {
                return Json(new { error = "You don't have permission to view items of this type." });
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (!Guid.TryParse(id, out Guid guid))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (string.IsNullOrEmpty(childProp))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            object item = null;
            try
            {
                item = await repository.FindItemAsync(guid);
            }
            catch
            {
                return Json(new { error = "Item could not be accessed." });
            }
            if (item == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/404" });
            }
            var typeInfo = item.GetType().GetTypeInfo();
            var pInfo = typeInfo.GetProperty(childProp.ToInitialCaps());
            if (pInfo == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var ptInfo = pInfo.PropertyType.GetTypeInfo();
            try
            {
                if (ptInfo.IsGenericType
                    && ptInfo.GetGenericTypeDefinition().IsAssignableFrom(typeof(ICollection<>)))
                {
                    var children = pInfo.GetValue(item) as IEnumerable;
                    if (ptInfo.GenericTypeArguments.FirstOrDefault().GetTypeInfo().IsSubclassOf(typeof(DataItem)))
                    {
                        var idInfo = ptInfo.GenericTypeArguments.FirstOrDefault().GetProperty("Id");
                        List<Guid> childIds = new List<Guid>();
                        foreach (var child in children)
                        {
                            childIds.Add((Guid)idInfo.GetValue(child));
                        }
                        return Json(childIds);
                    }
                    else if (ptInfo.GenericTypeArguments.FirstOrDefault().GetTypeInfo().ImplementedInterfaces.Any(i => i == typeof(IDataItemMtM)))
                    {
                        var idInfo = ptInfo.GenericTypeArguments.FirstOrDefault().GetProperties().FirstOrDefault(p =>
                            p.Name == pInfo.Name + "Id" || pInfo.Name.GetSingularForms().Any(s => p.Name == s + "Id"));
                        List<Guid> childIds = new List<Guid>();
                        foreach (var child in children)
                        {
                            childIds.Add((Guid)idInfo.GetValue(child));
                        }
                        return Json(childIds);
                    }
                    else
                    {
                        return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
                    }
                }
                else
                {
                    return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
                }
            }
            catch
            {
                return Json(new { error = "Item could not be accessed." });
            }
        }

        /// <summary>
        /// Called to retrieve the primary key of a child entity in the given relationship.
        /// </summary>
        /// <param name="dataType">The type of the parent entity.</param>
        /// <param name="id">The primary key of the parent entity.</param>
        /// <param name="childProp">The navigation property of the relationship.</param>
        /// <returns>
        /// Redirect to an error page in the event of a bad request; an error if there is a problem;
        /// or the primary key of the child entity (as a JSON object with 'response' set to the value).
        /// </returns>
        [HttpGet("{id}/{childProp}")]
        public async Task<IActionResult> GetChildId(string dataType, string id, string childProp)
        {
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (!Guid.TryParse(id, out Guid guid))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
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
            if (!AuthorizationController.IsAuthorized(claims, dataType, CustomClaimTypes.PermissionDataView, id))
            {
                return Json(new { error = "You don't have permission to view this item." });
            }
            if (string.IsNullOrEmpty(childProp))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var pInfo = repository.GetType()
                .GenericTypeArguments
                .FirstOrDefault()
                .GetTypeInfo()
                .GetProperty(childProp.ToInitialCaps());
            if (pInfo == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            try
            {
                var childGuid = await repository.GetChildIdAsync(guid, pInfo);
                return Json(new { response = childGuid });
            }
            catch
            {
                return Json(new { error = "Item could not be accessed." });
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
        /// <returns>
        /// Redirect to an error page in the event of a bad request; return an error if there is a
        /// problem; or the list of ViewModels representing the child objects on the requested page
        /// (as JSON).
        /// </returns>
        [HttpGet("{id}/{childProp}")]
        public async Task<IActionResult> GetChildPage(
            string dataType,
            string id,
            string childProp,
            string search,
            string sortBy,
            bool descending,
            int page,
            int rowsPerPage)
        {
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (!Guid.TryParse(id, out Guid guid))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
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
            if (!AuthorizationController.IsAuthorized(claims, dataType, CustomClaimTypes.PermissionDataView, id))
            {
                return Json(new { error = "You don't have permission to view this item." });
            }
            if (string.IsNullOrEmpty(childProp))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            object item = null;
            try
            {
                item = await repository.FindItemAsync(guid);
            }
            catch
            {
                return Json(new { error = "Item could not be accessed." });
            }
            if (item == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/404" });
            }
            var typeInfo = item.GetType().GetTypeInfo();
            var pInfo = typeInfo.GetProperty(childProp.ToInitialCaps());
            if (pInfo == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var ptInfo = pInfo.PropertyType.GetTypeInfo();
            IEnumerable children = null;
            Type childType = null;
            if (ptInfo.GenericTypeArguments.FirstOrDefault().GetTypeInfo().IsSubclassOf(typeof(DataItem)))
            {
                children = pInfo.GetValue(item) as IEnumerable;
                childType = ptInfo.GenericTypeArguments.FirstOrDefault();
            }
            else if (ptInfo.GenericTypeArguments.FirstOrDefault().GetTypeInfo().ImplementedInterfaces.Any(i => i == typeof(IDataItemMtM)))
            {
                var mtmChildren = pInfo.GetValue(item) as IEnumerable;
                var nav = ptInfo.GenericTypeArguments.FirstOrDefault().GetProperties().FirstOrDefault(p =>
                    p.Name == pInfo.Name || pInfo.Name.GetSingularForms().Contains(p.Name));
                List<DataItem> navChildren = new List<DataItem>();
                foreach (object mtmChild in mtmChildren)
                {
                    foreach (var reference in _context.Entry(mtmChild).References)
                    {
                        reference.Load();
                    }
                    navChildren.Add(nav.GetValue(mtmChild) as DataItem);
                }
                children = navChildren;
                childType = nav.PropertyType;
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (!AuthorizationController.IsAuthorized(claims, childType.Name, CustomClaimTypes.PermissionDataView))
            {
                return Json(new { error = "You don't have permission to view items of this type." });
            }
            try
            {
                var repoMethod = typeof(Repository<>)
                    .MakeGenericType(childType)
                    .GetMethod("GetPageItems");
                return Json(repoMethod.Invoke(null, new object[] {
                    children.Cast<DataItem>().AsQueryable(),
                    search,
                    sortBy,
                    descending,
                    page,
                    rowsPerPage,
                    claims
                }));
            }
            catch
            {
                return Json(new { error = "Item could not be accessed." });
            }
        }

        /// <summary>
        /// Called to retrieve the total number of child entities in the given relationship.
        /// </summary>
        /// <param name="dataType">The type of the parent entity.</param>
        /// <param name="id">The primary key of the parent entity.</param>
        /// <param name="childProp">The navigation property of the relationship on the parent entity.</param>
        /// <returns>
        /// Redirect to an error page in the event of a bad request; an error if there is a problem;
        /// or the total (as a JSON object with 'response' set to the value).
        /// </returns>
        [HttpGet("{id}/{childProp}")]
        public async Task<IActionResult> GetChildTotal(string dataType, string id, string childProp)
        {
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (!Guid.TryParse(id, out Guid guid))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
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
            if (!AuthorizationController.IsAuthorized(claims, dataType, CustomClaimTypes.PermissionDataView, id))
            {
                return Json(new { error = "You don't have permission to view this item." });
            }
            if (string.IsNullOrEmpty(childProp))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var pInfo = repository.GetType()
                .GenericTypeArguments
                .FirstOrDefault()
                .GetTypeInfo()
                .GetProperty(childProp.ToInitialCaps());
            if (pInfo == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            try
            {
                var count = await repository.GetChildTotalAsync(guid, pInfo);
                return Json(new { response = count });
            }
            catch
            {
                return Json(new { error = "Item could not be accessed." });
            }
        }

        /// <summary>
        /// Called to retrieve a list of all entities which are not MenuClass types.
        /// </summary>
        /// <returns>
        /// Redirect to an error page in the event of a bad request; or the list of type names (as JSON).
        /// </returns>
        [AllowAnonymous]
        [HttpGet("/api/[controller]/[action]")]
        public IActionResult GetChildTypes()
        {
            try
            {
                var types = _context.Model.GetEntityTypes()
                    .Select(t => t.ClrType)
                    .Where(t => t.GetTypeInfo().IsSubclassOf(typeof(DataItem)));
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
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
        }

        /// <summary>
        /// Called to retrieve a list of <see cref="FieldDefinition"/> s for the given data type.
        /// </summary>
        /// <returns>
        /// Redirect to an error page in the event of a bad request; an error if there is a problem;
        /// or the list of <see cref="FieldDefinition"/> s (as JSON).
        /// </returns>
        [HttpGet]
        public IActionResult GetFieldDefinitions(string dataType)
        {
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            try
            {
                return Json(repository.GetFieldDefinitions());
            }
            catch
            {
                return Json(new { error = "Data could not be retrieved." });
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
        /// The primary keys of items which should be excluded from the results before
        /// caluclating the page contents.
        /// </param>
        /// <returns>
        /// Redirect to an error page in the event of a bad request; return an error if there is a
        /// problem; or the list of ViewModels representing the entities on the requested page (as JSON).
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> GetPage(
            string dataType,
            string search,
            string sortBy,
            bool descending,
            int page,
            int rowsPerPage,
            [FromBody]string[] except)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
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
            if (!AuthorizationController.IsAuthorized(claims, dataType, CustomClaimTypes.PermissionDataView))
            {
                return Json(new { error = "You don't have permission to view items of this type." });
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            List<Guid> exceptGuids = new List<Guid>();
            foreach (var id in except)
            {
                if (!Guid.TryParse(id, out Guid guid))
                {
                    return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
                }
                else
                {
                    exceptGuids.Add(guid);
                }
            }
            try
            {
                return Json(repository.GetPage(search, sortBy, descending, page, rowsPerPage, exceptGuids, claims));
            }
            catch
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
        }

        /// <summary>
        /// Called to retrieve the total number of entities of the given data type.
        /// </summary>
        /// <param name="dataType">The type of the entity.</param>
        /// <returns>
        /// Redirect to an error page in the event of a bad request; or the total (as a JSON object
        /// with 'response' set to the value).
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetTotal(string dataType)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
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
            if (!AuthorizationController.IsAuthorized(claims, dataType, CustomClaimTypes.PermissionDataView))
            {
                return Json(new { error = "You don't have permission to view items of this type." });
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var total = await repository.GetTotalAsync();
            return Json(new { response = total });
        }

        /// <summary>
        /// Called to retrieve a list of all entities which are MenuClass types.
        /// </summary>
        /// <returns>
        /// Redirect to an error page in the event of a bad request; or the list of type names (as JSON).
        /// </returns>
        [AllowAnonymous]
        [HttpGet("/api/[controller]/[action]")]
        public IActionResult GetTypes()
        {
            try
            {
                var types = _context.Model.GetEntityTypes()
                    .Select(t => t.ClrType)
                    .Where(t => t.GetTypeInfo().IsSubclassOf(typeof(DataItem)));
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
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
        }

        /// <summary>
        /// Called to remove an entity from the <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <param name="dataType">The type of entity to remove.</param>
        /// <param name="id">The primary key of the entity to remove.</param>
        /// <returns>
        /// Redirect to an error page in the event of a bad request; an error if there is a problem;
        /// or an OK result.
        /// </returns>
        [HttpPost("{id}")]
        public async Task<IActionResult> Remove(string dataType, string id)
        {
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (!Guid.TryParse(id, out Guid guid))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
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
            if (!AuthorizationController.IsAuthorized(claims, dataType, CustomClaimTypes.PermissionDataAll, id))
            {
                return Json(new { error = "You don't have permission to remove this item." });
            }
            try
            {
                await repository.RemoveAsync(guid);
                _context.UserClaims.RemoveRange(_context.UserClaims.Where(c => c.ClaimValue == $"{dataType}{{{id}}}"));
                _context.RoleClaims.RemoveRange(_context.RoleClaims.Where(c => c.ClaimValue == $"{dataType}{{{id}}}"));
                await _context.SaveChangesAsync();
            }
            catch
            {
                return Json(new { error = "Item could not be removed." });
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
        /// <returns>
        /// Redirect to an error page in the event of a bad request; an error if there is a problem;
        /// or an OK result.
        /// </returns>
        [HttpPost("{id}/{childProp}")]
        public async Task<IActionResult> RemoveChildrenFromCollection(string dataType, string id, string childProp, [FromBody]string[] childIds)
        {
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (!Guid.TryParse(id, out Guid guid))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
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
            if (!AuthorizationController.IsAuthorized(claims, dataType, CustomClaimTypes.PermissionDataEdit, id))
            {
                return Json(new { error = "You don't have permission to edit this item." });
            }
            if (string.IsNullOrEmpty(childProp))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            List<Guid> childGuids = new List<Guid>();
            foreach (var childId in childIds)
            {
                if (!Guid.TryParse(childId, out Guid childGuid))
                {
                    return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
                }
                else
                {
                    childGuids.Add(childGuid);
                }
            }
            var pInfo = repository.GetType()
                .GenericTypeArguments
                .FirstOrDefault()
                .GetTypeInfo()
                .GetProperty(childProp.ToInitialCaps());
            if (pInfo == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            try
            {
                await repository.RemoveChildrenFromCollectionAsync(guid, pInfo, childGuids);
                return Ok();
            }
            catch
            {
                return Json(new { error = "Item could not be accessed." });
            }
        }

        /// <summary>
        /// Called to terminate a relationship bewteen two entities. If the child entity is made an
        /// orphan by the removal and is not a MenuClass object, it is then removed from the <see
        /// cref="ApplicationDbContext"/> entirely.
        /// </summary>
        /// <param name="dataType">The type of the child entity.</param>
        /// <param name="id">The primary key of the child entity whose relationship is being severed.</param>
        /// <param name="childProp">The navigation property of the relationship being severed.</param>
        /// <returns>
        /// Redirect to an error page in the event of a bad request; an error if there is a problem;
        /// or an OK result.
        /// </returns>
        [HttpPost("{id}/{childProp}")]
        public async Task<IActionResult> RemoveFromParent(string dataType, string id, string childProp)
        {
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (!Guid.TryParse(id, out Guid guid))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
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
            if (!AuthorizationController.IsAuthorized(claims, dataType, CustomClaimTypes.PermissionDataEdit, id))
            {
                return Json(new { error = "You don't have permission to edit this item." });
            }
            if (string.IsNullOrEmpty(childProp))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var pInfo = repository.GetType()
                .GenericTypeArguments
                .FirstOrDefault()
                .GetTypeInfo()
                .GetProperty(childProp.ToInitialCaps());
            if (pInfo == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            try
            {
                var removed = await repository.RemoveFromParentAsync(guid, pInfo);
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
                return Json(new { error = "Item could not be removed." });
            }
        }

        /// <summary>
        /// Called to remove a collection of entities from the <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <param name="dataType">The type of entity to remove.</param>
        /// <param name="ids">The primary keys of the entities to remove.</param>
        /// <returns>
        /// Redirect to an error page in the event of a bad request; an error if there is a problem;
        /// or an OK result.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> RemoveRange(string dataType, [FromBody]List<string> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
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
                if (Guid.TryParse(id, out Guid guid))
                {
                    if (!AuthorizationController.IsAuthorized(claims, dataType, CustomClaimTypes.PermissionDataAll, id))
                    {
                        return Json(new { error = "You don't have permission to remove one or more of these items." });
                    }
                    guids.Add(guid);
                }
                else
                {
                    return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
                }
            }
            try
            {
                await repository.RemoveRangeAsync(guids);
                _context.UserClaims.RemoveRange(_context.UserClaims.Where(c => ids.Any(id => c.ClaimValue == $"{dataType}{{{id}}}")));
                _context.RoleClaims.RemoveRange(_context.RoleClaims.Where(c => ids.Any(id => c.ClaimValue == $"{dataType}{{{id}}}")));
                await _context.SaveChangesAsync();
            }
            catch
            {
                return Json(new { error = "One or more items could not be removed." });
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
        /// <returns>
        /// Redirect to an error page in the event of a bad request; an error if there is a problem;
        /// or an OK result.
        /// </returns>
        [HttpPost("{id}/{childProp}")]
        public async Task<IActionResult> RemoveRangeFromParent(string dataType, string childProp, [FromBody]List<string> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
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
                if (Guid.TryParse(id, out Guid guid))
                {
                    if (!AuthorizationController.IsAuthorized(claims, dataType, CustomClaimTypes.PermissionDataEdit, id))
                    {
                        return Json(new { error = "You don't have permission to edit one or more of these items." });
                    }
                    guids.Add(guid);
                }
                else
                {
                    return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
                }
            }
            if (string.IsNullOrEmpty(childProp))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var pInfo = repository.GetType()
                .GenericTypeArguments
                .FirstOrDefault()
                .GetTypeInfo()
                .GetProperty(childProp.ToInitialCaps());
            if (pInfo == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            try
            {
                var removedIds = await repository.RemoveRangeFromParentAsync(guids, pInfo);
                _context.UserClaims.RemoveRange(_context.UserClaims.Where(c => removedIds.Any(id => c.ClaimValue == $"{dataType}{{{id}}}")));
                _context.RoleClaims.RemoveRange(_context.RoleClaims.Where(c => removedIds.Any(id => c.ClaimValue == $"{dataType}{{{id}}}")));
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return Json(new { error = "One or more items could not be removed." });
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
        /// <returns>
        /// Redirect to an error page in the event of a bad request; an error if there is a problem;
        /// or an OK result.
        /// </returns>
        [HttpPost("{parentId}/{newChildId}/{childProp}")]
        public async Task<IActionResult> ReplaceChild(string dataType, string parentId, string newChildId, string childProp)
        {
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (string.IsNullOrEmpty(parentId) || string.IsNullOrEmpty(newChildId))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (!Guid.TryParse(parentId, out Guid parentGuid)
                || !Guid.TryParse(newChildId, out Guid newChildGuid))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (string.IsNullOrEmpty(childProp))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var pInfo = repository.GetType()
                .GenericTypeArguments
                .FirstOrDefault()
                .GetTypeInfo()
                .GetProperty(childProp.ToInitialCaps());
            if (pInfo == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
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
            if (!AuthorizationController.IsAuthorized(claims, dataType, CustomClaimTypes.PermissionDataEdit, parentId))
            {
                return Json(new { error = "You don't have permission to edit this item." });
            }
            try
            {
                var replacedId = await repository.ReplaceChildAsync(parentGuid, newChildGuid, pInfo);
                if (replacedId.HasValue)
                {
                    _context.UserClaims.RemoveRange(_context.UserClaims.Where(c => c.ClaimValue == $"{dataType}{{{replacedId.Value}}}"));
                    _context.RoleClaims.RemoveRange(_context.RoleClaims.Where(c => c.ClaimValue == $"{dataType}{{{replacedId.Value}}}"));
                    await _context.SaveChangesAsync();
                }
                return Ok();
            }
            catch
            {
                return Json(new { error = "Item could not be added." });
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
        /// <returns>
        /// Redirect to an error page in the event of a bad request; an error if there is a problem;
        /// or an OK result.
        /// </returns>
        [HttpPost("{parentId}/{childProp}")]
        public async Task<IActionResult> ReplaceChildWithNew(string dataType, string parentId, string childProp)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (string.IsNullOrEmpty(parentId))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (!Guid.TryParse(parentId, out Guid parentGuid))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (string.IsNullOrEmpty(childProp))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var pInfo = repository.GetType()
                .GenericTypeArguments
                .FirstOrDefault()
                .GetTypeInfo()
                .GetProperty(childProp.ToInitialCaps());
            if (pInfo == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
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
            if (!AuthorizationController.IsAuthorized(claims, dataType, CustomClaimTypes.PermissionDataEdit, parentId))
            {
                return Json(new { error = "You don't have permission to edit this item." });
            }
            try
            {
                var (newItem, replacedId) = await repository.ReplaceChildWithNewAsync(parentGuid, pInfo);
                var claimValue = $"{dataType}{{{newItem["id"]}}}";
                await _userManager.AddClaimsAsync(user, new Claim[] {
                    new Claim(CustomClaimTypes.PermissionDataOwner, claimValue),
                    new Claim(CustomClaimTypes.PermissionDataAll, claimValue)
                });
                if (replacedId.HasValue)
                {
                    _context.UserClaims.RemoveRange(_context.UserClaims.Where(c => c.ClaimValue == $"{dataType}{{{replacedId.Value}}}"));
                    _context.RoleClaims.RemoveRange(_context.RoleClaims.Where(c => c.ClaimValue == $"{dataType}{{{replacedId.Value}}}"));
                    await _context.SaveChangesAsync();
                }
                return Json(new { data = newItem });
            }
            catch
            {
                return Json(new { error = "Item could not be added." });
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
            repository = GetRepository(type, context);
            return true;
        }

        private static IRepository GetRepository(Type type, ApplicationDbContext context)
            => (IRepository)Activator.CreateInstance(typeof(Repository<>).MakeGenericType(type), context);

        private bool TryResolveObject(string dataType, JObject item, out DataItem obj, out Type type)
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
                obj = item.ToObject(type) as DataItem;
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
        /// <returns>
        /// Redirect to an error page in the event of a bad request; an error if there is a problem;
        /// or a ViewModel representing the updated item (as JSON).
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> Update(string dataType, [FromBody]JObject item)
        {
            if (!TryResolveObject(dataType, item, out DataItem obj, out Type type))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
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
            if (!AuthorizationController.IsAuthorized(claims, dataType, CustomClaimTypes.PermissionDataEdit, obj.Id.ToString()))
            {
                return Json(new { error = "You don't have permission to edit this item." });
            }
            IRepository repository = GetRepository(type, _context);
            try
            {
                var updatedItem = await repository.UpdateAsync(obj);
                return Json(new { data = updatedItem });
            }
            catch
            {
                return Json(new { error = "Item could not be updated." });
            }
        }
    }
}
