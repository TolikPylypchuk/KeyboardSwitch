namespace KeyboardSwitch.Linux.Services;

internal sealed unsafe class XkbAutoConfigurationService : AutoConfigurationServiceBase
{
    private const uint SingleLayoutIndex = 0;

    protected override IEnumerable<List<KeyToCharResult>> GetChars(List<string> layoutIds)
    {
        if (layoutIds.Count == 0)
        {
            return [];
        }

        using var context = XkbCommon.ContextNew(XkbContextFlags.NoEnvironmentNames);

        if (context.IsInvalid)
        {
            throw new XkbException("Could not create an XKB context");
        }

        var keymaps = layoutIds.Select(id => CreateKeymap(context, id)).ToList();

        try
        {
            uint minKeyCode = keymaps.Min(XkbCommon.KeymapMinKeycode);
            uint maxKeyCode = keymaps.Max(XkbCommon.KeymapMaxKeycode);

            var keyCodes = this.ClosedRange(minKeyCode, maxKeyCode).ToList();

            uint maxLevel = keymaps
                .SelectMany(keymap => keyCodes.Select(keyCode =>
                    XkbCommon.KeymapNumLevelsForKey(keymap, keyCode, SingleLayoutIndex)))
                .Max();

            return Enumerable.Range(0, (int)maxLevel)
                .SelectMany(level => keyCodes.Select(keyCode =>
                    this.GetCharsForKey(keyCode, (uint)level, layoutIds, keymaps)))
                .ToList();
        } finally
        {
            keymaps.ForEach(keymap => keymap.Dispose());
        }
    }

    private static XkbKeymapHandle CreateKeymap(XkbContextHandle context, string layoutId)
    {
        int separator = layoutId.IndexOf(':', StringComparison.Ordinal);
        var (symbol, variant) = separator < 0
            ? (layoutId, String.Empty)
            : (layoutId[..separator], layoutId[(separator + 1)..]);

        var names = new XkbRuleNames
        {
            Layout = Marshal.StringToCoTaskMemUTF8(symbol),
            Variant = !String.IsNullOrEmpty(variant) ? Marshal.StringToCoTaskMemUTF8(variant) : IntPtr.Zero
        };

        try
        {
            var keymap = XkbCommon.KeymapNewFromNames(context, names, XkbKeymapCompileFlags.NoFlags);

            return !keymap.IsInvalid
                ? keymap
                : throw new XkbException($"Could not compile an XKB keymap for layout: {layoutId}");
        } finally
        {
            Marshal.FreeCoTaskMem(names.Layout);
            Marshal.FreeCoTaskMem(names.Variant);
        }
    }

    private List<KeyToCharResult> GetCharsForKey(
        uint keyCode,
        uint level,
        List<string> layoutIds,
        List<XkbKeymapHandle> keymaps) =>
        layoutIds.Zip(keymaps)
            .Select(layout => this.GetCharForKey(keyCode, level, layout.First, layout.Second))
            .ToList();

    private KeyToCharResult GetCharForKey(uint keyCode, uint level, string layoutId, XkbKeymapHandle keymap)
    {
        int count = XkbCommon.KeymapKeyGetSymsByLevel(keymap, keyCode, SingleLayoutIndex, level, out uint* keysyms);

        if (count != 1)
        {
            return KeyToCharResult.Failure();
        }

        char ch = XkbKeysym.ToChar(keysyms[0]);
        return ch != '\0' ? KeyToCharResult.Success(ch, layoutId) : KeyToCharResult.Failure();
    }

    private IEnumerable<uint> ClosedRange(uint fromInclusive, uint toInclusive) =>
        Enumerable.Range((int)fromInclusive, (int)(toInclusive - fromInclusive + 1)).Select(value => (uint)value);
}
