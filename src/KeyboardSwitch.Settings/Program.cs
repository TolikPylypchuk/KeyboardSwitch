#if LINUX
using KeyboardSwitch.Linux;
#endif

using ReactiveUI.Avalonia.Reactive.Splat;

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
        const string appId = "keyboard-switch-settings";

        builder = SessionDetector.IsRunningOnWayland
            ? builder.UseWayland().With(new WaylandPlatformOptions { AppId = appId })
            : builder.With(new X11PlatformOptions { WmClass = appId });
#endif

        return builder.LogToTrace()
            .UseReactiveUIWithMicrosoftDependencyResolver(
                services => services.ConfigureServices(),
                sp => { },
                reactiveUI => reactiveUI.WithSuspensionHost<AppState>());
    }
}
