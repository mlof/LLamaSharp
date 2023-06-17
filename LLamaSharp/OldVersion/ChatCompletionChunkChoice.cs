namespace LLama.OldVersion;

public record ChatCompletionChunkChoice(int Index, ChatCompletionChunkDelta Delta, string? FinishReason);