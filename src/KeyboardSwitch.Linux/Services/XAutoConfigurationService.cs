namespace KeyboardSwitch.Linux.Services;

internal sealed class XAutoConfigurationService(X11Service x11) : AutoConfigurationServiceBase
{
    protected override IEnumerable<List<KeyToCharResult>> GetChars(List<string> layoutIds)
    {
        var (minKeyCode, maxKeyCode) = this.GetMinAndMaxKeyCodes();

        return this.ClosedRange(0, XLib.XkbMaxShiftLevel)
            .SelectMany(keyCode => this.GetCharsForKey(keyCode, minKeyCode, maxKeyCode, layoutIds))
            .ToList();
    }

    public (byte Min, byte Max) GetMinAndMaxKeyCodes()
    {
        using var keyboardDescHandle = new XMapHandle(
            XLib.XkbGetMap(x11.Display, XkbMapComponentMask.XkbAllClientInfoMask, XkbKeyboardSpec.XkbUseCoreKbd),
            XkbMapComponentMask.XkbAllClientInfoMask);

        var keyboardDesc = Marshal.PtrToStructure<XkbDesc>(keyboardDescHandle.DangerousGetHandle());

        return (keyboardDesc.MinKeyCode, keyboardDesc.MaxKeyCode);
    }

    private List<List<KeyToCharResult>> GetCharsForKey(
        int level,
        byte minKeyCode,
        byte maxKeyCode,
        List<string> layoutIds) =>
        this.ClosedRange(minKeyCode, maxKeyCode)
            .Select(keyCode => this.GetCharsForKey((byte)keyCode, level, layoutIds))
            .ToList();

    private List<KeyToCharResult> GetCharsForKey(
        byte keyCode,
        int level,
        List<string> layoutIds) =>
        layoutIds.Select((id, index) =>
        {
            var keysym = XLib.XkbKeycodeToKeysym(x11.Display, keyCode, index, level);
            char ch = keysym != XKeySym.NoSymbol ? keysym.ToChar() : '\0';
            return ch != '\0' ? KeyToCharResult.Success(ch, id) : KeyToCharResult.Failure();
        })
        .ToList();

    private IEnumerable<int> ClosedRange(int fromInclusive, int toInclusive) =>
        Enumerable.Range(fromInclusive, toInclusive - fromInclusive + 1);
}
