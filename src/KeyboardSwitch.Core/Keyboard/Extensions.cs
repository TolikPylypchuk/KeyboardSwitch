namespace KeyboardSwitch.Core.Keyboard;

public static class Extensions
{
    public static ModifierMask? ToModifierMask(this KeyCode keyCode) =>
        keyCode switch
        {
            KeyCode.VcLeftControl => ModifierMask.LeftCtrl,
            KeyCode.VcRightControl => ModifierMask.RightCtrl,
            KeyCode.VcLeftShift => ModifierMask.LeftShift,
            KeyCode.VcRightShift => ModifierMask.RightShift,
            KeyCode.VcLeftAlt => ModifierMask.LeftAlt,
            KeyCode.VcRightAlt => ModifierMask.RightAlt,
            KeyCode.VcLeftMeta => ModifierMask.LeftMeta,
            KeyCode.VcRightMeta => ModifierMask.RightMeta,
            _ => null
        };

    public static ModifierMask Flatten(this IEnumerable<ModifierMask> keys) =>
        keys.Aggregate(ModifierMask.None, (acc, key) => acc | key);

    public static bool IsSubsetKeyOf(this ModifierMask subKey, ModifierMask superKey) =>
        superKey.Contains(subKey, ModifierMask.Ctrl) &&
        superKey.Contains(subKey, ModifierMask.Shift) &&
        superKey.Contains(subKey, ModifierMask.Alt) &&
        superKey.Contains(subKey, ModifierMask.Meta);

    private static bool Contains(this ModifierMask superKey, ModifierMask subKey, ModifierMask mask)
    {
        var subMask = subKey & mask;
        var superMask = superKey & mask;
        bool isAbsent = subMask == ModifierMask.None && superMask == ModifierMask.None;
        return isAbsent || subMask != ModifierMask.None && (subMask & superMask) == subMask;
    }
}
