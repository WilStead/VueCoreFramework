using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MVCCoreVue.Data;
using MVCCoreVue.Data.Attributes;
using MVCCoreVue.Extensions;
using MVCCoreVue.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MVCCoreVue.Controllers
{
    [Authorize]
    [Route("api/[controller]/{dataType}/[action]")]
    public class DataController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationDbContext _context;

        public DataController(ILogger<AccountController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost("{childProp}/{parentId}")]
        public async Task<IActionResult> Add(string dataType, string childProp, string parentId)
        {
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
                return Json(newItem);
            }
            catch
            {
                return Json(new { error = "Item could not be saved." });
            }
        }

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
            return Json(item);
        }

        [HttpGet]
        public IActionResult GetAll(string dataType)
        {
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            return Json(repository.GetAll());
        }

        [HttpGet("{id}/{childProp}")]
        public async Task<IActionResult> GetAllChildIds(
            string dataType,
            string id,
            string childProp)
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
                    && ptInfo.GetGenericTypeDefinition().IsAssignableFrom(typeof(ICollection<>))
                    && ptInfo.GenericTypeArguments.FirstOrDefault().GetTypeInfo().IsSubclassOf(typeof(DataItem)))
                {
                    var children = pInfo.GetValue(item) as IEnumerable;
                    var idInfo = ptInfo.GenericTypeArguments.FirstOrDefault().GetProperty("Id");
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
            catch
            {
                return Json(new { error = "Item could not be accessed." });
            }
        }

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
                var repoMethod = typeof(Repository<>)
                    .MakeGenericType(ptInfo.GenericTypeArguments.FirstOrDefault())
                    .GetMethod("GetPageItems");
                var children = pInfo.GetValue(item) as IEnumerable;
                return Json(repoMethod.Invoke(null, new object[] { children.Cast<DataItem>().AsQueryable(), search, sortBy, descending, page, rowsPerPage }));
            }
            catch
            {
                return Json(new { error = "Item could not be accessed." });
            }
        }

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
            try
            {
                var children = pInfo.GetValue(item) as ICollection<DataItem>;
                return Json(new { response = children.Count });
            }
            catch
            {
                return Json(new { error = "Item could not be accessed." });
            }
        }

        [AllowAnonymous]
        [HttpGet("/api/[controller]/[action]")]
        public IActionResult GetChildTypes()
        {
            try
            {
                var types = _context.Model.GetEntityTypes()
                    .Select(t => t.ClrType)
                    .Where(t => t.GetTypeInfo().IsSubclassOf(typeof(DataItem)));
                List<string> classes = new List<string>();
                foreach (var type in types)
                {
                    var attr = type.GetTypeInfo().GetCustomAttribute<MenuClassAttribute>();
                    if (attr == null)
                    {
                        classes.Add(type.Name);
                    }
                }
                return Json(classes);
            }
            catch
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
        }

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

        [HttpPost]
        public IActionResult GetPage(
            string dataType,
            string search,
            string sortBy,
            bool descending,
            int page,
            int rowsPerPage,
            [FromBody]string[] except)
        {
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
                return Json(repository.GetPage(search, sortBy, descending, page, rowsPerPage, exceptGuids));
            }
            catch
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTotal(string dataType)
        {
            if (!TryGetRepository(_context, dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var total = await repository.GetTotalAsync();
            return Json(new { response = total });
        }

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
                            new {
                                category = string.IsNullOrEmpty(attr.Category) ? "/" : attr.Category,
                                iconClass = attr.IconClass
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
            try
            {
                await repository.RemoveAsync(guid);
            }
            catch
            {
                return Json(new { error = "Item could not be removed." });
            }
            return Ok();
        }

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
                await repository.RemoveFromParentAsync(guid, pInfo);
                return Ok();
            }
            catch
            {
                return Json(new { error = "Item could not be removed." });
            }
        }

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
            List<Guid> guids = new List<Guid>();
            foreach (var id in ids)
            {
                if (!Guid.TryParse(id, out Guid guid))
                {
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
            }
            catch
            {
                return Json(new { error = "One or more items could not be removed." });
            }
            return Ok();
        }

        [HttpPost("{id}/{childFKProp}")]
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
            List<Guid> guids = new List<Guid>();
            foreach (var id in ids)
            {
                if (!Guid.TryParse(id, out Guid guid))
                {
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
                await repository.RemoveRangeFromParentAsync(guids, pInfo);
                return Ok();
            }
            catch
            {
                return Json(new { error = "One or more items could not be removed." });
            }
        }

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
            try
            {
                await repository.ReplaceChildAsync(parentGuid, newChildGuid, pInfo);
                return Ok();
            }
            catch
            {
                return Json(new { error = "Item could not be added." });
            }
        }

        [HttpPost("{parentId}/{childProp}")]
        public async Task<IActionResult> ReplaceChildWithNew(string dataType, string parentId, string childProp)
        {
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
            try
            {
                var newItem = await repository.ReplaceChildWithNewAsync(parentGuid, pInfo);
                return Json(newItem);
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

        [HttpPost]
        public async Task<IActionResult> Update(string dataType, [FromBody]JObject item)
        {
            if (!TryResolveObject(dataType, item, out DataItem obj, out Type type))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            IRepository repository = GetRepository(type, _context);
            try
            {
                var updatedItem = await repository.UpdateAsync(obj);
                return Json(updatedItem);
            }
            catch
            {
                return Json(new { error = "Item could not be updated." });
            }
        }
    }
}
