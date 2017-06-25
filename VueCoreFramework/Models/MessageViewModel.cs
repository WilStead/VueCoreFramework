using System;

namespace VueCoreFramework.Models
{
    /// <summary>
    /// Used to transfer information about a message.
    /// </summary>
    public class MessageViewModel
    {
        /// <summary>
        /// The content of the message. May have markdown-formatted text.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Indicates that the message is from the system, rather than from a user.
        /// </summary>
        public bool IsSystemMessage { get; set; }

        /// <summary>
        /// The name of the user who sent the message.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The date and time when the message was sent.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
