#if LINUX
using KeyboardSwitch.Linux;
#endif

using ReactiveUI.Avalonia.Splat;

using Serilog;

using SharpHook.Providers;

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

            UioHookProvider.Instance.SetLinuxMode(LinuxMode.AutoLowLevel);

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

    public static AppBuilder BuildAvaloniaApp()
    {
        var builder = AppBuilder.Configure<App>().UsePlatformDetect();

#if LINUX
        if (LinuxSessionDetector.IsRunningOnWayland)
        {
            builder = builder.UseWayland();
        }
#endif

        return builder.LogToTrace()
            .UseReactiveUIWithMicrosoftDependencyResolver(
                services => services.ConfigureServices(),
                sp => { },
                reactiveUI => reactiveUI.WithSuspensionHost<AppState>());
    }
}
