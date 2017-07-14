using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VueCoreFramework.Data;
using VueCoreFramework.Models;
using VueCoreFramework.Models.ViewModels;
using VueCoreFramework.Services;

namespace VueCoreFramework.Controllers
{
    /// <summary>
    /// An MVC controller for handling messaging tasks.
    /// </summary>
    [Authorize]
    [Route("api/[controller]/[action]")]
    public class MessageController : Controller
    {
        private readonly AdminOptions _adminOptions;
        private readonly ApplicationDbContext _context;
        private readonly IStringLocalizer<ErrorMessages> _errorLocalizer;
        private readonly IStringLocalizer<EmailMessages> _responseLocalizer;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Initializes a new instance of <see cref="MessageController"/>.
        /// </summary>
        public MessageController(
            IOptions<AdminOptions> adminOptions,
            ApplicationDbContext context,
            IStringLocalizer<ErrorMessages> localizer,
            IStringLocalizer<EmailMessages> responseLocalizer,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _adminOptions = adminOptions.Value;
            _context = context;
            _errorLocalizer = localizer;
            _responseLocalizer = responseLocalizer;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        /// <summary>
        /// Called to get a list of users involved in individual conversations in which the current
        /// user is a sender or recipient, with an unread message count.
        /// </summary>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">A list of <see cref="ConversationViewModel"/>s.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, object>), 200)]
        public async Task<IActionResult> GetConversations()
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

            List<ConversationViewModel> vms = new List<ConversationViewModel>();
            foreach(var message in _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.SingleRecipient)
                .Where(m => !m.IsSystemMessage
                && m.GroupRecipient == null
                && ((m.Sender == user && !m.SenderDeleted)
                || (m.SingleRecipient == user && !m.RecipientDeleted))))
            {
                var interlocutor = message.Sender == user ? message.SingleRecipientName : message.SenderUsername;
                var conversation = vms.FirstOrDefault(v => v.Interlocutor == interlocutor);
                if (conversation == null)
                {
                    conversation = new ConversationViewModel { Interlocutor = interlocutor };
                    vms.Add(conversation);
                }
                if (message.SingleRecipient == user && !message.Received)
                {
                    conversation.UnreadCount++;
                }
            }
            return Json(vms);
        }

        /// <summary>
        /// Called to get the messages exchanged within the given group.
        /// </summary>
        /// <param name="group">The name of the group whose conversation will be retrieved.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">The ordered list of <see cref="MessageViewModel"/>s.</response>
        [HttpGet("{group}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, object>), 200)]
        public async Task<IActionResult> GetGroupMessages(string group)
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

            var groupRole = await _roleManager.FindByNameAsync(group);
            if (groupRole == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidTargetGroupError]);
            }
            var managerId = _context.UserClaims.FirstOrDefault(c =>
                c.ClaimType == CustomClaimTypes.PermissionGroupManager && c.ClaimValue == group)?
                .UserId;
            ApplicationUser manager = null;
            if (managerId != null)
            {
                manager = await _userManager.FindByIdAsync(managerId);
            }

            var vms = new List<MessageViewModel>();
            foreach (var message in _context.Messages
                .Include(m => m.GroupRecipient)
                .Include(m => m.Sender)
                .Where(m => m.GroupRecipient == groupRole)
                .OrderBy(m => m.Timestamp))
            {
                if (message.IsSystemMessage)
                {
                    vms.Add(new MessageViewModel
                    {
                        Content = message.Content,
                        IsSystemMessage = true,
                        Username = message.SenderUsername,
                        Timestamp = message.Timestamp
                    });
                }
                else
                {
                    var roles = await _userManager.GetRolesAsync(message.Sender);
                    vms.Add(new MessageViewModel
                    {
                        Content = message.Content,
                        IsSystemMessage = false,
                        IsUserAdmin = roles.Contains(CustomRoles.Admin),
                        IsUserManager = manager != null && message.Sender == manager,
                        IsUserSiteAdmin = roles.Contains(CustomRoles.SiteAdmin),
                        Username = message.SenderUsername,
                        Timestamp = message.Timestamp
                    });
                }
            }
            return Json(vms);
        }

        /// <summary>
        /// Called to get a list of users involved in individual conversations in which the given
        /// user is a sender or recipient. For use by admins to review chat logs.
        /// </summary>
        /// <param name="proxy">The name of the user whose conversation will be retrieved.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">A list of <see cref="ConversationViewModel"/>s.</response>
        [HttpGet("{proxy}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, object>), 200)]
        public async Task<IActionResult> GetProxyConversations(string proxy)
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
            if (!roles.Contains(CustomRoles.Admin))
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.AdminOnlyError]);
            }

            List<ConversationViewModel> vms = new List<ConversationViewModel>();
            foreach (var message in _context.Messages
                .Where(m => !m.IsSystemMessage
                && (m.SenderUsername == proxy || m.SingleRecipientName == proxy)))
            {
                var interlocutor = message.SenderUsername == proxy ? message.SingleRecipientName : message.SenderUsername;
                var conversation = vms.FirstOrDefault(v => v.Interlocutor == interlocutor);
                if (conversation == null)
                {
                    conversation = new ConversationViewModel { Interlocutor = interlocutor };
                    vms.Add(conversation);
                }
            }
            return Json(vms);
        }

        /// <summary>
        /// Called to get the messages between a proxy user and the given user. For use by admins to
        /// review chat logs.
        /// </summary>
        /// <param name="proxy">
        /// The name of the user whose conversation with the other user will be retrieved.
        /// </param>
        /// <param name="username">
        /// The name of the user whose conversation with the proxy user will be retrieved.
        /// </param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">The ordered list of <see cref="MessageViewModel"/>s.</response>
        [HttpGet("{proxy}/{username}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, object>), 200)]
        public async Task<IActionResult> GetProxyUserMessages(string proxy, string username)
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
            if (!roles.Contains(CustomRoles.Admin))
            {
                return StatusCode(403, _errorLocalizer[ErrorMessages.AdminOnlyError]);
            }

            var vms = new List<MessageViewModel>();
            foreach (var message in _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.SingleRecipient)
                .Where(m => (m.SingleRecipientName == proxy && m.SenderUsername == username)
                || (m.SingleRecipientName == username && m.SenderUsername == proxy))
                .OrderBy(m => m.Timestamp))
            {
                var recipientRoles = await _userManager.GetRolesAsync(message.SenderUsername == proxy ? message.SingleRecipient : message.Sender);
                vms.Add(new MessageViewModel
                {
                    Content = message.Content,
                    IsSystemMessage = message.IsSystemMessage,
                    IsUserAdmin = recipientRoles.Contains(CustomRoles.Admin),
                    IsUserSiteAdmin = recipientRoles.Contains(CustomRoles.SiteAdmin),
                    Received = message.Received,
                    Username = message.SenderUsername,
                    Timestamp = message.Timestamp
                });
            }
            return Json(vms);
        }

        /// <summary>
        /// Called to get the system messages sent to the current user which have not been marked
        /// deleted by the current user.
        /// </summary>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">The ordered list of <see cref="MessageViewModel"/>s.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, object>), 200)]
        public async Task<IActionResult> GetSystemMessages()
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

            return Json(_context.Messages.Where(m =>
                m.SingleRecipient == user && m.IsSystemMessage)
                .OrderBy(m => m.Timestamp)
                .Select(m => new MessageViewModel
                {
                    Content = m.Content,
                    IsSystemMessage = true,
                    Received = m.Received,
                    Timestamp = m.Timestamp
                }));
        }

        /// <summary>
        /// Called to get the messages between the current user and the given user which have not
        /// been marked deleted by the current user.
        /// </summary>
        /// <param name="username">
        /// The name of the user whose conversation with the current user will be retrieved.
        /// </param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">The ordered list of <see cref="MessageViewModel"/>s.</response>
        [HttpGet("{username}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(typeof(IDictionary<string, object>), 200)]
        public async Task<IActionResult> GetUserMessages(string username)
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

            var vms = new List<MessageViewModel>();
            foreach (var message in _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.SingleRecipient)
                .Where(m => (m.SingleRecipient == user && m.SenderUsername == username && !m.RecipientDeleted)
                || (m.SingleRecipientName == username && m.Sender == user && !m.SenderDeleted))
                .OrderBy(m => m.Timestamp))
            {
                var roles = await _userManager.GetRolesAsync(message.Sender == user ? message.SingleRecipient : message.Sender);
                vms.Add(new MessageViewModel
                {
                    Content = message.Content,
                    IsSystemMessage = message.IsSystemMessage,
                    IsUserAdmin = roles.Contains(CustomRoles.Admin),
                    IsUserSiteAdmin = roles.Contains(CustomRoles.SiteAdmin),
                    Received = message.Received,
                    Username = message.SenderUsername,
                    Timestamp = message.Timestamp
                });
            }
            return Json(vms);
        }

        /// <summary>
        /// Called to mark a conversation with a given user deleted.
        /// </summary>
        /// <param name="username">
        /// The name of the user whose conversation with the current user will be marked deleted.
        /// </param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost("{username}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> MarkConversationDeleted(string username)
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

            foreach (var message in _context.Messages.Where(m =>
                (m.SingleRecipient == user && m.SenderUsername == username)
                || (m.SingleRecipientName == username && m.Sender == user)))
            {
                if (message.Sender == user)
                {
                    message.SenderDeleted = true;
                }
                else
                {
                    message.RecipientDeleted = true;
                }
                // Messages are actually deleted once both participants have marked them as such.
                if (message.SenderDeleted && message.RecipientDeleted)
                {
                    _context.Messages.Remove(message);
                }
            }
            await _context.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Called to mark a conversation with a given user read, from the perspective of the current user.
        /// </summary>
        /// <param name="username">
        /// The name of the user whose conversation with the current user will be marked read.
        /// </param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost("{username}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> MarkConversationRead(string username)
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

            foreach (var message in _context.Messages.Where(m => m.SingleRecipient == user
                && m.SenderUsername == username))
            {
                message.Received = true;
            }
            await _context.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Called to mark all system messages sent to the current user read.
        /// </summary>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> MarkSystemMessagesRead()
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

            foreach (var message in _context.Messages.Where(m => m.SingleRecipient == user && m.IsSystemMessage))
            {
                message.Received = true;
            }
            await _context.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Called to send a message to the given group.
        /// </summary>
        /// <param name="group">The name of the group to which the message will be sent.</param>
        /// <param name="message">The message to send.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost("{group}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> SendMessageToGroup(string group, string message)
        {
            if (string.IsNullOrEmpty(message) || message.Length > 125)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MessageInvalidLengthError]);
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

            var groupRole = await _roleManager.FindByNameAsync(group);
            if (groupRole == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidTargetGroupError]);
            }

            var messages = _context.Messages.Where(m => m.GroupRecipient == groupRole);
            if (messages.Count() >= 250)
            {
                _context.Messages.Remove(messages.OrderBy(m => m.Timestamp).FirstOrDefault());
            }
            _context.Messages.Add(new Message
            {
                Content = message,
                Sender = user,
                SenderUsername = user.UserName,
                GroupRecipient = groupRole,
                GroupRecipientName = groupRole.Name
            });
            await _context.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// Called to send a message to the given user.
        /// </summary>
        /// <param name="username">The name of the user to whom the message will be sent.</param>
        /// <param name="message">The message to send.</param>
        /// <response code="400">Bad request.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="200">Success.</response>
        [HttpPost("{username}")]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 403)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> SendMessageToUser(string username, string message)
        {
            if (string.IsNullOrEmpty(message) || message.Length > 125)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.MessageInvalidLengthError]);
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

            var targetUser = await _userManager.FindByNameAsync(username);
            if (targetUser == null)
            {
                return BadRequest(_errorLocalizer[ErrorMessages.InvalidTargetUserError]);
            }

            var messages = _context.Messages.Where(m =>
                (m.Sender == user && m.SingleRecipient == targetUser)
                || (m.Sender == targetUser && m.SingleRecipient == user));
            if (messages.Count() >= 100)
            {
                _context.Messages.Remove(messages.OrderBy(m => m.Timestamp).FirstOrDefault());
            }
            _context.Messages.Add(new Message
            {
                Content = message,
                Sender = user,
                SenderUsername = user.UserName,
                SingleRecipient = targetUser,
                SingleRecipientName = targetUser.UserName
            });
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
