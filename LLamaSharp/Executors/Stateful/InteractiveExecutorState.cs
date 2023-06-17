using System;
using System.Text.Json.Serialization;

namespace LLama.Executors.Stateful
{
    public class InteractiveExecutorState : ExecutorBaseState
    {
        [JsonPropertyName("is_prompt_run")]
        public bool IsPromptRun { get; set; }
        [JsonPropertyName("llama_token_newline")]
        public Int32[] LLamaNewlineTokens { get; set; }
    }
}