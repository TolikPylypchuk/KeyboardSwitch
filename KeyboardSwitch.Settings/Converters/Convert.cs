using System;
using System.Collections.Generic;
using System.Linq;

using KeyboardSwitch.Common.Keyboard;
using KeyboardSwitch.Settings.Properties;

using static KeyboardSwitch.Common.Util;

namespace KeyboardSwitch.Settings.Converters
{
    public static class Convert
    {
        private static readonly IReadOnlyDictionary<ModifierKey, string> KeysToStrings =
            Enum.GetValues<ModifierKey>()
                .Cast<ModifierKey>()
                .ToDictionary(key => key, KeyToString);
        
        private static readonly IReadOnlyDictionary<string, ModifierKey> StringsToKeys =
            KeysToStrings.ToDictionary(e => e.Value, e => e.Key);

        public static string ModifierKeyToString(ModifierKey modifierKey) =>
            KeysToStrings[modifierKey];

        public static ModifierKey StringToModifierKey(string str) =>
            StringsToKeys[str];

        private static string KeyToString(ModifierKey key) =>
            key switch
            {
                ModifierKey.None => Messages.ModifierKeyNone,

                ModifierKey.LeftCtrl => Messages.ModifierKeyLeftCtrl,
                ModifierKey.RightCtrl => Messages.ModifierKeyRightCtrl,
                ModifierKey.Ctrl => Messages.ModifierKeyCtrl,

                ModifierKey.LeftShift => Messages.ModifierKeyLeftShift,
                ModifierKey.RightShift => Messages.ModifierKeyRightShift,
                ModifierKey.Shift => Messages.ModifierKeyShift,

                ModifierKey.LeftAlt => PlatformDependent(
                    windows: () => Messages.ModifierKeyLeftAlt,
                    macos: () => Messages.ModifierKeyLeftOption,
                    linux: () => Messages.ModifierKeyLeftAlt),
                ModifierKey.RightAlt => PlatformDependent(
                    windows: () => Messages.ModifierKeyRightAlt,
                    macos: () => Messages.ModifierKeyRightOption,
                    linux: () => Messages.ModifierKeyRightAlt),
                ModifierKey.Alt => PlatformDependent(
                    windows: () => Messages.ModifierKeyAlt,
                    macos: () => Messages.ModifierKeyOption,
                    linux: () => Messages.ModifierKeyAlt),

                ModifierKey.LeftMeta => PlatformDependent(
                    windows: () => Messages.ModifierKeyLeftWin,
                    macos: () => Messages.ModifierKeyLeftCommand,
                    linux: () => Messages.ModifierKeyLeftSuper),
                ModifierKey.RightMeta => PlatformDependent(
                    windows: () => Messages.ModifierKeyRightWin,
                    macos: () => Messages.ModifierKeyRightCommand,
                    linux: () => Messages.ModifierKeyRightSuper),
                ModifierKey.Meta => PlatformDependent(
                    windows: () => Messages.ModifierKeyWin,
                    macos: () => Messages.ModifierKeyCommand,
                    linux: () => Messages.ModifierKeySuper),

                _ => String.Empty
            };
    }
}
