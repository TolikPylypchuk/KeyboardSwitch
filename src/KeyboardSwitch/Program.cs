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

using SharpHook.Data;
using SharpHook.Providers;

using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace KeyboardSwitch;

public static partial class Program
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
        Directory.SetCurrentDirectory(Path.GetDirectoryName(AppContext.BaseDirectory) ?? String.Empty);

        UioHookProvider.Instance.SetLinuxMode(LinuxMode.AutoLowLevel);

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

            LogExecutionStarted(logger);

            host.Start();
            mainLoopRunner.RunMainLoop(token: applicationLifetime.ApplicationStopping);
            host.WaitForShutdown();

            LogExecutionStopped(logger);
        } catch (Exception e) when (e is OperationCanceledException or TaskCanceledException)
        {
            LogExecutionCancelled(logger);
        } catch (Exception e)
        {
            LogServiceCrashed(logger, e);
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
            .AddSerilog(
                SerilogLoggerFactory.CreateLogger(context.Configuration, addLibUioHookLogging: true),
                dispose: true);

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
            .Do(_ => LogStoppingService(logger))
            .SubscribeAsync(async _ => await host.StopAsync());

        namedPipeService.ReceivedString
            .Where(command => command.IsCommand(ExternalCommand.ReloadSettings))
            .Do(_ => LogInvalidatingSettings(logger))
            .Subscribe(_ => settingsService.InvalidateAppSettings());

        namedPipeService.ReceivedString
            .Where(command => command.IsUnknownCommand())
            .Subscribe(command => LogExternalCommandNotRecognized(logger, command));
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

    [LoggerMessage(LogLevel.Information, "Keyboard Switch service execution started")]
    private static partial void LogExecutionStarted(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Keyboard Switch service execution stopped")]
    private static partial void LogExecutionStopped(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Keyboard Switch service execution cancelled")]
    private static partial void LogExecutionCancelled(ILogger logger);

    [LoggerMessage(LogLevel.Critical, "Keyboard Switch service has crashed")]
    private static partial void LogServiceCrashed(ILogger logger, Exception e);

    [LoggerMessage(LogLevel.Information, "Stopping the service by external request")]
    private static partial void LogStoppingService(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Invalidating the settings by external request")]
    private static partial void LogInvalidatingSettings(ILogger logger);

    [LoggerMessage(LogLevel.Warning, "External request '{Command}' is not recognized")]
    private static partial void LogExternalCommandNotRecognized(ILogger logger, string command);
}
