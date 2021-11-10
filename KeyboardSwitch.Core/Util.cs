namespace KeyboardSwitch.Core;

using System.Runtime.InteropServices;

public static class Util
{
    public static T PlatformDependent<T>(Func<T> windows, Func<T> macos, Func<T> linux)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return windows();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return macos();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return linux();
        }

        throw new PlatformNotSupportedException();
    }
}
