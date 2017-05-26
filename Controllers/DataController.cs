using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MVCCoreVue.Data;
using MVCCoreVue.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCCoreVue.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
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
        public async Task<JsonResult> Add([FromBody]JObject item)
        {
            if (item == null)
            {
                AddError(item, "There was a problem with your request.");
            }
        }

        private static void AddError(JObject item, string error)
        {
            var hasError = item.TryGetValue("errors", out JToken errors);
            if (!hasError)
            {
                item.Add("errors", JToken.FromObject(new string[] { error }));
            }
            else
            {
                JArray eArray = errors as JArray;
                eArray.Add(JToken.FromObject(error));
            }
        }
    }
}
