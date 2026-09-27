using Tmds.DBus.Protocol;

namespace KeyboardSwitch.Linux.Gnome;

internal sealed partial class GnomeShellExtensionClient(
    DBusConnectionProvider connectionProvider,
    ILogger<GnomeShellExtensionClient> logger)
    : DisposableService
{
    public const string ExtensionUuid = "switch-layout@tolik.io";

    public const int InterfaceVersion = 3;

    private const string ShellService = "org.gnome.Shell";

    private const string SwitchLayoutPath = "/org/gnome/Shell/Extensions/SwitchLayout";
    private const string SwitchLayoutInterface = "org.gnome.Shell.Extensions.SwitchLayout";

    private const string ShellPath = "/org/gnome/Shell";
    private const string ExtensionsInterface = "org.gnome.Shell.Extensions";

    private const string StateKey = "state";
    private const string VersionKey = "version";

    private static readonly TimeSpan CallTimeout = TimeSpan.FromSeconds(5);

    private readonly DedicatedDBusConnection connection = connectionProvider.CreateConnection(DBusAddress.Session);

    public async Task<GnomeInputSource> GetCurrentLayout()
    {
        var connection = await this.connection.Get();

        this.LogGettingCurrentLayout();

        return await connection
            .CallMethodAsync(
                this.CreateCall(connection, SwitchLayoutPath, SwitchLayoutInterface, "GetCurrentLayout"),
                this.ReadInputSource,
                null)
            .WaitAsync(CallTimeout);
    }

    public async Task<List<GnomeInputSource>> GetLayouts()
    {
        var connection = await this.connection.Get();

        this.LogGettingLayouts();

        return await connection
            .CallMethodAsync(
                this.CreateCall(connection, SwitchLayoutPath, SwitchLayoutInterface, "GetLayouts"),
                this.ReadInputSources,
                null)
            .WaitAsync(CallTimeout);
    }

    public async Task SetCurrentLayout(uint index)
    {
        var connection = await this.connection.Get();

        this.LogSettingCurrentLayout(index);

        await connection
            .CallMethodAsync(this.CreateSetCurrentLayoutCall(connection, index))
            .WaitAsync(CallTimeout);
    }

    public async Task<GnomeExtensionInfo> GetExtensionInfo()
    {
        var connection = await this.connection.Get();

        this.LogGettingExtensionInfo();

        return await connection
            .CallMethodAsync(this.CreateExtensionCall(connection, "GetExtensionInfo"), this.ReadExtensionInfo, null)
            .WaitAsync(CallTimeout);
    }

    public async Task<bool> EnableExtension()
    {
        var connection = await this.connection.Get();

        this.LogEnablingExtension();

        return await connection
            .CallMethodAsync(this.CreateExtensionCall(connection, "EnableExtension"), this.ReadBoolean, null)
            .WaitAsync(CallTimeout);
    }

    public async Task<string[]> GetExtensionErrors()
    {
        var connection = await this.connection.Get();

        return await connection
            .CallMethodAsync(this.CreateExtensionCall(connection, "GetExtensionErrors"), this.ReadStrings, null)
            .WaitAsync(CallTimeout);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.connection.Dispose();
        }
    }

    private MessageBuffer CreateCall(DBusConnection connection, string path, string @interface, string member)
    {
        using var writer = connection.GetMessageWriter();
        writer.WriteMethodCallHeader(ShellService, path, @interface, member, null, MessageFlags.None);
        return writer.CreateMessage();
    }

    private MessageBuffer CreateSetCurrentLayoutCall(DBusConnection connection, uint index)
    {
        using var writer = connection.GetMessageWriter();

        writer.WriteMethodCallHeader(
            ShellService, SwitchLayoutPath, SwitchLayoutInterface, "SetCurrentLayout", "u", MessageFlags.None);

        writer.WriteUInt32(index);

        return writer.CreateMessage();
    }

    private MessageBuffer CreateExtensionCall(DBusConnection connection, string member)
    {
        using var writer = connection.GetMessageWriter();

        writer.WriteMethodCallHeader(
            ShellService, ShellPath, ExtensionsInterface, member, "s", MessageFlags.None);

        writer.WriteString(ExtensionUuid);

        return writer.CreateMessage();
    }

    private GnomeInputSource ReadInputSource(Message message, object? state)
    {
        var reader = message.GetBodyReader();
        return this.ReadInputSource(ref reader);
    }

    private List<GnomeInputSource> ReadInputSources(Message message, object? state)
    {
        var reader = message.GetBodyReader();
        var sources = new List<GnomeInputSource>();

        var arrayEnd = reader.ReadArrayStart(DBusType.Struct);

        while (reader.HasNext(arrayEnd))
        {
            reader.AlignStruct();
            sources.Add(this.ReadInputSource(ref reader));
        }

        return sources;
    }

    private GnomeInputSource ReadInputSource(ref Reader reader) =>
        new(reader.ReadUInt32(), reader.ReadString(), reader.ReadString(), reader.ReadString());

    private GnomeExtensionInfo ReadExtensionInfo(Message message, object? state)
    {
        var reader = message.GetBodyReader();

        var info = reader.ReadDictionaryOfStringToVariantValue();

        return new(
            info.TryGetValue(StateKey, out var extensionState)
                ? (GnomeExtensionState)this.ReadNumber(extensionState)
                : GnomeExtensionState.NotLoaded,
            info.TryGetValue(VersionKey, out var version) ? this.ReadNumber(version) : 0);
    }

    private int ReadNumber(VariantValue value) =>
        value.Type switch
        {
            VariantValueType.Double => (int)value.GetDouble(),
            VariantValueType.Int16 => value.GetInt16(),
            VariantValueType.UInt16 => value.GetUInt16(),
            VariantValueType.Int32 => value.GetInt32(),
            VariantValueType.UInt32 => (int)value.GetUInt32(),
            VariantValueType.Int64 => (int)value.GetInt64(),
            VariantValueType.UInt64 => (int)value.GetUInt64(),
            _ => 0
        };

    private bool ReadBoolean(Message message, object? state) =>
        message.GetBodyReader().ReadBool();

    private string[] ReadStrings(Message message, object? state) =>
        message.GetBodyReader().ReadArrayOfString();

    [LoggerMessage(LogLevel.Debug, "Getting the current layout through the GNOME extension")]
    private partial void LogGettingCurrentLayout();

    [LoggerMessage(LogLevel.Debug, "Getting all layouts through the GNOME extension")]
    private partial void LogGettingLayouts();

    [LoggerMessage(LogLevel.Debug, "Setting the current layout through the GNOME extension: {Index}")]
    private partial void LogSettingCurrentLayout(uint index);

    [LoggerMessage(LogLevel.Debug, "Getting the info on the GNOME extension")]
    private partial void LogGettingExtensionInfo();

    [LoggerMessage(LogLevel.Debug, "Enabling the Switch Layout extension for GNOME")]
    private partial void LogEnablingExtension();
}
