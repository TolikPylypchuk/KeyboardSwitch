namespace KeyboardSwitch.Settings;

public static class Program
{
    public static int Main(string[] args)
    {
        Directory.SetCurrentDirectory(
            Path.GetDirectoryName(AppContext.BaseDirectory) ?? String.Empty);

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .StartWithClassicDesktopLifetime(args);
    }
}
