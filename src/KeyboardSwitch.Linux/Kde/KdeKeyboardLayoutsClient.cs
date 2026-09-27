using Tmds.DBus.Protocol;

namespace KeyboardSwitch.Linux.Kde;

internal sealed partial class KdeKeyboardLayoutsClient(
    DBusConnectionProvider connectionProvider,
    ILogger<KdeKeyboardLayoutsClient> logger)
    : DisposableService
{
    private const string KeyboardService = "org.kde.keyboard";

    private const string LayoutsPath = "/Layouts";
    private const string KeyboardLayoutsInterface = "org.kde.KeyboardLayouts";

    private static readonly TimeSpan CallTimeout = TimeSpan.FromSeconds(5);

    private readonly DedicatedDBusConnection connection = connectionProvider.CreateConnection(DBusAddress.Session);

    public async Task<uint> GetLayout()
    {
        var connection = await this.connection.Get();

        this.LogGettingCurrentLayout();

        return await connection
            .CallMethodAsync(this.CreateCall(connection, "getLayout"), this.ReadUInt32, null)
            .WaitAsync(CallTimeout);
    }

    public async Task<List<KdeLayout>> GetLayoutsList()
    {
        var connection = await this.connection.Get();

        this.LogGettingLayouts();

        return await connection
            .CallMethodAsync(this.CreateCall(connection, "getLayoutsList"), this.ReadLayouts, null)
            .WaitAsync(CallTimeout);
    }

    public async Task<bool> SetLayout(uint index)
    {
        var connection = await this.connection.Get();

        this.LogSettingCurrentLayout(index);

        return await connection
            .CallMethodAsync(this.CreateSetLayoutCall(connection, index), this.ReadBoolean, null)
            .WaitAsync(CallTimeout);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.connection.Dispose();
        }
    }

    private MessageBuffer CreateCall(DBusConnection connection, string member)
    {
        using var writer = connection.GetMessageWriter();

        writer.WriteMethodCallHeader(
            KeyboardService, LayoutsPath, KeyboardLayoutsInterface, member, null, MessageFlags.None);

        return writer.CreateMessage();
    }

    private MessageBuffer CreateSetLayoutCall(DBusConnection connection, uint index)
    {
        using var writer = connection.GetMessageWriter();

        writer.WriteMethodCallHeader(
            KeyboardService, LayoutsPath, KeyboardLayoutsInterface, "setLayout", "u", MessageFlags.None);

        writer.WriteUInt32(index);

        return writer.CreateMessage();
    }

    private List<KdeLayout> ReadLayouts(Message message, object? state)
    {
        var reader = message.GetBodyReader();
        var layouts = new List<KdeLayout>();

        var arrayEnd = reader.ReadArrayStart(DBusType.Struct);

        while (reader.HasNext(arrayEnd))
        {
            reader.AlignStruct();
            layouts.Add(new(reader.ReadString(), reader.ReadString(), reader.ReadString()));
        }

        return layouts;
    }

    private uint ReadUInt32(Message message, object? state) =>
        message.GetBodyReader().ReadUInt32();

    private bool ReadBoolean(Message message, object? state) =>
        message.GetBodyReader().ReadBool();

    [LoggerMessage(LogLevel.Debug, "Getting the current layout via Plasma's keyboard layouts interface")]
    private partial void LogGettingCurrentLayout();

    [LoggerMessage(LogLevel.Debug, "Getting all layouts via Plasma's keyboard layouts interface")]
    private partial void LogGettingLayouts();

    [LoggerMessage(LogLevel.Debug, "Setting the current layout via Plasma's keyboard layouts interface: {Index}")]
    private partial void LogSettingCurrentLayout(uint index);
}
