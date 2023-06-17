namespace LLama.OldVersion;

public record ChatCompletionChoice(int Index, ChatCompletionMessage Message, string? FinishReason);