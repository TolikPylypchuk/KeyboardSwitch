using X11;

namespace KeyboardSwitch.Linux.X11
{
    internal sealed class XDisplayHandle : XHandleBase
    {
        private XDisplayHandle()
            : base()
        { }

        protected override bool ReleaseHandle() =>
            Xlib.XCloseDisplay(this.handle) != Status.Failure;
    }
}
