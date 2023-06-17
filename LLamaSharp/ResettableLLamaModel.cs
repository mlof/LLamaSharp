using LLama.Common;

namespace LLama
{
    /// <summary>
    ///     A LLamaModel what could be reset. Note that using this class will consume about 10% more memories.
    /// </summary>
    public class ResettableLLamaModel : LLamaModel
    {
        public ResettableLLamaModel(ModelParams Params, string encoding = "UTF-8") : base(Params, encoding)
        {
            OriginalState = GetStateData();
        }

        public byte[] OriginalState { get; set; }

        public void Reset()
        {
            LoadState(OriginalState);
        }
    }
}
