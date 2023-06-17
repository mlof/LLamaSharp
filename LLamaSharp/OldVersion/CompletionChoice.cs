namespace LLama.OldVersion;

public record CompletionChoice(string Text, int Index, CompletionLogprobs? Logprobs, string? FinishReason);