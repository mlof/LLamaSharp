using System;
using System.Runtime.InteropServices;

namespace LLama.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LLamaTokenDataArrayNative
    {
        public IntPtr data;
        public ulong size;
        public bool sorted;
    }
}
