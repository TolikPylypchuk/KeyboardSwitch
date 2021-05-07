using System;

using X11;

namespace KeyboardSwitch.Linux.X11
{
    internal sealed class XDisplayHandle : XHandleBase
    {
        public XDisplayHandle(IntPtr handle) =>
            this.SetHandle(handle);

        private XDisplayHandle()
        { }

        protected override bool ReleaseHandle() =>
            Xlib.XCloseDisplay(this.handle) != Status.Failure;
    }
}
