﻿using LLama.Common;
using LLama.Exceptions;
using LLama.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LLama
{
    using llama_token = Int32;
    internal static class Utils
    {
        public static SafeLLamaContextHandle InitLLamaContextFromModelParams(ModelParams @params)
        {
            var lparams = NativeApi.llama_context_default_params();

            lparams.n_ctx = @params.ContextSize;
            lparams.n_gpu_layers = @params.GpuLayerCount;
            lparams.seed = @params.Seed;
            lparams.f16_kv = @params.UseFp16Memory;
            lparams.use_mmap = @params.UseMemoryLock;
            lparams.use_mlock = @params.UseMemoryLock;
            lparams.logits_all = @params.Perplexity;
            lparams.embedding = @params.EmbeddingMode;

            if (!File.Exists(@params.ModelPath))
            {
                throw new FileNotFoundException($"The model file does not exist: {@params.ModelPath}");
            }

            var ctx_ptr = NativeApi.llama_init_from_file(@params.ModelPath, lparams);

            if (ctx_ptr == IntPtr.Zero)
            {
                throw new RuntimeError($"Failed to load model {@params.ModelPath}.");
            }

            SafeLLamaContextHandle ctx = new(ctx_ptr);

            if (!string.IsNullOrEmpty(@params.LoraAdapter))
            {
                int err = NativeApi.llama_apply_lora_from_file(ctx, @params.LoraAdapter,
                    string.IsNullOrEmpty(@params.LoraBase) ? null : @params.LoraBase, @params.Threads);
                if (err != 0)
                {
                    throw new RuntimeError("Failed to apply lora adapter.");
                }
            }
            return ctx;
        }

        public static IEnumerable<llama_token> Tokenize(SafeLLamaContextHandle ctx, string text, bool add_bos, Encoding encoding)
        {
            var cnt = encoding.GetByteCount(text);
            llama_token[] res = new llama_token[cnt + (add_bos ? 1 : 0)];
            int n = NativeApi.llama_tokenize(ctx, text, encoding, res, res.Length, add_bos);
            if (n < 0)
            {
                throw new RuntimeError("Error happened during tokenization. It's possibly caused by wrong encoding. Please try to " +
                    "specify the encoding.");
            }
            return res.Take(n);
        }

        public unsafe static Span<float> GetLogits(SafeLLamaContextHandle ctx, int length)
        {
            var logits = NativeApi.llama_get_logits(ctx);
            return new Span<float>(logits, length);
        }

        public static unsafe int Eval(SafeLLamaContextHandle ctx, llama_token[] tokens, int startIndex, int n_tokens, int n_past, int n_threads)
        {
            int result;
            fixed(llama_token* p = tokens)
            {
                result = NativeApi.llama_eval_with_pointer(ctx, p + startIndex, n_tokens, n_past, n_threads);
            }
            return result;
        }

        public static string TokenToString(llama_token token, SafeLLamaContextHandle ctx, Encoding encoding)
        {
            return PtrToString(NativeApi.llama_token_to_str(ctx, token), encoding);
        }

        public static unsafe string PtrToString(IntPtr ptr, Encoding encoding)
        {
#if NET6_0_OR_GREATER
            if(encoding == Encoding.UTF8)
            {
                return Marshal.PtrToStringUTF8(ptr);
            }
            else if(encoding == Encoding.Unicode)
            {
                return Marshal.PtrToStringUni(ptr);
            }
            else
            {
                return Marshal.PtrToStringAuto(ptr);
            }
#else
            byte* tp = (byte*)ptr.ToPointer();
            List<byte> bytes = new();
            while (true)
            {
                byte c = *tp++;
                if (c == '\0')
                {
                    break;
                }
                else
                {
                    bytes.Add(c);
                }
            }
            return encoding.GetString(bytes.ToArray());
#endif
        }
    }
}
