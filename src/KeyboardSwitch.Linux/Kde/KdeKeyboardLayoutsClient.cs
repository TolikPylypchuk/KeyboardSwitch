using Tmds.DBus.Protocol;

namespace KeyboardSwitch.Linux.Kde;

internal sealed partial class KdeKeyboardLayoutsClient(
    DBusConnection connection,
    ILogger<KdeKeyboardLayoutsClient> logger)
{
    private const string KeyboardService = "org.kde.keyboard";

    private const string LayoutsPath = "/Layouts";
    private const string KeyboardLayoutsInterface = "org.kde.KeyboardLayouts";

    private static readonly TimeSpan CallTimeout = TimeSpan.FromSeconds(5);

    private readonly DBusConnection connection = connection;

    public Task<uint> GetLayout()
    {
        this.LogGettingCurrentLayout();

        return this.connection
            .CallMethodAsync(this.CreateCall("getLayout"), this.ReadUInt32, null)
            .WaitAsync(CallTimeout);
    }

    public Task<List<KdeLayout>> GetLayoutsList()
    {
        this.LogGettingLayouts();

        return this.connection
            .CallMethodAsync(this.CreateCall("getLayoutsList"), this.ReadLayouts, null)
            .WaitAsync(CallTimeout);
    }

    public Task<bool> SetLayout(uint index)
    {
        this.LogSettingCurrentLayout(index);

        return this.connection
            .CallMethodAsync(this.CreateSetLayoutCall(index), this.ReadBoolean, null)
            .WaitAsync(CallTimeout);
    }

    private MessageBuffer CreateCall(string member)
    {
        using var writer = this.connection.GetMessageWriter();

        writer.WriteMethodCallHeader(
            KeyboardService, LayoutsPath, KeyboardLayoutsInterface, member, null, MessageFlags.None);

        return writer.CreateMessage();
    }

    private MessageBuffer CreateSetLayoutCall(uint index)
    {
        using var writer = this.connection.GetMessageWriter();

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
