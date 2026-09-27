using Tmds.DBus.Protocol;

namespace KeyboardSwitch.Linux.Session;

internal sealed record ScreenSaverEndpoint(string Service, string Path)
{
    public static readonly ScreenSaverEndpoint Gnome = new("org.gnome.ScreenSaver", "/org/gnome/ScreenSaver");
    public static readonly ScreenSaverEndpoint Freedesktop = new("org.freedesktop.ScreenSaver", "/ScreenSaver");
}

internal sealed partial class ScreenSaverClient(
    DBusConnectionProvider connectionProvider,
    ScreenSaverEndpoint endpoint,
    ILogger<ScreenSaverClient> logger)
    : DisposableService, ILockStateClient
{
    private static readonly TimeSpan CallTimeout = TimeSpan.FromSeconds(5);

    private readonly DedicatedDBusConnection connection = connectionProvider.CreateConnection(DBusAddress.Session);
    private readonly ScreenSaverEndpoint endpoint = endpoint;

    public async Task<bool> GetLockState()
    {
        var connection = await this.connection.Get();

        this.LogGettingActive(this.endpoint.Service);

        return await connection
            .CallMethodAsync(this.CreateGetActiveCall(connection), this.ReadBoolean, null)
            .WaitAsync(CallTimeout);
    }

    public async ValueTask<IDisposable> WatchLockState(Action<Exception?, bool> handler)
    {
        var connection = await this.connection.Get();

        this.LogWatchingActive(this.endpoint.Service);

        return await connection.WatchSignalAsync(
            this.endpoint.Service,
            this.endpoint.Path,
            this.endpoint.Service,
            "ActiveChanged",
            this.ReadBoolean,
            notification => notification.Handle(handler),
            DBusConstants.CompletionFlags,
            emitOnCapturedContext: false);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.connection.Dispose();
        }
    }

    private MessageBuffer CreateGetActiveCall(DBusConnection connection)
    {
        using var writer = connection.GetMessageWriter();

        writer.WriteMethodCallHeader(
            this.endpoint.Service, this.endpoint.Path, this.endpoint.Service, "GetActive", null, MessageFlags.None);

        return writer.CreateMessage();
    }

    private bool ReadBoolean(Message message, object? state) =>
        message.GetBodyReader().ReadBool();

    [LoggerMessage(LogLevel.Debug, "Getting whether the screen saver is active: {Service}")]
    private partial void LogGettingActive(string service);

    [LoggerMessage(LogLevel.Debug, "Watching whether the screen saver is active: {Service}")]
    private partial void LogWatchingActive(string service);
}
