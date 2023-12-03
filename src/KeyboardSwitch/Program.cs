namespace KeyboardSwitch;

using System.Diagnostics;
using System.Globalization;
using System.Reactive.Concurrency;

using Akavache;

#if WINDOWS
using KeyboardSwitch.Windows;
#elif MACOS
using KeyboardSwitch.MacOS;
#elif LINUX
using KeyboardSwitch.Linux;
#endif

using Serilog;
using Serilog.Settings.Configuration;

using ILogger = ILogger; // From Microsoft.Extensions.Logging

public static class Program
{
    private static readonly ExitCodeAccessor exitCodeAccessor = new();

    public static int Main(string[] args)
    {
        var command = ParseCommand(args);

        switch (command)
        {
            case Command.Run:
            case Command.Stop:
            case Command.ReloadSettings:
                Run(args);
                break;
            case Command.CheckIfRunning:
                ShowIfRunning();
                break;
            case Command.ShowHelp:
                Help.Show(Console.Error);
                break;
            case Command.None:
                Help.Show(Console.Error);
                exitCodeAccessor.AppExitCode = ExitCode.UnknownCommand;
                break;
        }

        return (int)exitCodeAccessor.AppExitCode;
    }

    private static void Run(string[] args)
    {
        Directory.SetCurrentDirectory(
            Path.GetDirectoryName(AppContext.BaseDirectory) ?? String.Empty);

        BlobCache.ApplicationName = nameof(KeyboardSwitch);

        using var host = Host.CreateDefaultBuilder(args)
            .UseContentRoot(GetConfigDirectory())
            .ConfigureServices(ConfigureServices)
            .ConfigureLogging(ConfigureLogging)
            .UseConsoleLifetime()
            .UseEnvironment(PlatformDependent(windows: () => "windows", macos: () => "macos", linux: () => "linux"))
            .Build();

        using var mutex = ConfigureSingleInstance(host.Services);

        if (args.Length == 0)
        {
            var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(Program));

            if (!SettingsExist(host))
            {
                logger.LogError("The settings file does not exist - open Keyboard Switch Settings to create them");
                exitCodeAccessor.AppExitCode = ExitCode.SettingsDoNotExist;
                return;
            }

            try
            {
                SubscribeToExternalCommands(host, logger);

                logger.LogInformation("KeyboardSwitch service execution started");

                host.Run();

                logger.LogInformation("KeyboardSwitch service execution stopped");
            } catch (Exception e) when (e is OperationCanceledException || e is TaskCanceledException)
            {
                logger.LogInformation("KeyboardSwitch service execution cancelled");
            } finally
            {
                mutex.ReleaseMutex();
            }
        } else
        {
            exitCodeAccessor.AppExitCode = ExitCode.KeyboardSwitchNotRunning;
        }
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services) =>
        services.AddHostedService<Worker>()
            .Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromMilliseconds(100))
            .Configure<GlobalSettings>(context.Configuration.GetSection("Settings"))
            .AddSingleton<IScheduler>(Scheduler.Default)
            .AddCoreKeyboardSwitchServices()
            .AddNativeKeyboardSwitchServices(context.Configuration)
            .AddSingleton<IExitCodeSetter>(exitCodeAccessor);

    private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder logging)
    {
        var configAssemblies = new[]
        {
            typeof(LoggerConfigurationAsyncExtensions).Assembly,
            typeof(ConsoleLoggerConfigurationExtensions).Assembly,
            typeof(FileLoggerConfigurationExtensions).Assembly,
            typeof(LoggerSinkConfigurationDebugExtensions).Assembly
        };

        logging
            .ClearProviders()
            .AddSerilog(
                new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .ReadFrom.Configuration(context.Configuration, new ConfigurationReaderOptions(configAssemblies))
                    .CreateLogger(),
                dispose: true);
    }

    private static Mutex ConfigureSingleInstance(IServiceProvider services)
    {
        var singleInstanceProvider = services.GetRequiredService<ServiceProvider<ISingleInstanceService>>();
        var singleInstanceService = singleInstanceProvider(nameof(KeyboardSwitch));

        return singleInstanceService.TryAcquireMutex();
    }

    private static bool SettingsExist(IHost host)
    {
        var globalSettings = host.Services.GetRequiredService<IOptions<GlobalSettings>>();
        var settingsPath = Environment.ExpandEnvironmentVariables(globalSettings.Value.SettingsFilePath);

        return File.Exists(settingsPath);
    }

    private static void SubscribeToExternalCommands(IHost host, ILogger logger)
    {
        var namedPipeProvider = host.Services.GetRequiredService<ServiceProvider<INamedPipeService>>();
        var namedPipeService = namedPipeProvider(nameof(KeyboardSwitch));

        var settingsService = host.Services.GetRequiredService<IAppSettingsService>();

        namedPipeService.StartServer();

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

    private static void ShowIfRunning()
    {
        var processes = Process.GetProcessesByName(nameof(KeyboardSwitch));
        bool isRunning = processes is not null && processes.Length > 1;

        Console.WriteLine(isRunning ? "KeyboardSwitch is running" : "KeyboardSwitch is not running");

        if (!isRunning)
        {
            exitCodeAccessor.AppExitCode = ExitCode.KeyboardSwitchNotRunning;
        }
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
