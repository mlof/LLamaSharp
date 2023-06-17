using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LLama.Common;
using LLama.Exceptions;
using LLama.Extensions;
using LLama.Native;
using Microsoft.Extensions.Logging;

namespace LLama
{
    using llama_token = Int32;

    public class LLamaModel : IDisposable
    {
        // TODO: expose more properties.
        private readonly ILogger? _logger;

        public LLamaModel(ModelParams Params, string encoding = "UTF-8", ILogger? logger = null)
        {
            _logger = logger;
            this.Params = Params;
            Encoding = Encoding.GetEncoding(encoding);
            _logger?.LogInformation($"Initializing LLama model with params: {this.Params}");
            NativeHandle = Utils.InitLLamaContextFromModelParams(this.Params);
            ContextSize = NativeApi.llama_n_ctx(NativeHandle);
        }

        public int ContextSize { get; }
        public ModelParams Params { get; set; }
        public SafeLLamaContextHandle NativeHandle { get; }

        public Encoding Encoding { get; }

        public void Dispose()
        {
            NativeHandle.Dispose();
        }

        /// <summary>
        ///     Tokenize a string.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="addBos">Whether to add a bos to the text.</param>
        /// <returns></returns>
        public IEnumerable<llama_token> Tokenize(string text, bool addBos = true)
        {
            // TODO: reconsider whether to convert to array here.
            return Utils.Tokenize(NativeHandle, text, addBos, Encoding);
        }

        /// <summary>
        ///     Detokenize the tokens to text.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public string DeTokenize(IEnumerable<llama_token> tokens)
        {
            StringBuilder sb = new();
            foreach (var token in tokens)
            {
                sb.Append(Utils.PtrToString(NativeApi.llama_token_to_str(NativeHandle, token), Encoding));
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Save the state to specified path.
        /// </summary>
        /// <param name="filename"></param>
        public void SaveState(string filename)
        {
            File.WriteAllBytes(filename, GetStateData());
        }

        /// <summary>
        ///     Get the state data as a byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] GetStateData()
        {
            var stateSize = NativeApi.llama_get_state_size(NativeHandle);
            var stateMemory = new byte[stateSize];
            NativeApi.llama_copy_state_data(NativeHandle, stateMemory);
            return stateMemory;
        }

        /// <summary>
        ///     Load the state from specified path.
        /// </summary>
        /// <param name="filename"></param>
        /// <exception cref="RuntimeError"></exception>
        public void LoadState(string filename)
        {
            var stateMemory = File.ReadAllBytes(filename);
            LoadState(stateMemory);
        }

        /// <summary>
        ///     Load the state from memory.
        /// </summary>
        /// <param name="stateData"></param>
        /// <exception cref="RuntimeError"></exception>
        public void LoadState(byte[] stateData)
        {
            var stateSize = (int)NativeApi.llama_get_state_size(NativeHandle);
            if (stateData.Length != stateSize)
            {
                throw new RuntimeError("Failed to validate state size.");
            }

            NativeApi.llama_set_state_data(NativeHandle, stateData);
        }

        public llama_token Sample(LLamaTokenDataArray candidates, float temperature = 0.8f,
            MiroStateType mirostat = MiroStateType.Disable,
            float mirostatTau = 5.0f, float mirostatEta = 0.1f, int topK = 40, float topP = 0.95f, float tfsZ = 1.0f,
            float typicalP = 1.0f)
        {
            var id = 0;
            if (temperature <= 0)
            {
                // Greedy sampling
                id = SamplingApi.llama_sample_token_greedy(NativeHandle, candidates);
            }
            else
            {
                if (mirostat == MiroStateType.MiroState)
                {
                    var mirostat_mu = 2.0f * mirostatTau;
                    const int mirostat_m = 100;
                    SamplingApi.llama_sample_temperature(NativeHandle, candidates, temperature);
                    id = SamplingApi.llama_sample_token_mirostat(NativeHandle, candidates, mirostatTau, mirostatEta,
                        mirostat_m, ref mirostat_mu);
                }
                else if (mirostat == MiroStateType.MiroState2)
                {
                    var mirostat_mu = 2.0f * mirostatTau;
                    SamplingApi.llama_sample_temperature(NativeHandle, candidates, temperature);
                    id = SamplingApi.llama_sample_token_mirostat_v2(NativeHandle, candidates, mirostatTau, mirostatEta,
                        ref mirostat_mu);
                }
                else
                {
                    // Temperature sampling
                    SamplingApi.llama_sample_top_k(NativeHandle, candidates, topK, 1);
                    SamplingApi.llama_sample_tail_free(NativeHandle, candidates, tfsZ, 1);
                    SamplingApi.llama_sample_typical(NativeHandle, candidates, typicalP, 1);
                    SamplingApi.llama_sample_top_p(NativeHandle, candidates, topP, 1);
                    SamplingApi.llama_sample_temperature(NativeHandle, candidates, temperature);
                    id = SamplingApi.llama_sample_token(NativeHandle, candidates);
                }
            }

            return id;
        }

        public LLamaTokenDataArray ApplyPenalty(IEnumerable<llama_token> lastTokens,
            Dictionary<llama_token, float>? logitBias = null,
            int repeatLastTokensCount = 64, float repeatPenalty = 1.1f, float alphaFrequency = .0f,
            float alphaPresence = .0f,
            bool penalizeNL = true)
        {
            var n_vocab = NativeApi.llama_n_vocab(NativeHandle);
            var logits = Utils.GetLogits(NativeHandle, n_vocab);

            // Apply params.logit_bias map
            if (logitBias is not null)
            {
                foreach (var (key, value) in logitBias)
                {
                    logits[key] += value;
                }
            }

            var candidates = new List<LLamaTokenData>();
            candidates.Capacity = n_vocab;
            for (var token_id = 0; token_id < n_vocab; token_id++)
            {
                candidates.Add(new LLamaTokenData(token_id, logits[token_id], 0.0f));
            }

            var candidates_p = new LLamaTokenDataArray(candidates.ToArray(), (ulong)candidates.Count, false);

            // Apply penalties
            var nl_logit = logits[NativeApi.llama_token_nl()];
            var lastTokensCount = lastTokens.Count();
            var last_n_repeat = Math.Min(Math.Min(lastTokensCount, repeatLastTokensCount), ContextSize);
            SamplingApi.llama_sample_repetition_penalty(NativeHandle, candidates_p,
                lastTokens.Skip(lastTokensCount - last_n_repeat).ToArray(),
                (ulong)last_n_repeat, repeatPenalty);
            SamplingApi.llama_sample_frequency_and_presence_penalties(NativeHandle, candidates_p,
                lastTokens.Skip(lastTokensCount - last_n_repeat).ToArray(),
                (ulong)last_n_repeat, alphaFrequency, alphaPresence);
            if (!penalizeNL)
            {
                logits[NativeApi.llama_token_nl()] = nl_logit;
            }

            return candidates_p;
        }

        /// <summary>
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="pastTokensCount"></param>
        /// <returns>The updated `pastTokensCount`.</returns>
        /// <exception cref="RuntimeError"></exception>
        public llama_token Eval(llama_token[] tokens, llama_token pastTokensCount)
        {
            var total = tokens.Length;
            for (var i = 0; i < total; i += Params.BatchSize)
            {
                var n_eval = total - i;
                if (n_eval > Params.BatchSize)
                {
                    n_eval = Params.BatchSize;
                }

                if (Utils.Eval(NativeHandle, tokens, i, n_eval, pastTokensCount, Params.Threads) != 0)
                {
                    _logger?.LogError("Failed to eval.");
                    throw new RuntimeError("Failed to eval.");
                }

                pastTokensCount += n_eval;
            }

            return pastTokensCount;
        }

        // TODO: add comment
        internal IEnumerable<string> GenerateResult(IEnumerable<llama_token> ids)
        {
            foreach (var id in ids)
            {
                yield return Utils.TokenToString(id, NativeHandle, Encoding);
            }
        }
    }
}
