namespace KeyboardSwitch.Settings;

using System.Reflection;

public static class Program
{
    public static int Main(string[] args)
    {
        Directory.SetCurrentDirectory(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location) ?? String.Empty);

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .StartWithClassicDesktopLifetime(args);
    }
}
