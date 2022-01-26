namespace KeyboardSwitch.MacOS.Native;

internal static class NativeUtils
{
    public const string KeyboardLayoutPrefix = "com.apple.keylayout.";

    public static IntPtr GetExportedConstant(string libName, string constant)
    {
        if (NativeLibrary.TryLoad(libName, out var lib))
        {
            using var handle = new NativeLibraryHandle(lib);

            var result = NativeLibrary.GetExport(lib, constant);
            return Marshal.ReadIntPtr(result);
        }

        return IntPtr.Zero;
    }

    public static bool IsKeyboardInputSource(TISInputSourceRef source) =>
        GetInputSourceName(source).StartsWith(KeyboardLayoutPrefix);

    public static string GetInputSourceName(TISInputSourceRef source)
    {
        using var name = new CFStringRef(HIToolbox.TISGetInputSourceProperty(
            source, HIToolbox.TISPropertyInputSourceID));

        return GetStringValue(name);
    }

    public static string GetInputSourceLocalizedName(TISInputSourceRef source)
    {
        using var localizedName = new CFStringRef(HIToolbox.TISGetInputSourceProperty(
            source, HIToolbox.TISPropertyLocalizedName));

        return GetStringValue(localizedName);
    }

    public static string GetStringValue(CFStringRef @ref)
    {
        if (@ref.IsInvalid)
        {
            return String.Empty;
        }

        long length = CoreFoundation.CFStringGetLength(@ref);
        long maxSize = CoreFoundation.CFStringGetMaximumSizeForEncoding(length, CFStringEncoding.UTF8);

        var buffer = new byte[(int)maxSize + 1];

        bool success = CoreFoundation.CFStringGetCString(@ref, buffer, maxSize, CFStringEncoding.UTF8);

        return success ? Encoding.UTF8.GetString(buffer).Trim('\0') : String.Empty;
    }
}
