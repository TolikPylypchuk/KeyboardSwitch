using Tmds.DBus.Protocol;

namespace KeyboardSwitch.Linux.Session;

internal sealed partial class LogindSessionClient(
    DBusConnectionProvider connectionProvider,
    ILogger<LogindSessionClient> logger)
    : DisposableService, ILockStateClient
{
    private const string LoginService = "org.freedesktop.login1";

    private const string ManagerPath = "/org/freedesktop/login1";
    private const string ManagerInterface = "org.freedesktop.login1.Manager";
    private const string SessionInterface = "org.freedesktop.login1.Session";
    private const string PropertiesInterface = "org.freedesktop.DBus.Properties";

    private const string LockedHintProperty = "LockedHint";

    private const string SessionIdVariable = "XDG_SESSION_ID";
    private const string AutoSessionId = "auto";

    private static readonly TimeSpan CallTimeout = TimeSpan.FromSeconds(5);

    private readonly DedicatedDBusConnection connection = connectionProvider.CreateConnection(DBusAddress.System);

    private string? sessionPath;

    public async Task<bool> GetLockState()
    {
        var connection = await this.connection.Get();
        string sessionPath = await this.GetSessionPath(connection);

        this.LogGettingLockedHint(sessionPath);

        return await connection
            .CallMethodAsync(CreateGetLockedHintCall(connection, sessionPath), this.ReadVariantBoolean, null)
            .WaitAsync(CallTimeout);
    }

    public async ValueTask<IDisposable> WatchLockState(Action<Exception?, bool> handler)
    {
        var connection = await this.connection.Get();
        string sessionPath = await this.GetSessionPath(connection);

        this.LogWatchingLockedHint(sessionPath);

        return await connection.WatchPropertiesChangedAsync(
            LoginService,
            sessionPath,
            SessionInterface,
            this.ReadLockedHintChange,
            notification => notification.Handle((e, isLocked) =>
            {
                if (e is not null || isLocked is not null)
                {
                    handler(e, isLocked ?? false);
                }
            }),
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

    private async Task<string> GetSessionPath(DBusConnection connection)
    {
        if (this.sessionPath is null)
        {
            string sessionId = GetSessionId();

            this.LogGettingSessionPath(sessionId);

            this.sessionPath = await connection
                .CallMethodAsync(CreateGetSessionCall(connection, sessionId), this.ReadObjectPath, null)
                .WaitAsync(CallTimeout);
        }

        return this.sessionPath;
    }

    private static string GetSessionId() =>
        Environment.GetEnvironmentVariable(SessionIdVariable) is { Length: > 0 } sessionId
            ? sessionId
            : AutoSessionId;

    private static MessageBuffer CreateGetSessionCall(DBusConnection connection, string sessionId)
    {
        using var writer = connection.GetMessageWriter();

        writer.WriteMethodCallHeader(
            LoginService, ManagerPath, ManagerInterface, "GetSession", "s", MessageFlags.None);

        writer.WriteString(sessionId);

        return writer.CreateMessage();
    }

    private static MessageBuffer CreateGetLockedHintCall(DBusConnection connection, string sessionPath)
    {
        using var writer = connection.GetMessageWriter();

        writer.WriteMethodCallHeader(LoginService, sessionPath, PropertiesInterface, "Get", "ss", MessageFlags.None);

        writer.WriteString(SessionInterface);
        writer.WriteString(LockedHintProperty);

        return writer.CreateMessage();
    }

    private string ReadObjectPath(Message message, object? state) =>
        message.GetBodyReader().ReadObjectPathAsString();

    private bool ReadVariantBoolean(Message message, object? state) =>
        message.GetBodyReader().ReadVariantValue().GetBool();

    private bool? ReadLockedHintChange(Message message, object? state)
    {
        var reader = message.GetBodyReader();

        reader.ReadString();
        var changedProperties = reader.ReadDictionaryOfStringToVariantValue();

        return changedProperties.TryGetValue(LockedHintProperty, out var value) ? value.GetBool() : null;
    }

    [LoggerMessage(LogLevel.Debug, "Getting the path of the logind session: {SessionId}")]
    private partial void LogGettingSessionPath(string sessionId);

    [LoggerMessage(LogLevel.Debug, "Getting the locked hint of the logind session: {SessionPath}")]
    private partial void LogGettingLockedHint(string sessionPath);

    [LoggerMessage(LogLevel.Debug, "Watching the locked hint of the logind session: {SessionPath}")]
    private partial void LogWatchingLockedHint(string sessionPath);
}
