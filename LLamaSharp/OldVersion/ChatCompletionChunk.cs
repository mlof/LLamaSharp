namespace LLama.OldVersion;

public record ChatCompletionChunk(string Id, string Model, string Object, int Created, ChatCompletionChunkChoice[] Choices);