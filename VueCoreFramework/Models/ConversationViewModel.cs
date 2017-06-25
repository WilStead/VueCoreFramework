namespace VueCoreFramework.Models
{
    /// <summary>
    /// Used to send information about a conversation.
    /// </summary>
    public class ConversationViewModel
    {
        /// <summary>
        /// The username of the other party in the conversation.
        /// </summary>
        public string Interlocutor { get; set; }

        /// <summary>
        /// The number of messages the current user has not read in the conversation.
        /// </summary>
        public int UnreadCount { get; set; }
    }
}
