using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MVCCoreVue.Data;
using MVCCoreVue.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

        [HttpPost]
        public async Task<IActionResult> Add(string dataType, [FromBody]JObject item)
        {
            if (!TryResolveObject(dataType, item, out object obj, out Type type))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            IRepository repository = GetRepository(type);
            try
            {
                var newItem = await repository.AddAsync(obj);
                return Json(newItem);
            }
            catch
            {
                return Json(new { error = "Item could not be saved." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Find(string dataType, string id)
        {
            if (!TryGetRepository(dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (Guid.TryParse(id, out Guid guid))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var item = await repository.FindAsync(guid);
            if (item == null)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/404" });
            }
            return Json(item);
        }

        [HttpGet]
        public IActionResult GetAll(string dataType)
        {
            if (!TryGetRepository(dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            return Json(repository.GetAll());
        }

        [HttpGet]
        public IActionResult GetFieldDefinitions(string dataType)
        {
            if (!TryGetRepository(dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            return Json(repository.GetFieldDefinitions());
        }

        [HttpGet]
        public IActionResult GetPage(
            string dataType,
            string search,
            string sortBy,
            bool descending,
            int page,
            int rowsPerPage)
        {
            if (!TryGetRepository(dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            return Json(repository.GetPage(search, sortBy, descending, page, rowsPerPage));
        }

        [HttpGet]
        public async Task<IActionResult> GetTotal(string dataType)
        {
            if (!TryGetRepository(dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            var total = await repository.GetTotalAsync();
            return Json(new { total = total });
        }

        [HttpPost]
        public async Task<IActionResult> Remove(string dataType, string id)
        {
            if (!TryGetRepository(dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (Guid.TryParse(id, out Guid guid))
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

        [HttpPost]
        public async Task<IActionResult> RemoveRange(string dataType, [FromBody]List<string> ids)
        {
            if (!TryGetRepository(dataType, out IRepository repository))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            if (ids.Count == 0)
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            List<Guid> guids = new List<Guid>();
            foreach (var id in ids)
            {
                if (Guid.TryParse(id, out Guid guid))
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

        private bool TryGetRepository(string dataType, out IRepository repository)
        {
            repository = null;
            if (string.IsNullOrEmpty(dataType))
            {
                return false;
            }
            var entity = _context.Model.FindEntityType(dataType);
            if (entity == null)
            {
                return false;
            }
            var type = entity.ClrType;
            if (type == null)
            {
                return false;
            }
            repository = (IRepository)Activator.CreateInstance(typeof(Repository<>).MakeGenericType(type));
            return true;
        }

        private IRepository GetRepository(Type type)
            => (IRepository)Activator.CreateInstance(typeof(Repository<>).MakeGenericType(type));

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
            var entity = _context.Model.FindEntityType(dataType);
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

        [HttpPost]
        public async Task<IActionResult> Update(string dataType, [FromBody]JObject item)
        {
            if (!TryResolveObject(dataType, item, out object obj, out Type type))
            {
                return RedirectToAction(nameof(HomeController.Index), new { forwardUrl = "/error/400" });
            }
            IRepository repository = GetRepository(type);
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
