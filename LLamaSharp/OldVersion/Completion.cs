namespace LLama.OldVersion;

public record Completion(string Id, string Object, int Created, string Model, CompletionChoice[] Choices, CompletionUsage Usage);