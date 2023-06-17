﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LLama.Transformations.Abstractions
{
    /// <summary>
    /// Takes a stream of tokens and transforms them.
    /// </summary>
    public interface ITextStreamTransform
    {
        /// <summary>
        /// Takes a stream of tokens and transforms them, returning a new stream of tokens.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        IEnumerable<string> Transform(IEnumerable<string> tokens);
        /// <summary>
        /// Takes a stream of tokens and transforms them, returning a new stream of tokens asynchronously.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        IAsyncEnumerable<string> TransformAsync(IAsyncEnumerable<string> tokens);
    }
}
