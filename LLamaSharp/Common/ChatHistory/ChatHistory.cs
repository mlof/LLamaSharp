using System.Collections.Generic;
using LLama.Common;

namespace LLama.Common.ChatHistory
{
    // copy from semantic-kernel
    public class ChatHistory
    {



        /// <summary>
        /// List of messages in the chat
        /// </summary>
        public List<Message> Messages { get; }

        /// <summary>
        /// Create a new instance of the chat content class
        /// </summary>
        public ChatHistory()
        {
            Messages = new List<Message>();
        }

        /// <summary>
        /// Add a message to the chat history
        /// </summary>
        /// <param name="authorRole">Role of the message author</param>
        /// <param name="content">Message content</param>
        public void AddMessage(AuthorRole authorRole, string content)
        {
            Messages.Add(new Message(authorRole, content));
        }
    }
}
