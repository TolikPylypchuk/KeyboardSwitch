using KeyboardSwitch.Core.Services.Users;

namespace KeyboardSwitch.Core.Services.Infrastructure;

internal sealed partial class SingleInstanceService(
    INamedPipeService namedPipeService,
    IUserProvider userProvider,
    ILogger<SingleInstanceService> logger)
    : ISingleInstanceService
{
    public Mutex TryAcquireMutex(string name)
    {
        var mutex = new Mutex(false, $"Global\\{userProvider.GetCurrentUser()}-{name}", out bool createdNew);

        if (!createdNew)
        {
            SendArgumentAndExit(name);
        }

        bool hasHandle = mutex.WaitOne(5000, false);
        if (!hasHandle)
        {
            this.LogMutexTimeout();
            throw new TimeoutException("Timeout waiting for exclusive access on the mutex");
        }

        this.LogAcquiredGlobalMutex();

        return mutex;
    }

    private void SendArgumentAndExit(string pipeName)
    {
        try
        {
            string? command = GetCommand();
            namedPipeService.Write(pipeName, command ?? String.Empty);

            this.LogSentCommand(command);
        } catch (Exception e)
        {
            this.LogUnknownError(e);
        } finally
        {
            Environment.Exit(0);
        }
    }

    private string? GetCommand() =>
        Environment.GetCommandLineArgs().Length <= 1
            ? null
            : StripCommandLineArgument(Environment.GetCommandLineArgs()[1]);

    [LoggerMessage(LogLevel.Error, "Timeout waiting for exclusive access on the mutex")]
    private partial void LogMutexTimeout();

    [LoggerMessage(LogLevel.Debug, "Acquired the global mutex")]
    private partial void LogAcquiredGlobalMutex();

    [LoggerMessage(LogLevel.Debug, "Sent the command to the original instance: {Command}")]
    private partial void LogSentCommand(string? command);

    [LoggerMessage(LogLevel.Error, "Unknown error during sending a command to the original instance")]
    private partial void LogUnknownError(Exception e);
}
