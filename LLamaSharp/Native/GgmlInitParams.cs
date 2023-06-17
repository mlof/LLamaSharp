using System;
using System.Runtime.InteropServices;

namespace LLama.Native
{
    internal struct GgmlInitParams
    {
        public ulong mem_size;
        public IntPtr mem_buffer;
        [MarshalAs(UnmanagedType.I1)]
        public bool no_alloc;
    }
}
