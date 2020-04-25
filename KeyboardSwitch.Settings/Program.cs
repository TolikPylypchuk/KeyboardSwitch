using Avalonia;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;

namespace KeyboardSwitch.Settings
{
    public static class Program
    {
        public static void Main(string[] args)
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug()
                .UseReactiveUI()
                .StartWithClassicDesktopLifetime(args);
    }
}
