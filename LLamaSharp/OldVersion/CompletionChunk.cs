namespace LLama.OldVersion;

public record CompletionChunk(string Id, string Object, int Created, string Model, CompletionChoice[] Choices);