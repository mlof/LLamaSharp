﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using LLama.Abstractions;
using LLama.Common;
using LLama.Enumerations;
using LLama.Exceptions;
using LLama.Native;
using Microsoft.Extensions.Logging;

namespace LLama.Executors.Stateful
{
    using llama_token = Int32;

    public abstract class StatefulExecutorBase : ILLamaExecutor
    {
        protected readonly LLamaModel _model;
        protected int _consumedTokensCount; // n_consume
        protected List<llama_token> _embed_inps = new();
        protected List<llama_token> _embeds = new(); // embd
        protected FixedSizeQueue<llama_token> _last_n_tokens;
        protected ILogger? _logger;
        protected int _n_matching_session_tokens;
        protected int _n_session_consumed;
        protected int _pastTokensCount; // n_past
        protected string? _pathSession;
        protected List<llama_token> _session_tokens = new();

        protected StatefulExecutorBase(LLamaModel model, ILogger? logger = null)
        {
            _model = model;
            _logger = logger;
            _pastTokensCount = 0;
            _consumedTokensCount = 0;
            _n_session_consumed = 0;
            _embeds = new List<int>();
            _embed_inps = new List<int>();
            _last_n_tokens = new FixedSizeQueue<llama_token>(_model.ContextSize).FillWith(0);
        }

        public LLamaModel Model => _model;


        public virtual IEnumerable<string> Infer(string text, InferenceParams? inferenceParams = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (inferenceParams is null)
            {
                inferenceParams = new InferenceParams();
            }

            var args = new InferStateArgs
            {
                Antiprompts = inferenceParams.AntiPrompts.ToList(),
                RemainedTokens = inferenceParams.MaxTokens,
                ReturnValue = false,
                WaitForInput = false,
                NeedToSaveSession = !string.IsNullOrEmpty(_pathSession) &&
                                    _n_matching_session_tokens < _embed_inps.Count
            };

            PreprocessInputs(text, args);

            while (GetLoopCondition(args))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                InferInternal(inferenceParams, args);

                if (args.ReturnValue)
                {
                    foreach (var item in _model.GenerateResult(_embeds))
                    {
                        yield return item;
                    }
                }

                var breakGeneration = PostProcess(inferenceParams, args, out var extraOutputs);
                if (extraOutputs is not null)
                {
                    foreach (var item in extraOutputs)
                    {
                        yield return item;
                    }
                }

                if (breakGeneration)
                {
                    break;
                }
            }
        }

        public virtual async IAsyncEnumerable<string> InferAsync(string text, InferenceParams? inferenceParams = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var result in Infer(text, inferenceParams, cancellationToken))
            {
                yield return result;
            }
        }

        public unsafe StatefulExecutorBase WithSessionFile(string filename)
        {
            _pathSession = filename;
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("File name cannot be empty.");
            }

            if (File.Exists(filename))
            {
                _logger?.LogInformation($"Attempting to load saved session from {filename}");
                var session_tokens = new llama_token[_model.ContextSize];
                ulong n_token_count_out = 0;
                if (!NativeApi.llama_load_session_file(_model.NativeHandle, _pathSession, session_tokens,
                        (ulong)_model.ContextSize, &n_token_count_out))
                {
                    _logger?.LogError($"Failed to load session file {filename}");
                    throw new RuntimeError($"Failed to load session file {_pathSession}");
                }

                _session_tokens = session_tokens.Take((int)n_token_count_out).ToList();
                _logger?.LogInformation($"Loaded a session with prompt size of {session_tokens.Length} tokens");
            }
            else
            {
                _logger?.LogWarning("Session file does not exist, will create");
            }

            _n_matching_session_tokens = 0;
            if (_session_tokens.Count > 0)
            {
                foreach (var id in _session_tokens)
                {
                    if (_n_matching_session_tokens >= _embed_inps.Count ||
                        id != _embed_inps[_n_matching_session_tokens])
                    {
                        break;
                    }

                    _n_matching_session_tokens++;
                }

                if (_n_matching_session_tokens >= _embed_inps.Count)
                {
                    _logger?.LogInformation("Session file has exact match for prompt!");
                }
                else if (_n_matching_session_tokens < _embed_inps.Count / 2)
                {
                    _logger?.LogWarning(
                        $"session file has low similarity to prompt ({_n_matching_session_tokens}" +
                        $" / {_embed_inps.Count} tokens); will mostly be reevaluated");
                }
                else
                {
                    _logger?.LogInformation( $"Session file matches {_n_matching_session_tokens} / " +
                                                  $"{_embed_inps.Count} tokens of prompt");
                }
            }

            return this;
        }

        public void SaveSessionFile(string filename)
        {
            var session_token_array = _session_tokens.ToArray();
            NativeApi.llama_save_session_file(_model.NativeHandle, filename, session_token_array,
                (ulong)session_token_array.Length);
        }

        protected virtual void HandleRunOutOfContext(int tokensToKeep)
        {
            // if we run out of context:
            // - take the tokensToKeep first tokens from the original prompt (via n_past)
            // - take half of the last (n_ctx - tokensToKeep) tokens and recompute the logits in batches
            var n_left = _pastTokensCount - tokensToKeep;

            _pastTokensCount = Math.Max(1, tokensToKeep);

            // insert n_left/2 tokens at the start of embed from last_n_tokens
            _embeds.InsertRange(0,
                _last_n_tokens.Take(_last_n_tokens.Count - _embeds.Count)
                    .Skip(_model.ContextSize - (n_left / 2) - _embeds.Count));

            // stop saving session if we run out of context
            _pathSession = string.Empty;
        }

        protected virtual void TryReuseMathingPrefix()
        {
            if (_n_session_consumed < _session_tokens.Count)
            {
                var i = 0;
                for (; i < _embeds.Count; i++)
                {
                    if (_embeds[i] != _session_tokens[_n_session_consumed])
                    {
                        _session_tokens = _session_tokens.Take(_n_session_consumed).ToList();
                        break;
                    }

                    _pastTokensCount++;
                    _n_session_consumed++;

                    if (_n_session_consumed >= _session_tokens.Count)
                    {
                        i++;
                        break;
                    }
                }

                if (i > 0)
                {
                    _embeds.RemoveRange(0, i);
                }
            }
        }

        protected abstract bool GetLoopCondition(InferStateArgs args);
        protected abstract void PreprocessInputs(string text, InferStateArgs args);

        protected abstract bool PostProcess(InferenceParams inferenceParams, InferStateArgs args,
            out IEnumerable<string>? extraOutputs);

        protected abstract void InferInternal(InferenceParams inferenceParams, InferStateArgs args);
        public abstract void SaveState(string filename);
        public abstract void LoadState(string filename);
    }
}
