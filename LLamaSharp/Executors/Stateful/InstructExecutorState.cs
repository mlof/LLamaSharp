using System;
using System.Text.Json.Serialization;

namespace LLama.Executors.Stateful
{
    public class InstructExecutorState : ExecutorBaseState
    {
        [JsonPropertyName("is_prompt_run")] public bool IsPromptRun { get; set; }

        [JsonPropertyName("inp_pfx")] public Int32[] InputPrefixTokens { get; set; }

        [JsonPropertyName("inp_sfx")] public Int32[] InputSuffixTokens { get; set; }
    }
}
