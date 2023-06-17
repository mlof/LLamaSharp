﻿using LLama.Common;
using LLama.Common.ChatHistory;
namespace LLama.Transformations.Abstractions
{
    /// <summary>
    /// Transform history to plain text and vice versa.
    /// </summary>
    public interface IHistoryTransform
    {
        /// <summary>
        /// Convert a ChatHistory instance to plain text.
        /// </summary>
        /// <param name="history">The ChatHistory instance</param>
        /// <returns></returns>
        string HistoryToText(ChatHistory history);

        /// <summary>
        /// Converts plain text to a ChatHistory instance.
        /// </summary>
        /// <param name="role">The role for the author.</param>
        /// <param name="text">The chat history as plain text.</param>
        /// <returns>The updated history.</returns>
        Common.ChatHistory.ChatHistory TextToHistory(AuthorRole role, string text);
    }
}
