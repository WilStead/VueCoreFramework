using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VueCoreFramework.Core.Messages;

namespace VueCoreFramework.API.Controllers
{
    /// <summary>
    /// Handles all API errors.
    /// </summary>
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="ErrorController"/>.
        /// </summary>
        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles all uncaught API errors.
        /// </summary>
        /// <param name="code">The status code.</param>
        /// <returns>The status code, and a default message.</returns>
        [HttpGet("Error/{code}")]
        public IActionResult Error(int code)
        {
            var exc = HttpContext.Features.Get<IExceptionHandlerFeature>();

            if (exc != null)
            {
                _logger.LogError(LogEvent.INTERNAL_ERROR, exc.Error, "An internal error occurred.");
            }

            var message = string.Empty;
            switch (code)
            {
                case 400:
                    message = "Bad request";
                    break;
                case 401:
                    message = "Unauthorized";
                    break;
                case 403:
                    message = "Forbidden";
                    break;
                case 404:
                    message = "Not found";
                    break;
                case 405:
                    message = "Method not allowed";
                    break;
                case 406:
                    message = "Not acceptable";
                    break;
                case 408:
                    message = "Request timeout";
                    break;
                case 414:
                    message = "URI too long";
                    break;
                case 415:
                    message = "Unsupported media type";
                    break;
                case 418:
                    message = "I'm a teapot";
                    break;
                case 500:
                    message = "Internal server error";
                    break;
                case 501:
                    message = "Not implemented";
                    break;
                case 505:
                    message = "HTTP version not supported";
                    break;
                default:
                    break;
            }

            if (string.IsNullOrEmpty(message))
            {
                return StatusCode(code);
            }
            else
            {
                return StatusCode(code, message);
            }
        }
    }
}
