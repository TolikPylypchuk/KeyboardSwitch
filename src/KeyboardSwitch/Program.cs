using System.Diagnostics;
using System.Globalization;
using System.Reactive.Concurrency;

using KeyboardSwitch.Core.Logging;

#if WINDOWS
using KeyboardSwitch.Windows;
#elif MACOS
using KeyboardSwitch.MacOS;
#elif LINUX
using KeyboardSwitch.Linux;
#endif

using Serilog;

using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace KeyboardSwitch;

public static class Program
{
    public static int Main(string[] args) =>
        (int)(ParseCommand(args) switch
        {
            Command.Run or Command.Stop or Command.ReloadSettings => Run(args),
            Command.CheckIfRunning => ShowIfRunning(),
            Command.ShowHelp => Help.Show(Console.Error, ExitCode.Success),
            _ => Help.Show(Console.Error, ExitCode.UnknownCommand)
        });

    private static ExitCode Run(string[] args)
    {
        Directory.SetCurrentDirectory(
            Path.GetDirectoryName(AppContext.BaseDirectory) ?? String.Empty);

        using var host = Host.CreateDefaultBuilder(args)
            .UseContentRoot(GetConfigDirectory())
            .ConfigureServices(ConfigureServices)
            .ConfigureLogging(ConfigureLogging)
            .UseConsoleLifetime()
            .UseEnvironment(PlatformDependent(windows: () => "windows", macos: () => "macos", linux: () => "linux"))
            .Build();

        var exitService = host.Services.GetRequiredService<IExitService>();
        var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(Program));

        using var mutex = ConfigureSingleInstance(host.Services);

        try
        {
            if (args.Length != 0)
            {
                return ExitCode.KeyboardSwitchNotRunning;
            }

            SubscribeToExternalCommands(host, logger);

            var mainLoopRunner = host.Services.GetRequiredService<IMainLoopRunner>();
            var applicationLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

            logger.LogInformation("Keyboard Switch service execution started");

            host.Start();
            mainLoopRunner.RunMainLoop(token: applicationLifetime.ApplicationStopping);
            host.WaitForShutdown();

            logger.LogInformation("Keyboard Switch service execution stopped");
        } catch (Exception e) when (e is OperationCanceledException or TaskCanceledException)
        {
            logger.LogInformation("Keyboard Switch service execution cancelled");
        } catch (Exception e)
        {
            logger.LogCritical(e, "Keyboard Switch service has crashed");
        } finally
        {
            mutex.ReleaseMutex();
        }

        return exitService.ExitCode;
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services) =>
        services.AddHostedService<Worker>()
            .Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromMilliseconds(100))
            .Configure<GlobalSettings>(context.Configuration.GetSection("Settings"))
            .AddSingleton<IScheduler>(Scheduler.Default)
            .AddCoreKeyboardSwitchServices()
            .AddNativeKeyboardSwitchServices(context.Configuration)
            .AddSingleton<IExitService, ExitService>();

    private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder logging) =>
        logging
            .ClearProviders()
            .AddSerilog(SerilogLoggerFactory.CreateLogger(context.Configuration), dispose: true);

    private static Mutex ConfigureSingleInstance(IServiceProvider services) =>
        services
            .GetRequiredService<ISingleInstanceService>()
            .TryAcquireMutex(nameof(KeyboardSwitch));

    private static void SubscribeToExternalCommands(IHost host, ILogger logger)
    {
        var namedPipeService = host.Services.GetRequiredService<INamedPipeService>();
        var settingsService = host.Services.GetRequiredService<IAppSettingsService>();
        var layoutService = host.Services.GetRequiredService<ILayoutService>();

        settingsService.SettingsInvalidated
            .Subscribe(layoutService.SettingsInvalidated);

        namedPipeService.StartServer(nameof(KeyboardSwitch));

        namedPipeService.ReceivedString
            .Where(command => command.IsCommand(ExternalCommand.Stop))
            .Do(_ => logger.LogInformation("Stopping the service by external request"))
            .SubscribeAsync(async _ => await host.StopAsync());

        namedPipeService.ReceivedString
            .Where(command => command.IsCommand(ExternalCommand.ReloadSettings))
            .Do(_ => logger.LogInformation("Invalidating the settings by external request"))
            .Subscribe(_ => settingsService.InvalidateAppSettings());

        namedPipeService.ReceivedString
            .Where(command => command.IsUnknownCommand())
            .Subscribe(command => logger.LogWarning("External request '{Command}' is not recognized", command));
    }

    private static ExitCode ShowIfRunning()
    {
        var processes = Process.GetProcessesByName(nameof(KeyboardSwitch));
        bool isRunning = processes.Length > 1;

        Console.WriteLine(isRunning ? "KeyboardSwitch is running" : "KeyboardSwitch is not running");

        return isRunning ? ExitCode.Success : ExitCode.KeyboardSwitchNotRunning;
    }

    private static Command ParseCommand(string[] args)
    {
        if (args.Length == 0)
        {
            return Command.Run;
        } else if (args.Length > 1)
        {
            return Command.None;
        }

        return StripCommandLineArgument(args[0]).ToLower(CultureInfo.InvariantCulture) switch
        {
            "stop" => Command.Stop,
            "reload-settings" => Command.ReloadSettings,
            "check" => Command.CheckIfRunning,
            "help" or "?" => Command.ShowHelp,
            _ => Command.None
        };
    }
}
