using LLama.Transformations.Abstractions;

namespace LLama.Transformations
{
    /// <summary>
    ///     A text input transform that only trims the text.
    /// </summary>
    public class NaiveTextInputTransform : ITextTransform
    {
        /// <inheritdoc />
        public string Transform(string text)
        {
            return text.Trim();
        }
    }
}
