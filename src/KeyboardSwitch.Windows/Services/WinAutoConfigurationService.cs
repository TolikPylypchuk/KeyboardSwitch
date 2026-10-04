using System.Collections.Immutable;

namespace KeyboardSwitch.Windows.Services;

internal sealed class WinAutoConfigurationService : AutoConfigurationServiceBase
{
    private const uint NoKeyboardStateModification = 1 << 2;
    private const int KeyStatePressed = 1 << 7;

    private const int ResultSuccess = 1;
    private const int ResultDeadKey = -1;

    private static readonly ImmutableList<VIRTUAL_KEY> KeyCodesToMap =
    [
        VIRTUAL_KEY.VK_Q,
        VIRTUAL_KEY.VK_W,
        VIRTUAL_KEY.VK_E,
        VIRTUAL_KEY.VK_R,
        VIRTUAL_KEY.VK_T,
        VIRTUAL_KEY.VK_Y,
        VIRTUAL_KEY.VK_U,
        VIRTUAL_KEY.VK_I,
        VIRTUAL_KEY.VK_O,
        VIRTUAL_KEY.VK_P,
        VIRTUAL_KEY.VK_OEM_4,
        VIRTUAL_KEY.VK_OEM_6,
        VIRTUAL_KEY.VK_A,
        VIRTUAL_KEY.VK_S,
        VIRTUAL_KEY.VK_D,
        VIRTUAL_KEY.VK_F,
        VIRTUAL_KEY.VK_G,
        VIRTUAL_KEY.VK_H,
        VIRTUAL_KEY.VK_J,
        VIRTUAL_KEY.VK_K,
        VIRTUAL_KEY.VK_L,
        VIRTUAL_KEY.VK_OEM_1,
        VIRTUAL_KEY.VK_OEM_7,
        VIRTUAL_KEY.VK_Z,
        VIRTUAL_KEY.VK_X,
        VIRTUAL_KEY.VK_C,
        VIRTUAL_KEY.VK_V,
        VIRTUAL_KEY.VK_B,
        VIRTUAL_KEY.VK_N,
        VIRTUAL_KEY.VK_M,
        VIRTUAL_KEY.VK_OEM_COMMA,
        VIRTUAL_KEY.VK_OEM_PERIOD,
        VIRTUAL_KEY.VK_OEM_2,
        VIRTUAL_KEY.VK_OEM_5,
        VIRTUAL_KEY.VK_OEM_3,
        VIRTUAL_KEY.VK_1,
        VIRTUAL_KEY.VK_2,
        VIRTUAL_KEY.VK_3,
        VIRTUAL_KEY.VK_4,
        VIRTUAL_KEY.VK_5,
        VIRTUAL_KEY.VK_6,
        VIRTUAL_KEY.VK_7,
        VIRTUAL_KEY.VK_8,
        VIRTUAL_KEY.VK_9,
        VIRTUAL_KEY.VK_0,
        VIRTUAL_KEY.VK_OEM_MINUS,
        VIRTUAL_KEY.VK_OEM_PLUS
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

    private List<KeyToCharResult> GetCharsFromKey(
        VIRTUAL_KEY keyCode,
        bool shift,
        bool altGr,
        List<string> layoutIds) =>
        layoutIds
            .Select(layoutId => this.GetCharFromKey(keyCode, shift, altGr, layoutId))
            .ToList();

    private KeyToCharResult GetCharFromKey(VIRTUAL_KEY keyCode, bool shift, bool altGr, string layoutId)
    {
        const int bufferSize = 256;

        Span<char> buffer = stackalloc char[bufferSize];
        Span<byte> keyboardState = stackalloc byte[bufferSize];

        if (shift)
        {
            keyboardState[(int)VIRTUAL_KEY.VK_SHIFT] = KeyStatePressed;
        }

        if (altGr)
        {
            keyboardState[(int)VIRTUAL_KEY.VK_CONTROL] = KeyStatePressed;
            keyboardState[(int)VIRTUAL_KEY.VK_MENU] = KeyStatePressed;
        }

        uint scanCode = MapToScanCode(keyCode);
        var hkl = (HKL)Int32.Parse(layoutId);
        int result = PInvoke.ToUnicodeEx(
            (uint)keyCode, scanCode, keyboardState, buffer, NoKeyboardStateModification, hkl);

        if (result == ResultDeadKey)
        {
            result = PInvoke.ToUnicodeEx(
                (uint)VIRTUAL_KEY.VK_SPACE,
                MapToScanCode(VIRTUAL_KEY.VK_SPACE),
                keyboardState,
                buffer,
                NoKeyboardStateModification,
                hkl);
        }

        return result switch
        {
            ResultSuccess => KeyToCharResult.Success(buffer[0], layoutId),
            _ => KeyToCharResult.Failure()
        };
    }

    private uint MapToScanCode(VIRTUAL_KEY keyCode) =>
        PInvoke.MapVirtualKey((uint)keyCode, MAP_VIRTUAL_KEY_TYPE.MAPVK_VK_TO_VSC_EX);
}
