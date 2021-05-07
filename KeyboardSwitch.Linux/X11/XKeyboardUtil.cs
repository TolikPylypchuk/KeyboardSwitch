using System;
using System.Linq;

using X11;

using static KeyboardSwitch.Linux.X11.Native;
using static X11.Xlib;

namespace KeyboardSwitch.Linux.X11
{
    public static class XKeyboardUtil
    {
        public static void SimulateKeyPresses(params XKeySym[] keys)
        {
            using var display = new XDisplayHandle(XOpenDisplay(String.Empty));

            var keyCodes = keys.Select(key => XKeysymToKeycode(display.DangerousGetHandle(), (KeySym)key)).ToList();

            foreach (var key in keyCodes)
            {
                XTestFakeKeyEvent(display, key, isPress: true, 0);
            }

            keyCodes.Reverse();

            foreach (var key in keyCodes)
            {
                XTestFakeKeyEvent(display, key, isPress: false, 0);
            }
        }
    }
}
