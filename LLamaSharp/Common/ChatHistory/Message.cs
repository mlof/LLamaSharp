namespace LLama.Common.ChatHistory
{
    /// <summary>
    ///     Chat message representation
    /// </summary>
    public class Message
    {
        /// <summary>
        ///     Create a new instance
        /// </summary>
        /// <param name="authorRole">Role of message author</param>
        /// <param name="content">Message content</param>
        public Message(AuthorRole authorRole, string content)
        {
            AuthorRole = authorRole;
            Content = content;
        }

        /// <summary>
        ///     Role of the message author, e.g. user/assistant/system
        /// </summary>
        public AuthorRole AuthorRole { get; set; }

        /// <summary>
        ///     Message content
        /// </summary>
        public string Content { get; set; }
    }
}
