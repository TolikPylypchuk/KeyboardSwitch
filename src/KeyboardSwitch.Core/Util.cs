using System.Diagnostics.CodeAnalysis;

namespace KeyboardSwitch.Core;

public static class Util
{
    [ExcludeFromCodeCoverage]
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

    public static string StripCommandLineArgument(string arg)
    {
        if (arg.StartsWith("--", StringComparison.InvariantCulture))
        {
            return arg[2..];
        }

        if (arg.StartsWith('-') || arg.StartsWith('/'))
        {
            return arg[1..];
        }

        return arg;
    }

    [ExcludeFromCodeCoverage]
    public static string GetConfigDirectory()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        return IsInMacOsBundle(currentDirectory)
            ? Path.Combine(Path.GetDirectoryName(currentDirectory) ?? String.Empty, "Resources")
            : currentDirectory;
    }

    [ExcludeFromCodeCoverage]
    private static bool IsInMacOsBundle(string directory) =>
        OperatingSystem.IsMacOS() &&
            directory.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries)
                .Any(dir => dir.EndsWith(".app"));
}
