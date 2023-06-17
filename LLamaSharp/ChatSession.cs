﻿using LLama.Abstractions;
using LLama.Common;
using LLama.Transformations.Abstractions;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using LLama.Common.ChatHistory;
using LLama.Transformations;

namespace LLama
{
    public class ChatSession
    {
        private ILLamaExecutor _executor;
        private Common.ChatHistory.ChatHistory _history;
        private static readonly string _executorStateFilename = "ExecutorState.json";
        private static readonly string _modelStateFilename = "ModelState.st";
        public ILLamaExecutor Executor => _executor;
        public Common.ChatHistory.ChatHistory History => _history;
        public SessionParams Params { get; set; }
        public IHistoryTransform HistoryTransform { get; set; } = new DefaultHistoryTransform();
        public List<ITextTransform> InputTransformPipeline { get; set; } = new();
        public ITextStreamTransform OutputTransform = new EmptyTextOutputStreamTransform();

        public ChatSession(ILLamaExecutor executor, SessionParams? sessionParams = null)
        {
            _executor = executor;
            _history = new ChatHistory();
            Params = sessionParams ?? new SessionParams();
        }

        public ChatSession WithHistoryTransform(IHistoryTransform transform)
        {
            HistoryTransform = transform;
            return this;
        }

        public ChatSession AddInputTransform(ITextTransform transform)
        {
            InputTransformPipeline.Add(transform);
            return this;
        }

        public ChatSession WithOutputTransform(ITextStreamTransform transform)
        {
            OutputTransform = transform;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">The directory name to save the session. If the directory does not exist, a new directory will be created.</param>
        public virtual void SaveSession(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            _executor.Model.SaveState(Path.Combine(path, _modelStateFilename));
            if(Executor is StatelessExecutor)
            {

            }
            else if(Executor is StatefulExecutorBase statefulExecutor)
            {
                statefulExecutor.SaveState(Path.Combine(path, _executorStateFilename));
            }
            else
            {
                throw new System.NotImplementedException("You're using a customized executor. Please inherit ChatSession and rewrite the method.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">The directory name to load the session.</param>
        public virtual void LoadSession(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new FileNotFoundException($"Directory {path} does not exist.");
            }
            _executor.Model.LoadState(Path.Combine(path, _modelStateFilename));
            if (Executor is StatelessExecutor)
            {

            }
            else if (Executor is StatefulExecutorBase statefulExecutor)
            {
                statefulExecutor.LoadState(Path.Combine(path, _executorStateFilename));
            }
            else
            {
                throw new System.NotImplementedException("You're using a customized executor. Please inherit ChatSession and rewrite the method.");
            }
        }

        /// <summary>
        /// Get the response from the LLama model with chat histories.
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="inferenceParams"></param>
        /// <returns></returns>
        public IEnumerable<string> Chat(Common.ChatHistory.ChatHistory history, InferenceParams? inferenceParams = null, CancellationToken cancellationToken = default)
        {
            var prompt = HistoryTransform.HistoryToText(history);
            History.Messages.AddRange(HistoryTransform.TextToHistory(AuthorRole.User, prompt).Messages);
            StringBuilder sb = new();
            foreach (var result in ChatInternal(prompt, inferenceParams, cancellationToken))
            {
                yield return result;
                sb.Append(result);
            }
            History.Messages.AddRange(HistoryTransform.TextToHistory(AuthorRole.Assistant, sb.ToString()).Messages);
        }

        /// <summary>
        /// Get the response from the LLama model. Note that prompt could not only be the preset words, 
        /// but also the question you want to ask.
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="inferenceParams"></param>
        /// <returns></returns>
        public IEnumerable<string> Chat(string prompt, InferenceParams? inferenceParams = null, CancellationToken cancellationToken = default)
        {
            foreach(var inputTransform in InputTransformPipeline)
            {
                prompt = inputTransform.Transform(prompt);
            }
            History.Messages.AddRange(HistoryTransform.TextToHistory(AuthorRole.User, prompt).Messages);
            StringBuilder sb = new();
            foreach (var result in ChatInternal(prompt, inferenceParams, cancellationToken))
            {
                yield return result;
                sb.Append(result);
            }
            History.Messages.AddRange(HistoryTransform.TextToHistory(AuthorRole.Assistant, sb.ToString()).Messages);
        }

        /// <summary>
        /// Get the response from the LLama model with chat histories.
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="inferenceParams"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<string> ChatAsync(Common.ChatHistory.ChatHistory history, InferenceParams? inferenceParams = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var prompt = HistoryTransform.HistoryToText(history);
            History.Messages.AddRange(HistoryTransform.TextToHistory(AuthorRole.User, prompt).Messages);
            StringBuilder sb = new();
            await foreach (var result in ChatAsyncInternal(prompt, inferenceParams, cancellationToken))
            {
                yield return result;
                sb.Append(result);
            }
            History.Messages.AddRange(HistoryTransform.TextToHistory(AuthorRole.Assistant, sb.ToString()).Messages);
        }

        public async IAsyncEnumerable<string> ChatAsync(string prompt, InferenceParams? inferenceParams = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var inputTransform in InputTransformPipeline)
            {
                prompt = inputTransform.Transform(prompt);
            }
            History.Messages.AddRange(HistoryTransform.TextToHistory(AuthorRole.User, prompt).Messages);
            StringBuilder sb = new();
            await foreach (var result in ChatAsyncInternal(prompt, inferenceParams, cancellationToken))
            {
                yield return result;
                sb.Append(result);
            }
            History.Messages.AddRange(HistoryTransform.TextToHistory(AuthorRole.Assistant, sb.ToString()).Messages);
        }

        private IEnumerable<string> ChatInternal(string prompt, InferenceParams? inferenceParams = null, CancellationToken cancellationToken = default)
        {
            var results = _executor.Infer(prompt, inferenceParams, cancellationToken);
            return OutputTransform.Transform(results);
        }

        private async IAsyncEnumerable<string> ChatAsyncInternal(string prompt, InferenceParams? inferenceParams = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var results = _executor.InferAsync(prompt, inferenceParams, cancellationToken);
            await foreach (var item in OutputTransform.TransformAsync(results))
            {
                yield return item;
            }
        }
    }
}