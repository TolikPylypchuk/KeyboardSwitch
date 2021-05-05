using System;

using KeyboardSwitch.Core.Services;
using KeyboardSwitch.Linux.X11;

using X11;

using static KeyboardSwitch.Linux.X11.Native;

using static X11.Xlib;

namespace KeyboardSwitch.Linux.Services
{
    internal class XUserActivitySimulator : IUserActivitySimulator
    {
        private const KeySym XKeyLeftControl = (KeySym)0xffe3;
        private const KeySym XKeyC = (KeySym)0x0063;
        private const KeySym XKeyV = (KeySym)0x0076;

        public void SimulateCopy()
        {
            using var display = new XDisplayHandle(XOpenDisplay(String.Empty));

            var leftControl = XKeysymToKeycode(display.DangerousGetHandle(), XKeyLeftControl);
            var c = XKeysymToKeycode(display.DangerousGetHandle(), XKeyC);

            XTestFakeKeyEvent(display, leftControl, isPress: true, 0);
            XTestFakeKeyEvent(display, c, isPress: true, 0);
            XTestFakeKeyEvent(display, c, isPress: false, 0);
            XTestFakeKeyEvent(display, leftControl, isPress: false, 0);
        }

        public void SimulatePaste()
        {
            using var display = new XDisplayHandle(XOpenDisplay(String.Empty));

            var leftControl = XKeysymToKeycode(display.DangerousGetHandle(), XKeyLeftControl);
            var v = XKeysymToKeycode(display.DangerousGetHandle(), XKeyV);

            XTestFakeKeyEvent(display, leftControl, isPress: true, 0);
            XTestFakeKeyEvent(display, v, isPress: true, 0);
            XTestFakeKeyEvent(display, v, isPress: false, 0);
            XTestFakeKeyEvent(display, leftControl, isPress: false, 0);
        }
    }
}
