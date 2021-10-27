using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KeyboardSwitch.Core.Services.AutoConfiguration;

using SharpHook.Native;

using static Vanara.PInvoke.User32;

namespace KeyboardSwitch.Windows.Services
{
    internal class WinAutoConfigurationService : AutoConfigurationServiceBase
    {
        private const int VkShift = 0x10;
        private const int VkCtrl = 0x11;
        private const int VkAlt = 0x12;
        private const int Pressed = 0x80;

        private const int ResultSuccess = 1;
        private const int ResultDeadKey = -1;

        private static readonly List<KeyCode> KeyCodesToMap = new()
        {
            KeyCode.VcQ,
            KeyCode.VcW,
            KeyCode.VcE,
            KeyCode.VcR,
            KeyCode.VcT,
            KeyCode.VcY,
            KeyCode.VcU,
            KeyCode.VcI,
            KeyCode.VcO,
            KeyCode.VcP,
            KeyCode.VcOpenBracket,
            KeyCode.VcCloseBracket,
            KeyCode.VcA,
            KeyCode.VcS,
            KeyCode.VcD,
            KeyCode.VcF,
            KeyCode.VcG,
            KeyCode.VcH,
            KeyCode.VcJ,
            KeyCode.VcK,
            KeyCode.VcL,
            KeyCode.VcSemicolon,
            KeyCode.VcQuote,
            KeyCode.VcZ,
            KeyCode.VcX,
            KeyCode.VcC,
            KeyCode.VcV,
            KeyCode.VcB,
            KeyCode.VcN,
            KeyCode.VcM,
            KeyCode.VcComma,
            KeyCode.VcPeriod,
            KeyCode.VcSlash,
            KeyCode.VcBackSlash,
            KeyCode.VcBackquote,
            KeyCode.Vc1,
            KeyCode.Vc2,
            KeyCode.Vc3,
            KeyCode.Vc4,
            KeyCode.Vc5,
            KeyCode.Vc6,
            KeyCode.Vc7,
            KeyCode.Vc8,
            KeyCode.Vc9,
            KeyCode.Vc0,
            KeyCode.VcMinus,
            KeyCode.VcEquals
        };

        protected override IEnumerable<List<KeyToCharResult>> GetChars(List<string> layoutIds) =>
            KeyCodesToMap
                .Select(keyCode => this.GetCharsFromKey(keyCode, shift: false, altGr: false, layoutIds))
                .Concat(KeyCodesToMap.Select(keyCode =>
                    this.GetCharsFromKey(keyCode, shift: true, altGr: false, layoutIds)))
                .Concat(KeyCodesToMap.Select(keyCode =>
                    this.GetCharsFromKey(keyCode, shift: false, altGr: true, layoutIds)))
                .Concat(KeyCodesToMap.Select(keyCode =>
                    this.GetCharsFromKey(keyCode, shift: true, altGr: true, layoutIds)));

        private List<KeyToCharResult> GetCharsFromKey(KeyCode keyCode, bool shift, bool altGr, List<string> layoutIds) =>
            layoutIds
                .Select(layoutId => this.GetCharFromKey(keyCode, shift, altGr, layoutId))
                .ToList();

        private KeyToCharResult GetCharFromKey(KeyCode keyCode, bool shift, bool altGr, string layoutId)
        {
            const int bufferSize = 256;

            var buffer = new StringBuilder(bufferSize);
            var keyboardState = new byte[bufferSize];

            if (shift)
            {
                keyboardState[VkShift] = Pressed;
            }

            if (altGr)
            {
                keyboardState[VkCtrl] = Pressed;
                keyboardState[VkAlt] = Pressed;
            }

            uint virtualKeyCode = MapToVirtualKey(keyCode);

            if (virtualKeyCode == 0)
            {
                return KeyToCharResult.Failure();
            }

            var hkl = (HKL)(IntPtr)Int32.Parse(layoutId);
            int result = ToUnicodeEx(virtualKeyCode, (uint)keyCode, keyboardState, buffer, bufferSize, 0, hkl);

            if (result == ResultDeadKey)
            {
                result = ToUnicodeEx(
                    MapToVirtualKey(KeyCode.VcSpace),
                    (uint)KeyCode.VcSpace,
                    keyboardState,
                    buffer,
                    bufferSize,
                    0,
                    hkl);
            }

            return result switch
            {
                ResultSuccess => KeyToCharResult.Success(buffer.ToString()[0], layoutId),
                _ => KeyToCharResult.Failure()
            };
        }

        private uint MapToVirtualKey(KeyCode keyCode) =>
            MapVirtualKey((uint)keyCode, MAPVK.MAPVK_VSC_TO_VK_EX);
    }
}
