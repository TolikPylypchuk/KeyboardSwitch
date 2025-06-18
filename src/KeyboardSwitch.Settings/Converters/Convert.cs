namespace KeyboardSwitch.Settings.Converters;

public static class Convert
{
    private static readonly Dictionary<EventMask, string> ModifiersToStrings =
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

    private static readonly Dictionary<string, EventMask> StringsToModifiers =
        ModifiersToStrings.ToDictionary(e => e.Value, e => e.Key);

    public static string ModifierToString(EventMask modifierKey) =>
        ModifiersToStrings[modifierKey];

    public static EventMask StringToModifier(string str) =>
        StringsToModifiers[str];

    public static string AppThemeToString(AppTheme theme) =>
        theme switch
        {
            AppTheme.Fluent => Messages.AppThemeFluent,
            AppTheme.MacOS => Messages.AppThemeMacOS,
            AppTheme.Simple => Messages.AppThemeSimple,
            _ => String.Empty
        };

    public static AppTheme StringToAppTheme(string theme) =>
        theme switch
        {
            var str when str == Messages.AppThemeMacOS => AppTheme.MacOS,
            var str when str == Messages.AppThemeSimple => AppTheme.Simple,
            _ => AppTheme.Fluent
        };

    public static string AppThemeVariantToString(AppThemeVariant variant) =>
        variant switch
        {
            AppThemeVariant.Auto => Messages.AppThemeVariantAuto,
            AppThemeVariant.Light => Messages.AppThemeVariantLight,
            AppThemeVariant.Dark => Messages.AppThemeVariantDark,
            _ => String.Empty
        };

    public static AppThemeVariant StringToAppThemeVariant(string theme) =>
        theme switch
        {
            var str when str == Messages.AppThemeVariantLight => AppThemeVariant.Light,
            var str when str == Messages.AppThemeVariantDark => AppThemeVariant.Dark,
            _ => AppThemeVariant.Auto
        };

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
