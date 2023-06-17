using System.Text;
using LLama.Common;
using LLama.Transformations.Abstractions;

namespace LLama.Transformations
{

    /// <summary>
    /// The default history transform.
    /// Uses plain text with the following format:
    /// [Author]: [Message]
    /// </summary>
    public class DefaultHistoryTransform : IHistoryTransform
        {
            private readonly string defaultUserName = "User";
            private readonly string defaultAssistantName = "Assistant";
            private readonly string defaultSystemName = "System";
            private readonly string defaultUnknownName = "??";

            string _userName;
            string _assistantName;
            string _systemName;
            string _unknownName;
            bool _isInstructMode;
            public DefaultHistoryTransform(string? userName = null, string? assistantName = null, 
                string? systemName = null, string? unknownName = null, bool isInstructMode = false)
            {
                _userName = userName ?? defaultUserName;
                _assistantName = assistantName ?? defaultAssistantName;
                _systemName = systemName ?? defaultSystemName;
                _unknownName = unknownName ?? defaultUnknownName;
                _isInstructMode = isInstructMode;
            }

            public virtual string HistoryToText(ChatHistory history)
            {
                StringBuilder sb = new();
                foreach (var message in history.Messages)
                {
                    if (message.AuthorRole == AuthorRole.User)
                    {
                        sb.AppendLine($"{_userName}: {message.Content}");
                    }
                    else if (message.AuthorRole == AuthorRole.System)
                    {
                        sb.AppendLine($"{_systemName}: {message.Content}");
                    }
                    else if (message.AuthorRole == AuthorRole.Unknown)
                    {
                        sb.AppendLine($"{_unknownName}: {message.Content}");
                    }
                    else if (message.AuthorRole == AuthorRole.Assistant)
                    {
                        sb.AppendLine($"{_assistantName}: {message.Content}");
                    }
                }
                return sb.ToString();
            }

            public virtual ChatHistory TextToHistory(AuthorRole role, string text)
            {
                ChatHistory history = new ChatHistory();
                history.AddMessage(role, TrimNamesFromText(text, role));
                return history;
            }

            public virtual string TrimNamesFromText(string text, AuthorRole role)
            {
                if (role == AuthorRole.User && text.StartsWith($"{_userName}:"))
                {
                    text = text.Substring($"{_userName}:".Length).TrimStart();
                }
                else if (role == AuthorRole.Assistant && text.EndsWith($"{_assistantName}:"))
                {
                    text = text.Substring(0, text.Length - $"{_assistantName}:".Length).TrimEnd();
                }
                if (_isInstructMode && role == AuthorRole.Assistant && text.EndsWith("\n> "))
                {
                    text = text.Substring(0, text.Length - "\n> ".Length).TrimEnd();
                }
                return text;
            }
        }
}
