using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace VueCoreFramework.Models
{
    /// <summary>
    /// Represents a message sent within the SPA framework to a user, or to a group.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// The primary key of the <see cref="Message"/>.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The content of the message. Accepts markdown-formatted text.
        /// </summary>
        [MaxLength(125)]
        public string Content { get; set; }

        /// <summary>
        /// The group to which the message was sent (if not an individual message).
        /// </summary>
        public IdentityRole GroupRecipient { get; set; }

        /// <summary>
        /// The name of the group to which the message was sent (if not an individual message).
        /// </summary>
        public string GroupRecipientName { get; set; }

        /// <summary>
        /// Indicates that the message is from the system, rather than from a user.
        /// </summary>
        public bool IsSystemMessage { get; set; }

        /// <summary>
        /// Indicates that the single recipient has read the message.
        /// </summary>
        public bool Received { get; set; }

        /// <summary>
        /// Indicates that the single recipient has marked the message as deleted.
        /// </summary>
        public bool RecipientDeleted { get; set; }

        /// <summary>
        /// The user who sent the message.
        /// </summary>
        public ApplicationUser Sender { get; set; }

        /// <summary>
        /// Indicates that the sender has marked the message as deleted.
        /// </summary>
        public bool SenderDeleted { get; set; }

        /// <summary>
        /// The name of the user who sent the message.
        /// </summary>
        public string SenderUsername { get; set; }

        /// <summary>
        /// The user to whom the message was sent (if not a group message).
        /// </summary>
        public ApplicationUser SingleRecipient { get; set; }

        /// <summary>
        /// The name of the user to whom the message was sent (if not a group message).
        /// </summary>
        public string SingleRecipientName { get; set; }

        /// <summary>
        /// The date and time when the message was sent.
        /// </summary>
        /// <remarks>Set automatically by the database.</remarks>
        public DateTime Timestamp { get; set; }
    }
}
