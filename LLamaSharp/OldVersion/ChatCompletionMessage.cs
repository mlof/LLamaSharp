namespace LLama.OldVersion;

public record ChatCompletionMessage(ChatRole Role, string Content, string? Name = null);