namespace LLama.OldVersion;

public record ChatCompletion(string Id, string Object, int Created, string Model, ChatCompletionChoice[] Choices, CompletionUsage Usage);