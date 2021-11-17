namespace KeyboardSwitch;

using System.Reactive.Concurrency;
using System.Reflection;

using Akavache;

#if WINDOWS
using KeyboardSwitch.Windows;
#else
using KeyboardSwitch.Linux;
#endif

using Serilog;

using ILogger = ILogger; // From Microsoft.Extensions.Logging

public static class Program
{
    public static void Main(string[] args)
    {
        Directory.SetCurrentDirectory(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location) ?? String.Empty);

        BlobCache.ApplicationName = nameof(KeyboardSwitch);

        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(ConfigureServices)
            .ConfigureLogging(ConfigureLogging)
            .UseConsoleLifetime()
            .UseEnvironment(PlatformDependent(windows: () => "windows", macos: () => "macos", linux: () => "linux"))
            .UseSystemd()
            .Build();

        var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(Program));

        using var mutex = ConfigureSingleInstance(host.Services);

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
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services) =>
        services.AddHostedService<Worker>()
            .Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromMilliseconds(100))
            .Configure<GlobalSettings>(context.Configuration.GetSection("Settings"))
            .AddSingleton<IScheduler>(Scheduler.Default)
            .AddCoreKeyboardSwitchServices()
            .AddNativeKeyboardSwitchServices(context.Configuration);

    private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder logging) =>
        logging
            .ClearProviders()
            .AddSerilog(
                new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .ReadFrom.Configuration(context.Configuration)
                    .CreateLogger(),
                dispose: true);

    private static Mutex ConfigureSingleInstance(IServiceProvider services)
    {
        var singleInstanceProvider = services.GetRequiredService<ServiceProvider<ISingleInstanceService>>();
        var singleInstanceService = singleInstanceProvider(nameof(KeyboardSwitch));

        return singleInstanceService.TryAcquireMutex();
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
}
