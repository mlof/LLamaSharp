using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using LLama.Abstractions;
using LLama.Common;
using LLama.Native;

namespace LLama.Executors.Stateless
{
    using llama_token = Int32;

    /// <summary>
    ///     This executor infer the input as one-time job. Previous inputs won't impact on the
    ///     response to current input.
    /// </summary>
    public class StatelessExecutor : ILLamaExecutor
    {
        private readonly byte[] _originalState;

        public StatelessExecutor(LLamaModel model)
        {
            Model = model;
            var tokens = model.Tokenize(" ");
            Utils.Eval(Model.NativeHandle, tokens.ToArray(), 0, tokens.Count(), 0, Model.Params.Threads);
            _originalState = model.GetStateData();
        }

        public LLamaModel Model { get; }

        public IEnumerable<string> Infer(string text, InferenceParams? inferenceParams = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var n_past = 1;
            if (inferenceParams is null)
            {
                inferenceParams = new InferenceParams();
            }

            List<llama_token> lastTokens = new(inferenceParams.RepeatLastTokensCount);
            for (var i = 0; i < lastTokens.Count; i++)
            {
                lastTokens[i] = 0;
            }

            var tokens = Model.Tokenize(text).ToList();
            var n_prompt_tokens = tokens.Count;

            Utils.Eval(Model.NativeHandle, tokens.ToArray(), 0, n_prompt_tokens, n_past, Model.Params.Threads);

            lastTokens.AddRange(tokens);
            n_past += n_prompt_tokens;

            var max_tokens = inferenceParams.MaxTokens < 0 ? int.MaxValue : inferenceParams.MaxTokens;
            for (var i = 0; i < max_tokens; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Model.LoadState(_originalState);
                    break;
                }

                var repeat_last_n = inferenceParams.RepeatLastTokensCount < 0
                    ? Model.ContextSize
                    : inferenceParams.RepeatLastTokensCount;

                var tokenDataArray = Model.ApplyPenalty(lastTokens, inferenceParams.LogitBias, repeat_last_n,
                    inferenceParams.RepeatPenalty, inferenceParams.FrequencyPenalty, inferenceParams.PresencePenalty,
                    inferenceParams.PenalizeNL);

                var id = Model.Sample(tokenDataArray, inferenceParams.Temperature, inferenceParams.Mirostat,
                    inferenceParams.MirostatTau,
                    inferenceParams.MirostatEta, inferenceParams.TopK, inferenceParams.TopP, inferenceParams.TfsZ,
                    inferenceParams.TypicalP);

                lastTokens.Add(id);

                var response = Utils.TokenToString(id, Model.NativeHandle, Model.Encoding);
                yield return response;

                tokens.Clear();
                tokens.Add(id);

                if (inferenceParams.AntiPrompts is not null && inferenceParams.AntiPrompts.Count() > 0)
                {
                    var last_output = "";
                    foreach (var token in lastTokens)
                    {
                        last_output += Utils.PtrToString(NativeApi.llama_token_to_str(Model.NativeHandle, id),
                            Model.Encoding);
                    }

                    var should_break = false;
                    foreach (var antiprompt in inferenceParams.AntiPrompts)
                    {
                        if (last_output.EndsWith(antiprompt))
                        {
                            should_break = true;
                            break;
                        }
                    }

                    if (should_break)
                    {
                        break;
                    }
                }

                // when run out of context
                if (n_past + tokens.Count > Model.ContextSize)
                {
                    var n_left = n_past - inferenceParams.TokensKeep;

                    n_past = Math.Max(1, inferenceParams.TokensKeep);

                    // insert n_left/2 tokens at the start of embed from last_n_tokens
                    tokens.InsertRange(0,
                        lastTokens.Take(lastTokens.Count - tokens.Count)
                            .Skip(Model.ContextSize - (n_left / 2) - tokens.Count));
                }

                n_past = Model.Eval(tokens.ToArray(), n_past);
            }

            Model.LoadState(_originalState);
        }


        public async IAsyncEnumerable<string> InferAsync(string text, InferenceParams? inferenceParams = null,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            yield return "";
            throw new NotImplementedException();
        }
    }
}
