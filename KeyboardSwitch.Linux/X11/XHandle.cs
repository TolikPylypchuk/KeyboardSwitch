using static KeyboardSwitch.Linux.X11.Native;

namespace KeyboardSwitch.Linux.X11
{
    internal sealed class XHandle : XHandleBase
    {
        protected override bool ReleaseHandle() =>
            XFree(this.handle);
    }
}
