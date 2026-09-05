using Tmds.DBus.Protocol;

namespace KeyboardSwitch.Linux.Services;

internal sealed partial class GnomeLayoutService(
    GnomeShellExtensionClient client,
    ILayoutService? fallback,
    ILogger<GnomeLayoutService> logger)
    : CachingLayoutService
{
    private static readonly ImmutableHashSet<string> UnavailableErrors =
    [
        "org.freedesktop.DBus.Error.ServiceUnknown",
        "org.freedesktop.DBus.Error.UnknownObject",
        "org.freedesktop.DBus.Error.UnknownInterface",
        "org.freedesktop.DBus.Error.UnknownMethod"
    ];

    private readonly GnomeShellExtensionClient client = client;
    private readonly ILayoutService? fallback = fallback;

    private Dictionary<string, uint>? shellIndexesByLayoutId;
    private bool isExtensionUnusable;

    public override async Task<KeyboardLayout> GetCurrentKeyboardLayout()
    {
        if (this.isExtensionUnusable)
        {
            return await this.GetFallback().GetCurrentKeyboardLayout();
        }

        try
        {
            return CreateKeyboardLayout(await this.CallExtension(this.client.GetCurrentLayout));
        } catch (Exception e) when (this.IsExtensionUnavailable(e))
        {
            return await this.GetFallback(e).GetCurrentKeyboardLayout();
        }
    }

    public override async Task SwitchCurrentLayout(SwitchDirection direction, SwitchSettings settings)
    {
        this.LogSwitchingCurrentLayout(direction);

        if (this.isExtensionUnusable)
        {
            await this.GetFallback().SwitchCurrentLayout(direction, settings);
            return;
        }

        try
        {
            var allLayouts = await this.GetKeyboardLayouts();
            var currentLayout = await this.GetCurrentKeyboardLayout();

            if (this.isExtensionUnusable)
            {
                await this.GetFallback().SwitchCurrentLayout(direction, settings);
                return;
            }

            int currentIndex = allLayouts
                .Select((layout, index) => (Layout: layout, Index: index))
                .Where(layout => layout.Layout.Id == currentLayout.Id)
                .Select(layout => (int?)layout.Index)
                .FirstOrDefault()
                ?? 0;

            int offset = direction == SwitchDirection.Forward ? 1 : -1;
            int newIndex = (currentIndex + offset + allLayouts.Count) % allLayouts.Count;

            await this.CallExtension(() => this.client.SetCurrentLayout(this.GetShellIndex(allLayouts[newIndex])));
        } catch (Exception e) when (this.IsExtensionUnavailable(e))
        {
            await this.GetFallback(e).SwitchCurrentLayout(direction, settings);
        }
    }

    protected override async Task<List<KeyboardLayout>> GetKeyboardLayoutsInternal()
    {
        if (this.isExtensionUnusable)
        {
            return [.. await this.GetFallback().GetKeyboardLayouts()];
        }

        try
        {
            var sources = await this.CallExtension(this.client.GetLayouts);

            this.shellIndexesByLayoutId = sources
                .GroupBy(GetLayoutId)
                .ToDictionary(sources => sources.Key, sources => sources.First().Index);

            return sources
                .Select(CreateKeyboardLayout)
                .DistinctBy(layout => layout.Id)
                .ToList();
        } catch (Exception e) when (this.IsExtensionUnavailable(e))
        {
            return [.. await this.GetFallback(e).GetKeyboardLayouts()];
        }
    }

    protected override void OnSettingsInvalidated()
    {
        this.isExtensionUnusable = false;
        this.shellIndexesByLayoutId = null;
    }

    private async Task<T> CallExtension<T>(Func<Task<T>> call)
    {
        try
        {
            return await call();
        } catch (Exception e) when (this.IsExtensionUnavailable(e))
        {
            this.LogExtensionCallFailed(e);

            await this.TryEnableExtension();

            return await call();
        }
    }

    private Task<object?> CallExtension(Func<Task> call) =>
        this.CallExtension<object?>(async () =>
        {
            await call();
            return null;
        });

    private async Task TryEnableExtension()
    {
        var info = await this.client.GetExtensionInfo();

        switch (info.State)
        {
            case GnomeExtensionState.NotLoaded:
                this.LogExtensionNotLoaded();
                break;

            case GnomeExtensionState.Active when info.Version < GnomeShellExtensionClient.InterfaceVersion:
                this.LogExtensionOutOfDate(info.Version);
                break;

            case GnomeExtensionState.Error:
                this.LogExtensionInErrorState(String.Join("; ", await this.client.GetExtensionErrors()));
                break;

            case GnomeExtensionState.OutOfDate:
                this.LogExtensionIncompatibleWithShell(GnomeDetector.TryGetGnomeVersion());
                break;

            case GnomeExtensionState.Inactive:
            case GnomeExtensionState.Initialized:
                if (!await this.client.EnableExtension())
                {
                    this.LogCouldNotEnableExtension();
                }

                break;
        }
    }

    private ILayoutService GetFallback(Exception? cause = null)
    {
        if (!this.isExtensionUnusable)
        {
            this.isExtensionUnusable = true;
            this.LogExtensionUnusable(cause);
        }

        const string waylandFallbackMessage = "The Switch Layout extension for GNOME isn't active, " +
            "and there is no way to manage keyboard layouts on Wayland without it";

        return this.fallback ?? (cause is not null
            ? throw new GnomeShellException(waylandFallbackMessage, cause)
            : throw new GnomeShellException(waylandFallbackMessage));
    }

    private uint GetShellIndex(KeyboardLayout layout) =>
        this.shellIndexesByLayoutId?.TryGetValue(layout.Id, out uint index) == true
            ? index
            : throw new GnomeShellException($"GNOME Shell doesn't have the keyboard layout: {layout.Id}");

    private bool IsExtensionUnavailable(Exception e) =>
        e is DBusErrorReplyException reply && UnavailableErrors.Contains(reply.ErrorName) ||
        e is DBusConnectFailedException or DBusConnectionClosedException;

    private KeyboardLayout CreateKeyboardLayout(GnomeInputSource source)
    {
        var (symbol, variant) = this.ParseXkbId(source.XkbId);

        return new(
            this.GetLayoutId(source),
            source.DisplayName,
            String.IsNullOrEmpty(variant) ? symbol : $"{symbol} ({variant})",
            String.Empty);
    }

    private string GetLayoutId(GnomeInputSource source)
    {
        var (symbol, variant) = this.ParseXkbId(source.XkbId);
        return $"{symbol}:{variant}";
    }

    private (string Symbol, string Variant) ParseXkbId(string xkbId)
    {
        int separator = xkbId.IndexOf('+', StringComparison.Ordinal);

        return separator < 0
            ? (xkbId, String.Empty)
            : (xkbId[..separator], xkbId[(separator + 1)..]);
    }

    [LoggerMessage(LogLevel.Debug, "Switching the current layout: {Direction}")]
    private partial void LogSwitchingCurrentLayout(SwitchDirection direction);

    [LoggerMessage(LogLevel.Debug, "A call to the Switch Layout extension for GNOME failed")]
    private partial void LogExtensionCallFailed(Exception e);

    [LoggerMessage(
        LogLevel.Warning,
        "GNOME Shell doesn't know about the Switch Layout extension - GNOME Shell must be restarted to load it")]
    private partial void LogExtensionNotLoaded();

    [LoggerMessage(
        LogLevel.Warning,
        "GNOME Shell is running version {Version} of the Switch Layout extension - " +
        "GNOME Shell must be restarted to load the new version")]
    private partial void LogExtensionOutOfDate(int version);

    [LoggerMessage(LogLevel.Error, "The Switch Layout extension for GNOME is in the error state: {Errors}")]
    private partial void LogExtensionInErrorState(string errors);

    [LoggerMessage(
        LogLevel.Error,
        "The Switch Layout extension for GNOME doesn't support the current version of GNOME Shell: {Version}")]
    private partial void LogExtensionIncompatibleWithShell(Version? version);

    [LoggerMessage(LogLevel.Error, "GNOME Shell refused to enable the Switch Layout extension")]
    private partial void LogCouldNotEnableExtension();

    [LoggerMessage(LogLevel.Warning, "The Switch Layout extension for GNOME is unusable")]
    private partial void LogExtensionUnusable(Exception? e);
}
