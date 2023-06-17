using System.Collections.Generic;

namespace LLama.OldVersion;

public record CompletionLogprobs(int[] TextOffset, float[] TokenLogProbs, string[] Tokens, Dictionary<string, float>[] TopLogprobs);