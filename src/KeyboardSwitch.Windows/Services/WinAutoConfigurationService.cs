namespace KeyboardSwitch.Windows.Services;

using System.Collections.Immutable;

internal class WinAutoConfigurationService : AutoConfigurationServiceBase
{
    private const uint NoKeyboardStateModification = 1 << 2;
    private const int KeyStatePressed = 1 << 7;

    private const int ResultSuccess = 1;
    private const int ResultDeadKey = -1;

    private static readonly ImmutableList<VK> KeyCodesToMap =
    [
        VK.VK_Q,
        VK.VK_W,
        VK.VK_E,
        VK.VK_R,
        VK.VK_T,
        VK.VK_Y,
        VK.VK_U,
        VK.VK_I,
        VK.VK_O,
        VK.VK_P,
        VK.VK_OEM_4,
        VK.VK_OEM_6,
        VK.VK_A,
        VK.VK_S,
        VK.VK_D,
        VK.VK_F,
        VK.VK_G,
        VK.VK_H,
        VK.VK_J,
        VK.VK_K,
        VK.VK_L,
        VK.VK_OEM_1,
        VK.VK_OEM_7,
        VK.VK_Z,
        VK.VK_X,
        VK.VK_C,
        VK.VK_V,
        VK.VK_B,
        VK.VK_N,
        VK.VK_M,
        VK.VK_OEM_COMMA,
        VK.VK_OEM_PERIOD,
        VK.VK_OEM_2,
        VK.VK_OEM_5,
        VK.VK_OEM_3,
        VK.VK_1,
        VK.VK_2,
        VK.VK_3,
        VK.VK_4,
        VK.VK_5,
        VK.VK_6,
        VK.VK_7,
        VK.VK_8,
        VK.VK_9,
        VK.VK_0,
        VK.VK_OEM_MINUS,
        VK.VK_OEM_PLUS
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

    private List<KeyToCharResult> GetCharsFromKey(VK keyCode, bool shift, bool altGr, List<string> layoutIds) =>
        layoutIds
            .Select(layoutId => this.GetCharFromKey(keyCode, shift, altGr, layoutId))
            .ToList();

    private KeyToCharResult GetCharFromKey(VK keyCode, bool shift, bool altGr, string layoutId)
    {
        const int bufferSize = 256;

        var buffer = new StringBuilder(bufferSize);
        var keyboardState = new byte[bufferSize];

        if (shift)
        {
            keyboardState[(int)VK.VK_SHIFT] = KeyStatePressed;
        }

        if (altGr)
        {
            keyboardState[(int)VK.VK_CONTROL] = KeyStatePressed;
            keyboardState[(int)VK.VK_MENU] = KeyStatePressed;
        }

        uint scanCode = MapToScanCode(keyCode);
        var hkl = (HKL)Int32.Parse(layoutId);
        int result = ToUnicodeEx(
            (uint)keyCode, scanCode, keyboardState, buffer, bufferSize, NoKeyboardStateModification, hkl);

        if (result == ResultDeadKey)
        {
            result = ToUnicodeEx(
                (uint)VK.VK_SPACE,
                MapToScanCode(VK.VK_SPACE),
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

    private uint MapToScanCode(VK keyCode) =>
        MapVirtualKey((uint)keyCode, MAPVK.MAPVK_VK_TO_VSC_EX);
}
