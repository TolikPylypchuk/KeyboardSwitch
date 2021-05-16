using System;
using System.Linq;
using System.Runtime.InteropServices;

using X11;

using static KeyboardSwitch.Linux.X11.Native;
using static X11.Xlib;

namespace KeyboardSwitch.Linux.X11
{
    internal static class XUtil
    {
        public static XDisplayHandle OpenXDisplay()
        {
            XkbIgnoreExtension(false);

            int major = XkbMajorVersion;
            int minor = XkbMinorVersion;

            var display = XkbOpenDisplay(String.Empty, out _, out _, ref major, ref minor, out var result);
            ValidateXOpenDisplayResult(result);

            return display;
        }

        public static (byte Min, byte Max) GetMinAndMaxKeyCodes(XDisplayHandle display)
        {
            using var keyboardDescHandle = new XMapHandle(
                XkbGetMap(display, XkbMapComponentMask.XkbAllClientInfoMask, XkbKeyboardSpec.XkbUseCoreKbd),
                XkbMapComponentMask.XkbAllClientInfoMask);

            var keyboardDesc = Marshal.PtrToStructure<XkbDesc>(keyboardDescHandle.DangerousGetHandle());

            return (keyboardDesc.MinKeyCode, keyboardDesc.MaxKeyCode);
        }

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

        private static void ValidateXOpenDisplayResult(XOpenDisplayResult result)
        {
            switch (result)
            {
                case XOpenDisplayResult.BadLibraryVersion:
                    throw new XException("Bad X11 version");
                case XOpenDisplayResult.ConnectionRefused:
                    throw new XException("Connection to X server refused");
                case XOpenDisplayResult.NonXkbServer:
                    throw new XException("XKB not present");
                case XOpenDisplayResult.BadServerVersion:
                    throw new XException("Bad X11 server version");
            }
        }
    }
}
