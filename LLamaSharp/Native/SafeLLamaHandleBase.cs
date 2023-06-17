using System;
using System.Runtime.InteropServices;

namespace LLama.Native
{
    public abstract class SafeLLamaHandleBase : SafeHandle
    {
        private protected SafeLLamaHandleBase()
            : base(IntPtr.Zero, true)
        {
        }

        private protected SafeLLamaHandleBase(IntPtr handle)
            : base(IntPtr.Zero, true)
        {
            SetHandle(handle);
        }

        private protected SafeLLamaHandleBase(IntPtr handle, bool ownsHandle)
            : base(IntPtr.Zero, ownsHandle)
        {
            SetHandle(handle);
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        public override string ToString()
        {
            return $"0x{handle.ToString("x16")}";
        }
    }
}
