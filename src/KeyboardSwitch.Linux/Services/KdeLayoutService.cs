using Tmds.DBus.Protocol;

namespace KeyboardSwitch.Linux.Services;

internal sealed partial class KdeLayoutService(
    KdeKeyboardLayoutsClient client,
    KxkbConfigReader configReader,
    ILogger<KdeLayoutService> logger)
    : CachingLayoutService
{
    private static readonly ImmutableHashSet<string> UnavailableErrors =
    [
        "org.freedesktop.DBus.Error.ServiceUnknown",
        "org.freedesktop.DBus.Error.UnknownObject",
        "org.freedesktop.DBus.Error.UnknownInterface",
        "org.freedesktop.DBus.Error.UnknownMethod"
    ];

    private readonly KdeKeyboardLayoutsClient client = client;
    private readonly KxkbConfigReader configReader = configReader;

    private List<KeyboardLayout>? layoutsByIndex;
    private Dictionary<string, uint>? indexesByLayoutId;
    private bool isInterfaceUnusable;

    public override LayoutServiceStatus Status =>
        this.isInterfaceUnusable ? LayoutServiceStatus.Unavailable : LayoutServiceStatus.Ok;

    public override async Task<KeyboardLayout> GetCurrentKeyboardLayout()
    {
        this.LogGettingCurrentLayout();

        try
        {
            await this.GetKeyboardLayouts();

            uint index = await this.client.GetLayout();
            var layouts = this.layoutsByIndex ?? [];

            return index < layouts.Count
                ? layouts[(int)index]
                : throw new KdeKeyboardLayoutsException($"Plasma reported an invalid layout index: {index}");
        } catch (Exception e) when (this.IsInterfaceUnavailable(e))
        {
            throw this.InterfaceUnusable(e);
        }
    }

    public override async Task SwitchCurrentLayout(SwitchDirection direction, SwitchSettings settings)
    {
        this.LogSwitchingCurrentLayout(direction);

        try
        {
            var allLayouts = await this.GetKeyboardLayouts();
            var currentLayout = await this.GetCurrentKeyboardLayout();

            int currentIndex = allLayouts
                .Select((layout, index) => (Layout: layout, Index: index))
                .Where(layout => layout.Layout.Id == currentLayout.Id)
                .Select(layout => (int?)layout.Index)
                .FirstOrDefault()
                ?? 0;

            int offset = direction == SwitchDirection.Forward ? 1 : -1;
            int newIndex = (currentIndex + offset + allLayouts.Count) % allLayouts.Count;

            uint kdeIndex = this.GetKdeIndex(allLayouts[newIndex]);

            bool success = await this.client.SetLayout(kdeIndex);

            if (!success)
            {
                this.LogCouldNotSetLayout(kdeIndex);
            }
        } catch (Exception e) when (this.IsInterfaceUnavailable(e))
        {
            throw this.InterfaceUnusable(e);
        }
    }

    protected override async Task<List<KeyboardLayout>> GetKeyboardLayoutsInternal()
    {
        this.LogGettingAllLayouts();

        try
        {
            var kdeLayouts = await this.client.GetLayoutsList();

            if (kdeLayouts.Count == 0)
            {
                throw new KdeKeyboardLayoutsException("Plasma didn't report any keyboard layouts");
            }

            var layouts = this.CreateKeyboardLayouts(kdeLayouts);

            this.layoutsByIndex = layouts;

            this.indexesByLayoutId = layouts
                .Select((layout, index) => (Layout: layout, Index: (uint)index))
                .GroupBy(layout => layout.Layout.Id)
                .ToDictionary(layouts => layouts.Key, layouts => layouts.First().Index);

            return [.. layouts.DistinctBy(layout => layout.Id)];
        } catch (Exception e) when (this.IsInterfaceUnavailable(e))
        {
            throw this.InterfaceUnusable(e);
        }
    }

    protected override void OnSettingsInvalidated()
    {
        this.isInterfaceUnusable = false;
        this.layoutsByIndex = null;
        this.indexesByLayoutId = null;
    }

    private List<KeyboardLayout> CreateKeyboardLayouts(List<KdeLayout> kdeLayouts)
    {
        var variants = this.GetVariants(kdeLayouts);
        return [.. kdeLayouts.Select((layout, index) => CreateKeyboardLayout(layout, variants[index]))];
    }

    private List<string> GetVariants(List<KdeLayout> kdeLayouts)
    {
        var configuredLayouts = this.configReader.ReadConfiguredLayouts();

        if (configuredLayouts.Count != kdeLayouts.Count ||
            !configuredLayouts.Zip(kdeLayouts).All(layouts =>
                layouts.First.Layout.Equals(layouts.Second.ShortName, StringComparison.Ordinal)))
        {
            this.LogConfiguredLayoutsMismatch();
            return [.. kdeLayouts.Select(_ => String.Empty)];
        }

        return [.. configuredLayouts.Select(layout => layout.Variant)];
    }

    private static KeyboardLayout CreateKeyboardLayout(KdeLayout layout, string variant) =>
        new(
            $"{layout.ShortName}:{variant}",
            String.IsNullOrEmpty(layout.LongName) ? layout.ShortName : layout.LongName,
            String.IsNullOrEmpty(variant) ? layout.ShortName : $"{layout.ShortName} ({variant})",
            String.Empty);

    private uint GetKdeIndex(KeyboardLayout layout) =>
        this.indexesByLayoutId?.TryGetValue(layout.Id, out uint index) == true
            ? index
            : throw new KdeKeyboardLayoutsException($"Plasma doesn't have the keyboard layout: {layout.Id}");

    private bool IsInterfaceUnavailable(Exception e) =>
        e is DBusErrorReplyException reply && UnavailableErrors.Contains(reply.ErrorName) ||
        e is DBusConnectFailedException or DBusConnectionClosedException;

    private KdeKeyboardLayoutsException InterfaceUnusable(Exception cause)
    {
        if (!this.isInterfaceUnusable)
        {
            this.isInterfaceUnusable = true;
            this.LogInterfaceUnusable(cause);
        }

        return new("Plasma's keyboard layouts interface isn't available", cause);
    }

    [LoggerMessage(LogLevel.Debug, "Getting the current keyboard layout")]
    private partial void LogGettingCurrentLayout();

    [LoggerMessage(LogLevel.Debug, "Switching the current layout: {Direction}")]
    private partial void LogSwitchingCurrentLayout(SwitchDirection direction);

    [LoggerMessage(LogLevel.Debug, "Getting all keyboard layouts")]
    private partial void LogGettingAllLayouts();

    [LoggerMessage(
        LogLevel.Warning,
        "Plasma's config doesn't describe keyboard layouts - they will be configured without variants")]
    private partial void LogConfiguredLayoutsMismatch();

    [LoggerMessage(LogLevel.Warning, "Plasma refused to set the current layout: {Index}")]
    private partial void LogCouldNotSetLayout(uint index);

    [LoggerMessage(LogLevel.Warning, "Plasma's keyboard layouts interface is unusable")]
    private partial void LogInterfaceUnusable(Exception e);
}
