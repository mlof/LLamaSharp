using System.Collections.Generic;
using LLama.Transformations.Abstractions;

namespace LLama.Transformations
{
    /// <summary>
    ///     A no-op text input transform.
    /// </summary>
    public class EmptyTextOutputStreamTransform : ITextStreamTransform
    {
        public IEnumerable<string> Transform(IEnumerable<string> tokens)
        {
            return tokens;
        }

        public IAsyncEnumerable<string> TransformAsync(IAsyncEnumerable<string> tokens)
        {
            return tokens;
        }
    }
}
