using Avalonia;
using Avalonia.Logging.Serilog;

namespace KeyboardSwitch.Settings
{
    public static class Program
    {
        public static int Main(string[] args)
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug()
                .StartWithClassicDesktopLifetime(args);
    }
}
