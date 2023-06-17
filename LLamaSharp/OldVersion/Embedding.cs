namespace LLama.OldVersion;

public record Embedding(string Object, string Model, EmbeddingData[] Data, EmbeddingUsage Usage);