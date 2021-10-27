using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia.Input;

using ReactiveUI;

using SharpHook.Native;

using static KeyboardSwitch.Core.Util;

namespace KeyboardSwitch.Settings.Converters
{
    public sealed class KeyCodeConverter : IBindingTypeConverter
    {
        private static readonly string Alt =
            PlatformDependent(windows: () => "Alt", macos: () => "Option", linux: () => "Alt");

        private static readonly string Meta =
            PlatformDependent(windows: () => "Win", macos: () => "Command", linux: () => "Super");

        private static readonly List<(KeyCode KeyCode, Key Key, string Name)> mappings = new()
        {
            (KeyCode.VcEscape, Key.Escape, "Esc"),

            (KeyCode.VcF1, Key.F1, "F1"),
            (KeyCode.VcF2, Key.F2, "F2"),
            (KeyCode.VcF3, Key.F3, "F3"),
            (KeyCode.VcF4, Key.F4, "F4"),
            (KeyCode.VcF5, Key.F5, "F5"),
            (KeyCode.VcF6, Key.F6, "F6"),
            (KeyCode.VcF7, Key.F7, "F7"),
            (KeyCode.VcF8, Key.F8, "F8"),
            (KeyCode.VcF9, Key.F9, "F9"),
            (KeyCode.VcF10, Key.F10, "F10"),
            (KeyCode.VcF11, Key.F11, "F11"),
            (KeyCode.VcF12, Key.F12, "F12"),
            (KeyCode.VcF13, Key.F13, "F13"),
            (KeyCode.VcF14, Key.F14, "F14"),
            (KeyCode.VcF15, Key.F15, "F15"),
            (KeyCode.VcF16, Key.F16, "F16"),
            (KeyCode.VcF17, Key.F17, "F17"),
            (KeyCode.VcF18, Key.F18, "F18"),
            (KeyCode.VcF19, Key.F19, "F19"),
            (KeyCode.VcF20, Key.F20, "F20"),
            (KeyCode.VcF21, Key.F21, "F21"),
            (KeyCode.VcF22, Key.F22, "F22"),
            (KeyCode.VcF23, Key.F23, "F23"),
            (KeyCode.VcF24, Key.F24, "F24"),

            (KeyCode.VcBackquote, Key.OemTilde, "`"),

            (KeyCode.Vc1, Key.D1, "1"),
            (KeyCode.Vc2, Key.D2, "2"),
            (KeyCode.Vc3, Key.D3, "3"),
            (KeyCode.Vc4, Key.D4, "4"),
            (KeyCode.Vc5, Key.D5, "5"),
            (KeyCode.Vc6, Key.D6, "6"),
            (KeyCode.Vc7, Key.D7, "7"),
            (KeyCode.Vc8, Key.D8, "8"),
            (KeyCode.Vc9, Key.D9, "9"),
            (KeyCode.Vc0, Key.D0, "0"),

            (KeyCode.VcMinus, Key.OemMinus, "-"),
            (KeyCode.VcEquals, Key.OemPlus, "="),
            (KeyCode.VcBackspace, Key.Back, "Backspace"),

            (KeyCode.VcTab, Key.Tab, "Tab"),
            (KeyCode.VcCapsLock, Key.CapsLock, "Caps Lock"),

            (KeyCode.VcA, Key.A, "A"),
            (KeyCode.VcB, Key.B, "B"),
            (KeyCode.VcC, Key.C, "C"),
            (KeyCode.VcD, Key.D, "D"),
            (KeyCode.VcE, Key.E, "E"),
            (KeyCode.VcF, Key.F, "F"),
            (KeyCode.VcG, Key.G, "G"),
            (KeyCode.VcH, Key.H, "H"),
            (KeyCode.VcI, Key.I, "I"),
            (KeyCode.VcJ, Key.J, "J"),
            (KeyCode.VcK, Key.K, "K"),
            (KeyCode.VcL, Key.L, "L"),
            (KeyCode.VcM, Key.M, "M"),
            (KeyCode.VcN, Key.N, "N"),
            (KeyCode.VcO, Key.O, "O"),
            (KeyCode.VcP, Key.P, "P"),
            (KeyCode.VcQ, Key.Q, "Q"),
            (KeyCode.VcR, Key.R, "R"),
            (KeyCode.VcS, Key.S, "S"),
            (KeyCode.VcT, Key.T, "T"),
            (KeyCode.VcU, Key.U, "U"),
            (KeyCode.VcV, Key.V, "V"),
            (KeyCode.VcW, Key.W, "W"),
            (KeyCode.VcX, Key.X, "X"),
            (KeyCode.VcY, Key.Y, "Y"),
            (KeyCode.VcZ, Key.Z, "Z"),

            (KeyCode.VcOpenBracket, Key.OemOpenBrackets, "["),
            (KeyCode.VcCloseBracket, Key.OemCloseBrackets, "]"),
            (KeyCode.VcBackSlash, Key.OemPipe, "\\"),

            (KeyCode.VcSemicolon, Key.OemSemicolon, ";"),
            (KeyCode.VcQuote, Key.OemQuotes, "'"),
            (KeyCode.VcEnter, Key.Enter, "Enter"),

            (KeyCode.VcComma, Key.OemComma, ","),
            (KeyCode.VcPeriod, Key.OemPeriod, "."),
            (KeyCode.VcSlash, Key.OemQuestion, "/"),

            (KeyCode.VcSpace, Key.Space, "Space"),

            (KeyCode.VcPrintscreen, Key.PrintScreen, "Print Screen"),
            (KeyCode.VcScrollLock, Key.Scroll, "Scroll Lock"),
            (KeyCode.VcPause, Key.Pause, "Pause"),

            (KeyCode.VcInsert, Key.Insert, "Insert"),
            (KeyCode.VcDelete, Key.Delete, "Delete"),
            (KeyCode.VcHome, Key.Home, "Home"),
            (KeyCode.VcEnd, Key.End, "End"),
            (KeyCode.VcPageUp, Key.PageUp, "Page Up"),
            (KeyCode.VcPageDown, Key.PageDown, "Page Down"),

            (KeyCode.VcUp, Key.Up, "Up"),
            (KeyCode.VcLeft, Key.Left, "Left"),
            (KeyCode.VcClear, Key.Clear, "Clear"),
            (KeyCode.VcRight, Key.Right, "Right"),
            (KeyCode.VcDown, Key.Down, "Down"),

            (KeyCode.VcNumLock, Key.NumLock, "Num Lock"),
            (KeyCode.VcNumPadDivide, Key.Divide, "/"),
            (KeyCode.VcNumPadMultiply, Key.Multiply, "*"),
            (KeyCode.VcNumPadSubtract, Key.Subtract, "-"),
            (KeyCode.VcNumPadAdd, Key.Add, "+"),
            (KeyCode.VcNumPadSeparator, Key.Decimal, "."),

            (KeyCode.VcNumPad1, Key.NumPad1, "1"),
            (KeyCode.VcNumPad2, Key.NumPad2, "2"),
            (KeyCode.VcNumPad3, Key.NumPad3, "3"),
            (KeyCode.VcNumPad4, Key.NumPad4, "4"),
            (KeyCode.VcNumPad5, Key.NumPad5, "5"),
            (KeyCode.VcNumPad6, Key.NumPad6, "6"),
            (KeyCode.VcNumPad7, Key.NumPad7, "7"),
            (KeyCode.VcNumPad8, Key.NumPad8, "8"),
            (KeyCode.VcNumPad9, Key.NumPad9, "9"),
            (KeyCode.VcNumPad0, Key.NumPad0, "0"),

            (KeyCode.VcLeftShift, Key.LeftShift, "Left Shift"),
            (KeyCode.VcRightShift, Key.RightShift, "Right Shift"),
            (KeyCode.VcLeftControl, Key.LeftCtrl, "Left Ctrl"),
            (KeyCode.VcRightControl, Key.RightCtrl, "Right Ctrl"),
            (KeyCode.VcLeftAlt, Key.LeftAlt, $"Left {Alt}"),
            (KeyCode.VcRightAlt, Key.RightAlt, $"Right {Alt}"),
            (KeyCode.VcLeftMeta, Key.LWin, $"Left {Meta}"),
            (KeyCode.VcRightMeta, Key.RWin, $"Right {Meta}"),

            (KeyCode.VcMediaPlay, Key.MediaPlayPause, "Play"),
            (KeyCode.VcMediaStop, Key.MediaStop, "Stop"),
            (KeyCode.VcMediaPrevious, Key.MediaPreviousTrack, "Previous"),
            (KeyCode.VcMediaNext, Key.MediaNextTrack, "Next"),
            (KeyCode.VcMediaSelect, Key.SelectMedia, "Select"),

            (KeyCode.VcVolumeMute, Key.VolumeMute, "Volume Mute"),
            (KeyCode.VcVolumeUp, Key.VolumeUp, "Volume Up"),
            (KeyCode.VcVolumeDown, Key.VolumeDown, "Volume Down"),
        };

        private static readonly Dictionary<KeyCode, Key> codesToKeys =
            mappings.ToDictionary(e => e.KeyCode, e => e.Key);

        private static readonly Dictionary<Key, KeyCode> keysToCodes =
            mappings.ToDictionary(e => e.Key, e => e.KeyCode);

        private static readonly Dictionary<KeyCode, string> codeNames =
            mappings.ToDictionary(e => e.KeyCode, e => e.Name);

        public Key? ConvertToKey(KeyCode keyCode) =>
            codesToKeys.TryGetValue(keyCode, out var key) ? key : null;

        public KeyCode? ConvertToKeyCode(Key key) =>
            keysToCodes.TryGetValue(key, out var keyCode) ? keyCode : null;

        public string? GetName(KeyCode keyCode) =>
            codeNames.TryGetValue(keyCode, out string? name) ? name : null;

        int IBindingTypeConverter.GetAffinityForObjects(Type fromType, Type toType) =>
            fromType == typeof(KeyCode) && toType == typeof(Key) ||
            fromType == typeof(Key) && toType == typeof(KeyCode) ||
            fromType == typeof(KeyCode) && toType == typeof(string)
                ? 10000
                : 0;

        bool IBindingTypeConverter.TryConvert(object? from, Type toType, object? conversionHint, out object? result)
        {
            bool converted;

            switch (from)
            {
                case KeyCode keyCodeFrom when toType == typeof(Key):
                    converted = codesToKeys.TryGetValue(keyCodeFrom, out var key);
                    result = key;
                    return converted;
                case KeyCode keyCodeFrom when toType == typeof(string):
                    converted = codeNames.TryGetValue(keyCodeFrom, out string? name);
                    result = name;
                    return converted;
                case Key keyFrom:
                    converted = keysToCodes.TryGetValue(keyFrom, out var keyCode);
                    result = keyCode;
                    return converted;
                default:
                    result = null;
                    return false;
            }
        }
    }
}
