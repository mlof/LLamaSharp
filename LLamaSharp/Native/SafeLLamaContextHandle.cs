using System;

namespace LLama.Native
{
    public class SafeLLamaContextHandle: SafeLLamaHandleBase
    {
        protected SafeLLamaContextHandle()
        {
        }

        public SafeLLamaContextHandle(IntPtr handle)
            : base(handle)
        {
        }

        protected override bool ReleaseHandle()
        {
            NativeApi.llama_free(handle);
            SetHandle(IntPtr.Zero);
            return true;
        }
    }
}
