using Serilog;

using Constants = Serilog.Core.Constants;

namespace KeyboardSwitch.Settings;

public static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        try
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(AppContext.BaseDirectory) ?? String.Empty);

            return BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        } catch (Exception e)
        {
            Log.ForContext(Constants.SourceContextPropertyName, typeof(Program).FullName)
                .Fatal(e, "The settings app has crashed");

            return (int)ExitCode.Error;
        } finally
        {
            Log.CloseAndFlush();
        }
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
}
