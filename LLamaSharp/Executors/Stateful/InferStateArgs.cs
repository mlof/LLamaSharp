using System.Collections.Generic;

namespace LLama.Executors.Stateful
{
    /// <summary>
    ///     State arguments that are used in single inference
    /// </summary>
    public class InferStateArgs
    {
        public IList<string>? Antiprompts { get; set; }

        /// <summary>
        ///     Tokens count remained to be used. (n_remain)
        /// </summary>
        public int RemainedTokens { get; set; }

        public bool ReturnValue { get; set; }
        public bool WaitForInput { get; set; }
        public bool NeedToSaveSession { get; set; }
    }
}
