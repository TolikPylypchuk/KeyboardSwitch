namespace KeyboardSwitch.Core.Keyboard;

public static class Extensions
{
    public static EventMask? ToEventMask(this KeyCode keyCode) =>
        keyCode switch
        {
            KeyCode.VcLeftControl => EventMask.LeftCtrl,
            KeyCode.VcRightControl => EventMask.RightCtrl,
            KeyCode.VcLeftShift => EventMask.LeftShift,
            KeyCode.VcRightShift => EventMask.RightShift,
            KeyCode.VcLeftAlt => EventMask.LeftAlt,
            KeyCode.VcRightAlt => EventMask.RightAlt,
            KeyCode.VcLeftMeta => EventMask.LeftMeta,
            KeyCode.VcRightMeta => EventMask.RightMeta,
            _ => null
        };

    public static bool IsSubsetKeyOf(this EventMask subKey, EventMask superKey) =>
        superKey.Contains(subKey, EventMask.Ctrl) &&
        superKey.Contains(subKey, EventMask.Shift) &&
        superKey.Contains(subKey, EventMask.Alt) &&
        superKey.Contains(subKey, EventMask.Meta);

    private static bool Contains(this EventMask superKey, EventMask subKey, EventMask mask)
    {
        var subMask = subKey & mask;
        var superMask = superKey & mask;
        bool isAbsent = subMask == EventMask.None && superMask == EventMask.None;
        return isAbsent || subMask != EventMask.None && (subMask & superMask) == subMask;
    }
}
