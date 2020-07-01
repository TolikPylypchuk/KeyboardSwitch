using System.IO;
using System.Reflection;

using Avalonia;
using Avalonia.Logging.Serilog;

namespace KeyboardSwitch.Settings
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug()
                .StartWithClassicDesktopLifetime(args);
        }
    }
}
