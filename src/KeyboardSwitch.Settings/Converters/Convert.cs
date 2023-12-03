namespace KeyboardSwitch.Settings.Converters;

public static class Convert
{
    private static readonly IReadOnlyDictionary<ModifierMask, string> ModifiersToStrings =
        new List<ModifierMask>
        {
                ModifierMask.None,
                ModifierMask.LeftShift,
                ModifierMask.LeftCtrl,
                ModifierMask.LeftMeta,
                ModifierMask.LeftAlt,
                ModifierMask.RightShift,
                ModifierMask.RightCtrl,
                ModifierMask.RightMeta,
                ModifierMask.RightAlt,
                ModifierMask.Shift,
                ModifierMask.Ctrl,
                ModifierMask.Meta,
                ModifierMask.Alt,
        }.ToDictionary(modifier => modifier, ModifierMaskToString);

    private static readonly IReadOnlyDictionary<string, ModifierMask> StringsToModifiers =
        ModifiersToStrings.ToDictionary(e => e.Value, e => e.Key);

    public static string ModifierToString(ModifierMask modifierKey) =>
        ModifiersToStrings[modifierKey];

    public static ModifierMask StringToModifier(string str) =>
        StringsToModifiers[str];

    private static string ModifierMaskToString(ModifierMask modifier) =>
        modifier switch
        {
            ModifierMask.None => Messages.ModifierKeyNone,

            ModifierMask.LeftCtrl => Messages.ModifierKeyLeftCtrl,
            ModifierMask.RightCtrl => Messages.ModifierKeyRightCtrl,
            ModifierMask.Ctrl => Messages.ModifierKeyCtrl,

            ModifierMask.LeftShift => Messages.ModifierKeyLeftShift,
            ModifierMask.RightShift => Messages.ModifierKeyRightShift,
            ModifierMask.Shift => Messages.ModifierKeyShift,

            ModifierMask.LeftAlt => PlatformDependent(
                windows: () => Messages.ModifierKeyLeftAlt,
                macos: () => Messages.ModifierKeyLeftOption,
                linux: () => Messages.ModifierKeyLeftAlt),
            ModifierMask.RightAlt => PlatformDependent(
                windows: () => Messages.ModifierKeyRightAlt,
                macos: () => Messages.ModifierKeyRightOption,
                linux: () => Messages.ModifierKeyRightAlt),
            ModifierMask.Alt => PlatformDependent(
                windows: () => Messages.ModifierKeyAlt,
                macos: () => Messages.ModifierKeyOption,
                linux: () => Messages.ModifierKeyAlt),

            ModifierMask.LeftMeta => PlatformDependent(
                windows: () => Messages.ModifierKeyLeftWin,
                macos: () => Messages.ModifierKeyLeftCommand,
                linux: () => Messages.ModifierKeyLeftSuper),
            ModifierMask.RightMeta => PlatformDependent(
                windows: () => Messages.ModifierKeyRightWin,
                macos: () => Messages.ModifierKeyRightCommand,
                linux: () => Messages.ModifierKeyRightSuper),
            ModifierMask.Meta => PlatformDependent(
                windows: () => Messages.ModifierKeyWin,
                macos: () => Messages.ModifierKeyCommand,
                linux: () => Messages.ModifierKeySuper),

            _ => String.Empty
        };
}
