using System.Collections.Generic;
using System.Linq;

namespace KeyboardSwitch.Core.Keyboard
{
    public static class Extensions
    {
        public static ModifierKey? ToModifierKey(this KeyCode keyCode) =>
            keyCode switch
            {
                KeyCode.VcLeftControl => ModifierKey.LeftCtrl,
                KeyCode.VcRightControl => ModifierKey.RightCtrl,
                KeyCode.VcLeftShift => ModifierKey.LeftShift,
                KeyCode.VcRightShift => ModifierKey.RightShift,
                KeyCode.VcLeftAlt => ModifierKey.LeftAlt,
                KeyCode.VcRightAlt => ModifierKey.RightAlt,
                KeyCode.VcLeftMeta => ModifierKey.LeftMeta,
                KeyCode.VcRightMeta => ModifierKey.RightMeta,
                _ => null
            };

        public static ModifierKey Flatten(this IEnumerable<ModifierKey> keys) =>
            keys.Aggregate(ModifierKey.None, (acc, key) => acc | key);

        public static bool IsSubsetKeyOf(this ModifierKey subKey, ModifierKey superKey) =>
            superKey.Contains(subKey, ModifierKey.Ctrl) &&
            superKey.Contains(subKey, ModifierKey.Shift) &&
            superKey.Contains(subKey, ModifierKey.Alt) &&
            superKey.Contains(subKey, ModifierKey.Meta);

        private static bool Contains(this ModifierKey superKey, ModifierKey subKey, ModifierKey mask)
        {
            var subMask = subKey & mask;
            var superMask = superKey & mask;
            bool isAbsent = subMask == ModifierKey.None && superMask == ModifierKey.None;
            return isAbsent || subMask != ModifierKey.None && (subMask & superMask) == subMask;
        }
    }
}
