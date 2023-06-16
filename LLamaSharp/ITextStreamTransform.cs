﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LLama
{
    public interface ITextStreamTransform
    {
        IEnumerable<string> Transform(IEnumerable<string> tokens);
        IAsyncEnumerable<string> TransformAsync(IAsyncEnumerable<string> tokens);
    }
}
