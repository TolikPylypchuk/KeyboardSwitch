namespace KeyboardSwitch.Core;

public static class Util
{
    public static T PlatformDependent<T>(Func<T> windows, Func<T> macos, Func<T> linux)
    {
        if (OperatingSystem.IsWindows())
        {
            return windows();
        }

        if (OperatingSystem.IsMacOS())
        {
            return macos();
        }

        if (OperatingSystem.IsLinux())
        {
            return linux();
        }

        throw new PlatformNotSupportedException();
    }
}
