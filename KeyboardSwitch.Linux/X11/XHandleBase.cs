using System;
using System.Runtime.InteropServices;

namespace KeyboardSwitch.Linux.X11
{
    internal abstract class XHandleBase : SafeHandle
    {
        private protected XHandleBase()
            : base(IntPtr.Zero, true)
        { }

        public override bool IsInvalid =>
            this.handle == IntPtr.Zero;
    }
}
