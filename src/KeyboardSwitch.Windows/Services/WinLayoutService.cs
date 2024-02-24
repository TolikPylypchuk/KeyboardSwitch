namespace KeyboardSwitch.Windows.Services;

using System.ComponentModel;
using System.Globalization;

internal sealed class WinLayoutService(ILogger<WinLayoutService> logger) : CachingLayoutService
{
    private const string KeyboardLayoutsRegistryKey = @"SYSTEM\CurrentControlSet\Control\Keyboard Layouts";
    private const string KeyboardLayoutNameRegistryKeyFormat = KeyboardLayoutsRegistryKey + @"\{0}";
    private const string LayoutText = "Layout Text";

    private static readonly IntPtr HklNext = 1;
    private static readonly IntPtr HklPrev = 0;
    public const int KlNameLength = 9;

    public bool IsLoadingLayoutsSupported => true;

    public override KeyboardLayout GetCurrentKeyboardLayout()
    {
        logger.LogDebug("Getting the keyboard layout of the foreground process");
        uint foregroundWindowThreadId = User32.GetWindowThreadProcessId(User32.GetForegroundWindow(), out _);
        return this.GetThreadKeyboardLayout(foregroundWindowThreadId);
    }

    public override void SwitchCurrentLayout(SwitchDirection direction, SwitchSettings settings)
    {
        logger.LogDebug(
            "Switching the keyboard layout of the foregound process {Direction}", direction.AsString());

        var foregroundWindowHandle = User32.GetForegroundWindow();
        uint foregroundWindowThreadId = User32.GetWindowThreadProcessId(foregroundWindowHandle, out uint _);

        var keyboardLayoutId = User32.GetKeyboardLayout(foregroundWindowThreadId);

        SetThreadKeyboardLayout(keyboardLayoutId);
        SetThreadKeyboardLayout(direction == SwitchDirection.Forward ? HklNext : HklPrev);

        bool success = User32.PostMessage(
            foregroundWindowHandle,
            (uint)User32.WindowMessage.WM_INPUTLANGCHANGEREQUEST,
            IntPtr.Zero,
            User32.GetKeyboardLayout(0).DangerousGetHandle());

        if (success)
        {
            logger.LogDebug("Posted the input language change message to the foreground window");
        } else
        {
            logger.LogError("Failed to post the input language change message to the foreground window");
        }
    }

    protected override List<KeyboardLayout> GetKeyboardLayoutsInternal()
    {
        logger.LogDebug("Getting the list of installed keyboard layouts");

        int count = User32.GetKeyboardLayoutList(0, null);
        var keyboardLayoutIds = new User32.HKL[count];

        int result = User32.GetKeyboardLayoutList(keyboardLayoutIds.Length, keyboardLayoutIds);

        if (result == 0)
        {
            logger.LogCritical("Could not get the list of installed keyboard layouts");
            throw new Win32Exception(result);
        }

        return keyboardLayoutIds
            .Select(this.CreateKeyboardLayout)
            .ToList();
    }

    private KeyboardLayout GetThreadKeyboardLayout(uint threadId) =>
        this.CreateKeyboardLayout(User32.GetKeyboardLayout(threadId));

    private void SetThreadKeyboardLayout(User32.HKL keyboardLayoutId) =>
        User32.ActivateKeyboardLayout(keyboardLayoutId, 0);

    private KeyboardLayout CreateKeyboardLayout(User32.HKL keyboardLayoutId)
    {
        int id = (int)keyboardLayoutId.DangerousGetHandle();
        var (name, tag) = this.GetLayoutDisplayNameAndTag(keyboardLayoutId);

        return new(id.ToString(), this.GetCultureInfo(id, name).EnglishName, name, tag);
    }

    private (string DisplayName, string Tag) GetLayoutDisplayNameAndTag(User32.HKL keyboardLayoutId)
    {
        var currentLayout = User32.GetKeyboardLayout(0);

        SetThreadKeyboardLayout(keyboardLayoutId);
        string name = this.GetCurrentLayoutName();

        SetThreadKeyboardLayout(currentLayout);

        using var key = Registry.LocalMachine.OpenSubKey(String.Format(
            CultureInfo.InvariantCulture, KeyboardLayoutNameRegistryKeyFormat, name));

        return (key?.GetValue(LayoutText)?.ToString() ?? String.Empty, name);
    }

    private string GetCurrentLayoutName()
    {
        var name = new StringBuilder(KlNameLength);
        User32.GetKeyboardLayoutName(name);
        return name.ToString();
    }

    private CultureInfo GetCultureInfo(int keyboardLayoutId, string layoutName)
    {
        int lcid = keyboardLayoutId & 0xFFFF;

        try
        {
            return CultureInfo.GetCultureInfo(lcid);
        } catch (CultureNotFoundException e)
        {
            logger.LogError(e, "Did not find the culture for layout: {Layout} (LCID {Lcid})", layoutName, lcid);
            return CultureInfo.InvariantCulture;
        }
    }
}
