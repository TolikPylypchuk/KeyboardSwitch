using System.Collections.Immutable;

namespace KeyboardSwitch.Windows.Services;

internal class WinAutoConfigurationService : AutoConfigurationServiceBase
{
    private const uint NoKeyboardStateModification = 1 << 2;
    private const int KeyStatePressed = 1 << 7;

    private const int ResultSuccess = 1;
    private const int ResultDeadKey = -1;

    private static readonly ImmutableList<User32.VK> KeyCodesToMap =
    [
        User32.VK.VK_Q,
        User32.VK.VK_W,
        User32.VK.VK_E,
        User32.VK.VK_R,
        User32.VK.VK_T,
        User32.VK.VK_Y,
        User32.VK.VK_U,
        User32.VK.VK_I,
        User32.VK.VK_O,
        User32.VK.VK_P,
        User32.VK.VK_OEM_4,
        User32.VK.VK_OEM_6,
        User32.VK.VK_A,
        User32.VK.VK_S,
        User32.VK.VK_D,
        User32.VK.VK_F,
        User32.VK.VK_G,
        User32.VK.VK_H,
        User32.VK.VK_J,
        User32.VK.VK_K,
        User32.VK.VK_L,
        User32.VK.VK_OEM_1,
        User32.VK.VK_OEM_7,
        User32.VK.VK_Z,
        User32.VK.VK_X,
        User32.VK.VK_C,
        User32.VK.VK_V,
        User32.VK.VK_B,
        User32.VK.VK_N,
        User32.VK.VK_M,
        User32.VK.VK_OEM_COMMA,
        User32.VK.VK_OEM_PERIOD,
        User32.VK.VK_OEM_2,
        User32.VK.VK_OEM_5,
        User32.VK.VK_OEM_3,
        User32.VK.VK_1,
        User32.VK.VK_2,
        User32.VK.VK_3,
        User32.VK.VK_4,
        User32.VK.VK_5,
        User32.VK.VK_6,
        User32.VK.VK_7,
        User32.VK.VK_8,
        User32.VK.VK_9,
        User32.VK.VK_0,
        User32.VK.VK_OEM_MINUS,
        User32.VK.VK_OEM_PLUS
    ];

    protected override IEnumerable<List<KeyToCharResult>> GetChars(List<string> layoutIds) =>
        KeyCodesToMap
            .Select(keyCode => this.GetCharsFromKey(keyCode, shift: false, altGr: false, layoutIds))
            .Concat(KeyCodesToMap.Select(keyCode =>
                this.GetCharsFromKey(keyCode, shift: true, altGr: false, layoutIds)))
            .Concat(KeyCodesToMap.Select(keyCode =>
                this.GetCharsFromKey(keyCode, shift: false, altGr: true, layoutIds)))
            .Concat(KeyCodesToMap.Select(keyCode =>
                this.GetCharsFromKey(keyCode, shift: true, altGr: true, layoutIds)));

    private List<KeyToCharResult> GetCharsFromKey(User32.VK keyCode, bool shift, bool altGr, List<string> layoutIds) =>
        layoutIds
            .Select(layoutId => this.GetCharFromKey(keyCode, shift, altGr, layoutId))
            .ToList();

    private KeyToCharResult GetCharFromKey(User32.VK keyCode, bool shift, bool altGr, string layoutId)
    {
        const int bufferSize = 256;

        var buffer = new StringBuilder(bufferSize);
        var keyboardState = new byte[bufferSize];

        if (shift)
        {
            keyboardState[(int)User32.VK.VK_SHIFT] = KeyStatePressed;
        }

        if (altGr)
        {
            keyboardState[(int)User32.VK.VK_CONTROL] = KeyStatePressed;
            keyboardState[(int)User32.VK.VK_MENU] = KeyStatePressed;
        }

        uint scanCode = MapToScanCode(keyCode);
        var hkl = (User32.HKL)Int32.Parse(layoutId);
        int result = User32.ToUnicodeEx(
            (uint)keyCode, scanCode, keyboardState, buffer, bufferSize, NoKeyboardStateModification, hkl);

        if (result == ResultDeadKey)
        {
            result = User32.ToUnicodeEx(
                (uint)User32.VK.VK_SPACE,
                MapToScanCode(User32.VK.VK_SPACE),
                keyboardState,
                buffer,
                bufferSize,
                NoKeyboardStateModification,
                hkl);
        }

        return result switch
        {
            ResultSuccess => KeyToCharResult.Success(buffer.ToString()[0], layoutId),
            _ => KeyToCharResult.Failure()
        };
    }

    private uint MapToScanCode(User32.VK keyCode) =>
        User32.MapVirtualKey((uint)keyCode, User32.MAPVK.MAPVK_VK_TO_VSC_EX);
}
