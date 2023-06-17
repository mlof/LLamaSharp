using System.Collections.Generic;
using System.Linq;
using LLama.Transformations.Abstractions;

namespace LLama.Transformations
{
    /// <summary>
    ///     A text output transform that removes the keywords from the response.
    /// </summary>
    public class KeywordTextOutputStreamTransform : ITextStreamTransform
    {
        private readonly HashSet<string> _keywords;
        private readonly int _maxKeywordLength;
        private readonly bool _removeAllMatchedTokens;

        /// <summary>
        /// </summary>
        /// <param name="keywords">Keywords that you want to remove from the response.</param>
        /// <param name="redundancyLength">
        ///     The extra length when searching for the keyword. For example, if your only keyword is "highlight",
        ///     maybe the token you get is "\r\nhighligt". In this condition, if redundancyLength=0, the token cannot be
        ///     successfully matched because the length of "\r\nhighligt" (10)
        ///     has already exceeded the maximum length of the keywords (8). On the contrary, setting redundancyLengyh >= 2 leads
        ///     to successful match.
        ///     The larger the redundancyLength is, the lower the processing speed. But as an experience, it won't introduce too
        ///     much performance impact when redundancyLength <= 5 </param>
        /// <param name="removeAllMatchedTokens">
        ///     If set to true, when getting a matched keyword, all the related tokens will be
        ///     removed. Otherwise only the part of keyword will be removed.
        /// </param>
        public KeywordTextOutputStreamTransform(IEnumerable<string> keywords, int redundancyLength = 3,
            bool removeAllMatchedTokens = false)
        {
            _keywords = new HashSet<string>(keywords);
            _maxKeywordLength = keywords.Select(x => x.Length).Max() + redundancyLength;
            _removeAllMatchedTokens = removeAllMatchedTokens;
        }

        /// <inheritdoc />
        public IEnumerable<string> Transform(IEnumerable<string> tokens)
        {
            var window = new Queue<string>();

            foreach (var s in tokens)
            {
                window.Enqueue(s);
                var current = string.Join("", window);
                if (_keywords.Any(x => current.Contains(x)))
                {
                    var total = window.Count;
                    for (var i = 0; i < total; i++)
                    {
                        window.Dequeue();
                    }
                }

                if (current.Length >= _maxKeywordLength)
                {
                    var total = window.Count;
                    for (var i = 0; i < total; i++)
                    {
                        yield return window.Dequeue();
                    }
                }
            }

            var totalCount = window.Count;
            for (var i = 0; i < totalCount; i++)
            {
                yield return window.Dequeue();
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<string> TransformAsync(IAsyncEnumerable<string> tokens)
        {
            var window = new Queue<string>();

            await foreach (var s in tokens)
            {
                window.Enqueue(s);
                var current = string.Join("", window);
                if (_keywords.Any(x => current.Contains(x)))
                {
                    var matchedKeyword = _keywords.First(x => current.Contains(x));
                    var total = window.Count;
                    for (var i = 0; i < total; i++)
                    {
                        window.Dequeue();
                    }

                    if (!_removeAllMatchedTokens)
                    {
                        yield return current.Replace(matchedKeyword, "");
                    }
                }

                if (current.Length >= _maxKeywordLength)
                {
                    var total = window.Count;
                    for (var i = 0; i < total; i++)
                    {
                        yield return window.Dequeue();
                    }
                }
            }

            var totalCount = window.Count;
            for (var i = 0; i < totalCount; i++)
            {
                yield return window.Dequeue();
            }
        }
    }
}
