namespace KeyboardSwitch.Settings.Converters;

public static class Convert
{
    private static readonly IReadOnlyDictionary<EventMask, string> ModifiersToStrings =
        new List<EventMask>
        {
                EventMask.None,
                EventMask.LeftShift,
                EventMask.LeftCtrl,
                EventMask.LeftMeta,
                EventMask.LeftAlt,
                EventMask.RightShift,
                EventMask.RightCtrl,
                EventMask.RightMeta,
                EventMask.RightAlt,
                EventMask.Shift,
                EventMask.Ctrl,
                EventMask.Meta,
                EventMask.Alt,
        }.ToDictionary(modifier => modifier, EventMaskToString);

    private static readonly IReadOnlyDictionary<string, EventMask> StringsToModifiers =
        ModifiersToStrings.ToDictionary(e => e.Value, e => e.Key);

    public static string ModifierToString(EventMask modifierKey) =>
        ModifiersToStrings[modifierKey];

    public static EventMask StringToModifier(string str) =>
        StringsToModifiers[str];

    private static string EventMaskToString(EventMask modifier) =>
        modifier switch
        {
            EventMask.None => Messages.ModifierKeyNone,

            EventMask.LeftCtrl => Messages.ModifierKeyLeftCtrl,
            EventMask.RightCtrl => Messages.ModifierKeyRightCtrl,
            EventMask.Ctrl => Messages.ModifierKeyCtrl,

            EventMask.LeftShift => Messages.ModifierKeyLeftShift,
            EventMask.RightShift => Messages.ModifierKeyRightShift,
            EventMask.Shift => Messages.ModifierKeyShift,

            EventMask.LeftAlt => PlatformDependent(
                windows: () => Messages.ModifierKeyLeftAlt,
                macos: () => Messages.ModifierKeyLeftOption,
                linux: () => Messages.ModifierKeyLeftAlt),
            EventMask.RightAlt => PlatformDependent(
                windows: () => Messages.ModifierKeyRightAlt,
                macos: () => Messages.ModifierKeyRightOption,
                linux: () => Messages.ModifierKeyRightAlt),
            EventMask.Alt => PlatformDependent(
                windows: () => Messages.ModifierKeyAlt,
                macos: () => Messages.ModifierKeyOption,
                linux: () => Messages.ModifierKeyAlt),

            EventMask.LeftMeta => PlatformDependent(
                windows: () => Messages.ModifierKeyLeftWin,
                macos: () => Messages.ModifierKeyLeftCommand,
                linux: () => Messages.ModifierKeyLeftSuper),
            EventMask.RightMeta => PlatformDependent(
                windows: () => Messages.ModifierKeyRightWin,
                macos: () => Messages.ModifierKeyRightCommand,
                linux: () => Messages.ModifierKeyRightSuper),
            EventMask.Meta => PlatformDependent(
                windows: () => Messages.ModifierKeyWin,
                macos: () => Messages.ModifierKeyCommand,
                linux: () => Messages.ModifierKeySuper),

            _ => String.Empty
        };
}
