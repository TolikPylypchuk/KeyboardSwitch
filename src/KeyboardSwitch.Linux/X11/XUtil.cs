namespace KeyboardSwitch.Linux.X11;

internal static class XUtil
{
    public static XDisplayHandle OpenXDisplay()
    {
        XkbIgnoreExtension(false);

        int major = XkbMajorVersion;
        int minor = XkbMinorVersion;

        var display = XkbOpenDisplay(String.Empty, out _, out _, ref major, ref minor, out var result);
        ValidateXOpenDisplayResult(result);

        return display;
    }

    public static (byte Min, byte Max) GetMinAndMaxKeyCodes(XDisplayHandle display)
    {
        using var keyboardDescHandle = new XMapHandle(
            XkbGetMap(display, XkbMapComponentMask.XkbAllClientInfoMask, XkbKeyboardSpec.XkbUseCoreKbd),
            XkbMapComponentMask.XkbAllClientInfoMask);

        var keyboardDesc = Marshal.PtrToStructure<XkbDesc>(keyboardDescHandle.DangerousGetHandle());

        return (keyboardDesc.MinKeyCode, keyboardDesc.MaxKeyCode);
    }

    private static void ValidateXOpenDisplayResult(XOpenDisplayResult result)
    {
        switch (result)
        {
            case XOpenDisplayResult.BadLibraryVersion:
                throw new XException("Bad X11 version");
            case XOpenDisplayResult.ConnectionRefused:
                throw new XException("Connection to X server refused");
            case XOpenDisplayResult.NonXkbServer:
                throw new XException("XKB not present");
            case XOpenDisplayResult.BadServerVersion:
                throw new XException("Bad X11 server version");
        }
    }
}
