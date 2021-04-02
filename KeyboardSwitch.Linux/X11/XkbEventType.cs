namespace KeyboardSwitch.Linux.X11
{
    internal enum XkbEventType : uint
    {
        XkbNewKeyboardNotify = 0,
        XkbMapNotify = 1,
        XkbStateNotify = 2,
        XkbControlsNotify = 3,
        XkbIndicatorStateNotify = 4,
        XkbIndicatorMapNotify = 5,
        XkbNamesNotify = 6,
        XkbCompatMapNotify = 7,
        XkbBellNotify = 8,
        XkbActionMessage = 9,
        XkbAccessXNotify = 10,
        XkbExtensionDeviceNotify = 11
    }
}
