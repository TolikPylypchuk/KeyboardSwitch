namespace KeyboardSwitch.Linux.Services;

public sealed class XAutoConfigurationService : AutoConfigurationServiceBase
{
    protected override IEnumerable<List<KeyToCharResult>> GetChars(List<string> layoutIds)
    {
        using var display = OpenXDisplay();

        var (minKeyCode, maxKeyCode) = GetMinAndMaxKeyCodes(display);

        return this.ClosedRange(0, XkbMaxShiftLevel)
            .SelectMany(keyCode => this.GetCharsForKey(display, keyCode, minKeyCode, maxKeyCode, layoutIds))
            .ToList();
    }

    private IEnumerable<List<KeyToCharResult>> GetCharsForKey(
        XDisplayHandle display,
        int level,
        byte minKeyCode,
        byte maxKeyCode,
        List<string> layoutIds) =>
        this.ClosedRange(minKeyCode, maxKeyCode)
            .Select(keyCode => this.GetCharsForKey(display, (byte)keyCode, level, layoutIds))
            .ToList();

    private List<KeyToCharResult> GetCharsForKey(
        XDisplayHandle display,
        byte keyCode,
        int level,
        List<string> layoutIds) =>
        layoutIds.Select((id, index) =>
        {
            var keysym = XkbKeycodeToKeysym(display, keyCode, index, level);
            char ch = keysym != XKeySym.NoSymbol ? keysym.ToChar() : '\0';
            return ch != '\0' ? KeyToCharResult.Success(ch, id) : KeyToCharResult.Failure();
        })
        .ToList();

    private IEnumerable<int> ClosedRange(int fromInclusive, int toInclusive) =>
        Enumerable.Range(fromInclusive, toInclusive - fromInclusive + 1);
}
