using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LLama.Executors.Stateful
{
    public class ExecutorBaseState
    {
        [JsonPropertyName("n_past")] public int PastTokensCount { get; set; }

        [JsonPropertyName("n_consumed")] public int ConsumedTokensCount { get; set; }

        [JsonPropertyName("n_session_consumed")]
        public int ConsumedSessionCount { get; set; }

        [JsonPropertyName("n_matching_session_tokens")]
        public int MatchingSessionTokensCount { get; set; }

        [JsonPropertyName("path_session")] public string SessionFilePath { get; set; }

        [JsonPropertyName("embd")] public List<Int32> Embeds { get; set; }

        [JsonPropertyName("embd_inps")] public List<Int32> EmbedInps { get; set; }

        [JsonPropertyName("session_tokens")] public List<Int32> SessionTokens { get; set; }

        [JsonPropertyName("last_n_tokens")] public Int32[] LastTokens { get; set; }

        [JsonPropertyName("last_tokens_maximum_count")]
        public int LastTokensCapacity { get; set; }
    }
}
