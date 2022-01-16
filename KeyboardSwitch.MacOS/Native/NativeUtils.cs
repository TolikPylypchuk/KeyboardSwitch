namespace KeyboardSwitch.MacOS.Native;

using System.Text;

internal static class NativeUtils
{
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
