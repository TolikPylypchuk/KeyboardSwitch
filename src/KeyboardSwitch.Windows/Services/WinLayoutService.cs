using System.ComponentModel;
using System.Globalization;

namespace KeyboardSwitch.Windows.Services;

internal sealed partial class WinLayoutService(ILogger<WinLayoutService> logger) : CachingLayoutService
{
    private const string KeyboardLayoutsRegistryKey = @"SYSTEM\CurrentControlSet\Control\Keyboard Layouts";
    private const string KeyboardLayoutNameRegistryKeyFormat = KeyboardLayoutsRegistryKey + @"\{0}";
    private const string LayoutText = "Layout Text";

    private static readonly HKL HklNext = (HKL)(nint)1;
    private static readonly HKL HklPrev = (HKL)(nint)0;
    public const int KlNameLength = 9;

    public bool IsLoadingLayoutsSupported => true;

    public override Task<KeyboardLayout> GetCurrentKeyboardLayout()
    {
        this.LogGettingLayoutOfForegroundProcess();
        uint foregroundWindowThreadId = PInvoke.GetWindowThreadProcessId(PInvoke.GetForegroundWindow(), out _);
        return Task.FromResult(this.GetThreadKeyboardLayout(foregroundWindowThreadId));
    }

    public override Task SwitchCurrentLayout(SwitchDirection direction, SwitchSettings settings)
    {
        this.LogSwitchingLayoutOfForegroundProcess(direction);

        var foregroundWindowHandle = PInvoke.GetForegroundWindow();
        uint foregroundWindowThreadId = PInvoke.GetWindowThreadProcessId(foregroundWindowHandle, out uint _);

        var keyboardLayoutId = PInvoke.GetKeyboardLayout(foregroundWindowThreadId);

        SetThreadKeyboardLayout(keyboardLayoutId);
        SetThreadKeyboardLayout(direction == SwitchDirection.Forward ? HklNext : HklPrev);

        bool success = PInvoke.PostMessage(
            foregroundWindowHandle,
            PInvoke.WM_INPUTLANGCHANGEREQUEST,
            default,
            (nint)PInvoke.GetKeyboardLayout(0));

        if (success)
        {
            this.LogPostedMessageToForegroundWindow();
        } else
        {
            this.LogFailedToPostMessageToForegroundWindow();
        }

        return Task.CompletedTask;
    }

    protected override Task<List<KeyboardLayout>> GetKeyboardLayoutsInternal()
    {
        this.LogGettingListOfInstalledLayouts();

        int count = PInvoke.GetKeyboardLayoutList([]);
        var keyboardLayoutIds = new HKL[count];

        int result = PInvoke.GetKeyboardLayoutList(keyboardLayoutIds);

        if (result == 0)
        {
            this.LogCouldNotGetListOfInstalledLayouts();
            throw new Win32Exception(result);
        }

        return Task.FromResult(keyboardLayoutIds
            .Select(this.CreateKeyboardLayout)
            .ToList());
    }

    private KeyboardLayout GetThreadKeyboardLayout(uint threadId) =>
        this.CreateKeyboardLayout(PInvoke.GetKeyboardLayout(threadId));

    private void SetThreadKeyboardLayout(HKL keyboardLayoutId) =>
        PInvoke.ActivateKeyboardLayout(keyboardLayoutId, 0);

    private KeyboardLayout CreateKeyboardLayout(HKL keyboardLayoutId)
    {
        int id = (int)(nint)keyboardLayoutId;
        var (name, tag) = this.GetLayoutDisplayNameAndTag(keyboardLayoutId);

        return new(id.ToString(), this.GetCultureInfo(id, name).EnglishName, name, tag);
    }

    private (string DisplayName, string Tag) GetLayoutDisplayNameAndTag(HKL keyboardLayoutId)
    {
        var currentLayout = PInvoke.GetKeyboardLayout(0);

        SetThreadKeyboardLayout(keyboardLayoutId);
        string name = this.GetCurrentLayoutName();

        SetThreadKeyboardLayout(currentLayout);

        using var key = Registry.LocalMachine.OpenSubKey(String.Format(
            CultureInfo.InvariantCulture, KeyboardLayoutNameRegistryKeyFormat, name));

        return (key?.GetValue(LayoutText)?.ToString() ?? String.Empty, name);
    }

    private string GetCurrentLayoutName()
    {
        Span<char> name = stackalloc char[KlNameLength];
        PInvoke.GetKeyboardLayoutName(name);
        return name.TrimEnd('\0').ToString();
    }

    private CultureInfo GetCultureInfo(int keyboardLayoutId, string layoutName)
    {
        int lcid = keyboardLayoutId & 0xFFFF;

        try
        {
            return CultureInfo.GetCultureInfo(lcid);
        } catch (CultureNotFoundException e)
        {
            this.LogDidNotFindCultureForLayout(layoutName, lcid, e);
            return CultureInfo.InvariantCulture;
        }
    }

    [LoggerMessage(LogLevel.Debug, "Getting the keyboard layout of the foreground process")]
    private partial void LogGettingLayoutOfForegroundProcess();

    [LoggerMessage(LogLevel.Debug, "Switching the keyboard layout of the foregound process: {Direction}")]
    private partial void LogSwitchingLayoutOfForegroundProcess(SwitchDirection direction);

    [LoggerMessage(LogLevel.Debug, "Posted the input language change message to the foreground window")]
    private partial void LogPostedMessageToForegroundWindow();

    [LoggerMessage(LogLevel.Error, "Failed to post the input language change message to the foreground window")]
    private partial void LogFailedToPostMessageToForegroundWindow();

    [LoggerMessage(LogLevel.Debug, "Getting the list of installed keyboard layouts")]
    private partial void LogGettingListOfInstalledLayouts();

    [LoggerMessage(LogLevel.Critical, "Could not get the list of installed keyboard layouts")]
    private partial void LogCouldNotGetListOfInstalledLayouts();

    [LoggerMessage(LogLevel.Error, "Did not find the culture for layout: {Layout} (LCID {Lcid})")]
    private partial void LogDidNotFindCultureForLayout(string layout, int lcid, Exception e);
}
