using KeyboardSwitch.Core.Keyboard;

namespace KeyboardSwitch.Linux.X11
{
    internal static class Extensions
    {
        public static char ToChar(this XKeySym keysym)
        {
            ulong keysymValue = (ulong)keysym;
            ushort keysymShortValue = (ushort)keysymValue;

            if ((keysymValue >= 0x0020 && keysymValue <= 0x007E) ||
                (keysymValue >= 0x00A0 && keysymValue <= 0x00FF))
            {
                return (char)keysymShortValue;
            }

            if (keysym == XKeySym.KeyPadSpace)
            {
                return (char)((short)XKeySym.Space & 0x7F);
            }

            if ((keysym >= XKeySym.BackSpace && keysym <= XKeySym.Clear) ||
                (keysym >= XKeySym.KeyPadMultiply && keysym <= XKeySym.KeyPad9) ||
                keysym == XKeySym.Return || keysym == XKeySym.Escape ||
                keysym == XKeySym.Delete || keysym == XKeySym.Tab ||
                keysym == XKeySym.KeyPadEnter || keysym == XKeySym.KeyPadEqual)
            {
                return (char)(keysymShortValue & 0x7F);
            }

            if (0x01000000 <= keysymValue && keysymValue <= 0x0110FFFF)
            {
                return (char)(keysymValue - 0x01000000);
            }

            return XKeySymToChar.Mappings.ContainsKey(keysymShortValue)
                ? (char)XKeySymToChar.Mappings[keysymShortValue]
                : '\0';
        }

        public static XKeySym ToKeySym(this KeyCode keyCode) =>
            keyCode switch
            {
                KeyCode.VcEscape => XKeySym.Escape,

                KeyCode.VcF1 => XKeySym.F1,
                KeyCode.VcF2 => XKeySym.F2,
                KeyCode.VcF3 => XKeySym.F3,
                KeyCode.VcF4 => XKeySym.F4,
                KeyCode.VcF5 => XKeySym.F5,
                KeyCode.VcF6 => XKeySym.F6,
                KeyCode.VcF7 => XKeySym.F7,
                KeyCode.VcF8 => XKeySym.F8,
                KeyCode.VcF9 => XKeySym.F8,
                KeyCode.VcF10 => XKeySym.F10,
                KeyCode.VcF11 => XKeySym.F11,
                KeyCode.VcF12 => XKeySym.F12,
                KeyCode.VcF13 => XKeySym.F13,
                KeyCode.VcF14 => XKeySym.F14,
                KeyCode.VcF15 => XKeySym.F15,
                KeyCode.VcF16 => XKeySym.F16,
                KeyCode.VcF17 => XKeySym.F17,
                KeyCode.VcF18 => XKeySym.F18,
                KeyCode.VcF19 => XKeySym.F19,
                KeyCode.VcF20 => XKeySym.F20,
                KeyCode.VcF21 => XKeySym.F21,
                KeyCode.VcF22 => XKeySym.F22,
                KeyCode.VcF23 => XKeySym.F23,
                KeyCode.VcF24 => XKeySym.F24,

                KeyCode.VcBackquote => XKeySym.GraveAccent,

                KeyCode.Vc1 => XKeySym.Key1,
                KeyCode.Vc2 => XKeySym.Key2,
                KeyCode.Vc3 => XKeySym.Key3,
                KeyCode.Vc4 => XKeySym.Key4,
                KeyCode.Vc5 => XKeySym.Key5,
                KeyCode.Vc6 => XKeySym.Key6,
                KeyCode.Vc7 => XKeySym.Key7,
                KeyCode.Vc8 => XKeySym.Key8,
                KeyCode.Vc9 => XKeySym.Key9,
                KeyCode.Vc0 => XKeySym.Key0,

                KeyCode.VcMinus => XKeySym.Minus,
                KeyCode.VcEquals => XKeySym.Equal,
                KeyCode.VcBackspace => XKeySym.BackSpace,

                KeyCode.VcTab => XKeySym.Tab,
                KeyCode.VcCapsLock => XKeySym.CapsLock,

                KeyCode.VcA => XKeySym.A,
                KeyCode.VcB => XKeySym.B,
                KeyCode.VcC => XKeySym.C,
                KeyCode.VcD => XKeySym.D,
                KeyCode.VcE => XKeySym.E,
                KeyCode.VcF => XKeySym.F,
                KeyCode.VcG => XKeySym.G,
                KeyCode.VcH => XKeySym.H,
                KeyCode.VcI => XKeySym.I,
                KeyCode.VcJ => XKeySym.J,
                KeyCode.VcK => XKeySym.K,
                KeyCode.VcL => XKeySym.L,
                KeyCode.VcM => XKeySym.M,
                KeyCode.VcN => XKeySym.N,
                KeyCode.VcO => XKeySym.O,
                KeyCode.VcP => XKeySym.P,
                KeyCode.VcQ => XKeySym.Q,
                KeyCode.VcR => XKeySym.R,
                KeyCode.VcS => XKeySym.S,
                KeyCode.VcT => XKeySym.T,
                KeyCode.VcU => XKeySym.U,
                KeyCode.VcV => XKeySym.V,
                KeyCode.VcW => XKeySym.W,
                KeyCode.VcX => XKeySym.X,
                KeyCode.VcY => XKeySym.Y,
                KeyCode.VcZ => XKeySym.Z,

                KeyCode.VcOpenBracket => XKeySym.LeftBracket,
                KeyCode.VcCloseBracket => XKeySym.RightBracket,
                KeyCode.VcBackSlash => XKeySym.Backslash,

                KeyCode.VcSemicolon => XKeySym.Semicolon,
                KeyCode.VcQuote => XKeySym.Apostrophe,
                KeyCode.VcEnter => XKeySym.Return,

                KeyCode.VcComma => XKeySym.Comma,
                KeyCode.VcPeriod => XKeySym.Period,
                KeyCode.VcSlash => XKeySym.Slash,

                KeyCode.VcSpace => XKeySym.Space,

                KeyCode.VcPrintscreen => XKeySym.Print,
                KeyCode.VcScrollLock => XKeySym.ScrollLock,
                KeyCode.VcPause => XKeySym.Pause,

                KeyCode.VcInsert => XKeySym.Insert,
                KeyCode.VcDelete => XKeySym.Delete,
                KeyCode.VcHome => XKeySym.Home,
                KeyCode.VcEnd => XKeySym.End,
                KeyCode.VcPageUp => XKeySym.PageUp,
                KeyCode.VcPageDown => XKeySym.PageDown,

                KeyCode.VcUp => XKeySym.Up,
                KeyCode.VcLeft => XKeySym.Left,
                KeyCode.VcClear => XKeySym.Clear,
                KeyCode.VcRight => XKeySym.Right,
                KeyCode.VcDown => XKeySym.Down,

                KeyCode.VcNumLock => XKeySym.NumLock,
                KeyCode.VcNumPadDivide => XKeySym.KeyPadDivide,
                KeyCode.VcNumPadMultiply => XKeySym.KeyPadMultiply,
                KeyCode.VcNumPadSubtract => XKeySym.KeyPadSubtract,
                KeyCode.VcNumPadEquals => XKeySym.KeyPadEqual,
                KeyCode.VcNumPadAdd => XKeySym.KeyPadAdd,
                KeyCode.VcNumPadEnter => XKeySym.KeyPadEnter,
                KeyCode.VcNumPadSeparator => XKeySym.KeyPadDecimal,

                KeyCode.VcNumPad0 => XKeySym.KeyPad0,
                KeyCode.VcNumPad1 => XKeySym.KeyPad1,
                KeyCode.VcNumPad2 => XKeySym.KeyPad2,
                KeyCode.VcNumPad3 => XKeySym.KeyPad3,
                KeyCode.VcNumPad4 => XKeySym.KeyPad4,
                KeyCode.VcNumPad5 => XKeySym.KeyPad5,
                KeyCode.VcNumPad6 => XKeySym.KeyPad6,
                KeyCode.VcNumPad7 => XKeySym.KeyPad7,
                KeyCode.VcNumPad8 => XKeySym.KeyPad8,
                KeyCode.VcNumPad9 => XKeySym.KeyPad9,

                KeyCode.VcNumPadEnd => XKeySym.KeyPadEnd,
                KeyCode.VcNumPadDown => XKeySym.KeyPadDown,
                KeyCode.VcNumPadPageDown => XKeySym.KeyPadPageDown,
                KeyCode.VcNumPadLeft => XKeySym.KeyPadLeft,
                KeyCode.VcNumPadRight => XKeySym.KeyPadRight,
                KeyCode.VcNumPadHome => XKeySym.KeyPadHome,
                KeyCode.VcNumPadUp => XKeySym.KeyPadUp,
                KeyCode.VcNumPadPageUp => XKeySym.KeyPadPageUp,
                KeyCode.VcNumPadInsert => XKeySym.KeyPadInsert,
                KeyCode.VcNumPadDelete => XKeySym.KeyPadDelete,

                KeyCode.VcLeftShift => XKeySym.LeftShift,
                KeyCode.VcRightShift => XKeySym.RightShift,
                KeyCode.VcLeftControl => XKeySym.LeftControl,
                KeyCode.VcRightControl => XKeySym.RightControl,
                KeyCode.VcLeftAlt => XKeySym.LeftAlt,
                KeyCode.VcRightAlt => XKeySym.RightAlt,
                KeyCode.VcLeftMeta => XKeySym.LeftSuper,
                KeyCode.VcRightMeta => XKeySym.RightSuper,
                KeyCode.VcContextMenu => XKeySym.Menu,

                _ => XKeySym.VoidSymbol
            };
    }
}
