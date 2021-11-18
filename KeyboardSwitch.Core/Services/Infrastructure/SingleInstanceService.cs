namespace KeyboardSwitch.Core.Services.Infrastructure;

internal sealed class SingleInstanceService : ISingleInstanceService
{
    private readonly INamedPipeService namedPipeService;
    private readonly ILogger<SingleInstanceService> logger;
    private readonly string name;

    public SingleInstanceService(
        ServiceProvider<INamedPipeService> namedPipeResolver,
        ILogger<SingleInstanceService> logger,
        string name)
    {
        this.namedPipeService = namedPipeResolver(name);
        this.logger = logger;
        this.name = name;
    }

    public Mutex TryAcquireMutex()
    {
        var mutex = new Mutex(false, $"Global\\{this.name}", out bool createdNew);

        if (!createdNew)
        {
            SendArgumentAndExit();
        }

        bool hasHandle = mutex.WaitOne(5000, false);
        if (!hasHandle)
        {
            const string message = "Timeout waiting for exclusive access on the mutex";
            this.logger.LogError(message);
            throw new TimeoutException(message);
        }

        this.logger.LogDebug("Acquired the global mutex");

        return mutex;
    }

    private void SendArgumentAndExit()
    {
        try
        {
            string? command = GetCommand();
            this.namedPipeService.Write(command ?? String.Empty);

            this.logger.LogDebug("Sent the command to the original instance: {Command}", command);
        } catch (Exception e)
        {
            this.logger.LogError(e, "Unknown error during sending a command to the original instance");
        } finally
        {
            Environment.Exit(0);
        }
    }

    private string? GetCommand() =>
        Environment.GetCommandLineArgs().Length <= 1
            ? null
            : StripCommandLineArgument(Environment.GetCommandLineArgs()[1]);
}
