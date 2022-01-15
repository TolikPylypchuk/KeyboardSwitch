namespace KeyboardSwitch.MacOS.Services;

internal class MacLayoutService : CachingLayoutService
{
    private const string KeyboardLayoutPrefix = "com.apple.keylayout.";

    public override KeyboardLayout GetCurrentKeyboardLayout()
    {
        using var source = HIToolbox.TISCopyCurrentKeyboardInputSource();
        return this.CreateKeyboardLayout(source);
    }

    public override void SwitchCurrentLayout(SwitchDirection direction, SwitchSettings settings)
    {
        using var sources = HIToolbox.TISCreateInputSourceList(new CFDictionaryRef(), includeAllInstalled: false);
        long count = CoreFoundation.CFArrayGetCount(sources);

        var sourcesList = Enumerable.Range(0, (int)count)
            .Select(i => new TISInputSourceRef(CoreFoundation.CFArrayGetValueAtIndex(sources, i)))
            .ToList();

        if (direction == SwitchDirection.Backward)
        {
            sourcesList.Reverse();
        }

        var currentLayout = this.GetCurrentKeyboardLayout();

        var sourceToSet = sourcesList
            .Select(source => (Source: source, Layout: this.CreateKeyboardLayout(source)))
            .Where(sourceAndLayout => this.IsActuallyKeyboardLayout(sourceAndLayout.Layout))
            .SkipWhile(sourceAndLayout => sourceAndLayout.Layout.Id != currentLayout.Id)
            .Skip(1)
            .Select(sourceAndLayout => sourceAndLayout.Source)
            .FirstOrDefault()
            ?? this.GetFirstKeyboardSource(sourcesList);

        HIToolbox.TISSelectInputSource(sourceToSet);
    }

    protected override List<KeyboardLayout> GetKeyboardLayoutsInternal()
    {
        using var sources = HIToolbox.TISCreateInputSourceList(new CFDictionaryRef(), includeAllInstalled: false);
        long count = CoreFoundation.CFArrayGetCount(sources);

        var result = new List<KeyboardLayout>((int)count);

        for (int i = 0; i < count; i++)
        {
            using var source = new TISInputSourceRef(CoreFoundation.CFArrayGetValueAtIndex(sources, i));

            var layout = this.CreateKeyboardLayout(source);

            if (this.IsActuallyKeyboardLayout(layout))
            {
                result.Add(layout);
            }
        }

        return result;
    }

    private KeyboardLayout CreateKeyboardLayout(TISInputSourceRef source)
    {
        using var nameRef = HIToolbox.TISGetInputSourceProperty(source, HIToolbox.GetTISPropertyInputSourceID());
        using var localizedNameRef = HIToolbox.TISGetInputSourceProperty(
            source, HIToolbox.GetTISPropertyLocalizedName());

        var name = GetStringValue(nameRef);
        var localizedName = GetStringValue(localizedNameRef);

        return new(name, localizedName, name[KeyboardLayoutPrefix.Length..], String.Empty);
    }

    private TISInputSourceRef GetFirstKeyboardSource(List<TISInputSourceRef> sources) =>
        sources.FirstOrDefault(source => this.IsActuallyKeyboardLayout(this.CreateKeyboardLayout(source)))
            ?? throw new ArgumentOutOfRangeException(nameof(sources), "No input sources are keyboard sources");

    private bool IsActuallyKeyboardLayout(KeyboardLayout layout) =>
        layout.Id.StartsWith(KeyboardLayoutPrefix);
}
