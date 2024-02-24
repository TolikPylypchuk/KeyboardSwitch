namespace KeyboardSwitch.MacOS.Services;

internal sealed class MacLayoutService(ILogger<MacLayoutService> logger) : CachingLayoutService
{
    public override KeyboardLayout GetCurrentKeyboardLayout()
    {
        logger.LogDebug("Getting current keyboard layout");

        using var source = HIToolbox.TISCopyCurrentKeyboardInputSource();
        return this.CreateKeyboardLayout(source);
    }

    public override void SwitchCurrentLayout(SwitchDirection direction, SwitchSettings settings)
    {
        logger.LogDebug("Switching the current layout {Direction}", direction.AsString());

        using var sources = HIToolbox.TISCreateInputSourceList(new CFDictionaryRef(), includeAllInstalled: false);
        long count = CoreFoundation.CFArrayGetCount(sources);

        var sourcesList = Enumerable.Range(0, (int)count)
            .Select(i => new TISInputSourceRef(CoreFoundation.CFArrayGetValueAtIndex(sources, i), shouldRelease: false))
            .Where(IsKeyboardInputSource)
            .ToList();

        if (direction == SwitchDirection.Backward)
        {
            sourcesList.Reverse();
        }

        using var currentSource = HIToolbox.TISCopyCurrentKeyboardInputSource();
        string currentSourceName = GetInputSourceName(currentSource);

        var sourceToSet = sourcesList
            .SkipWhile(source => GetInputSourceName(source) != currentSourceName)
            .Skip(1)
            .FirstOrDefault()
            ?? this.GetFirstKeyboardSource(sourcesList);

        HIToolbox.TISSelectInputSource(sourceToSet);
    }

    protected override List<KeyboardLayout> GetKeyboardLayoutsInternal()
    {
        using var properties = new CFDictionaryRef();
        using var sources = HIToolbox.TISCreateInputSourceList(properties, includeAllInstalled: false);
        long count = CoreFoundation.CFArrayGetCount(sources);

        return Enumerable.Range(0, (int)count)
            .Select(i => new TISInputSourceRef(CoreFoundation.CFArrayGetValueAtIndex(sources, i), shouldRelease: false))
            .Where(IsKeyboardInputSource)
            .Select(this.CreateKeyboardLayout)
            .ToList();
    }

    private KeyboardLayout CreateKeyboardLayout(TISInputSourceRef source)
    {
        var name = GetInputSourceName(source);
        var localizedName = GetInputSourceLocalizedName(source);

        return new(name, localizedName, name[KeyboardLayoutPrefix.Length..].Replace('.', ' '), String.Empty);
    }

    private TISInputSourceRef GetFirstKeyboardSource(List<TISInputSourceRef> sources) =>
        sources.FirstOrDefault(IsKeyboardInputSource)
            ?? throw new ArgumentOutOfRangeException(nameof(sources), "No input sources are keyboard sources");
}
