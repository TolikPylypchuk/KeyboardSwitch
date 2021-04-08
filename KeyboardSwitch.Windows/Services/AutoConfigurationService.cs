using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

using KeyboardSwitch.Core.Keyboard;
using KeyboardSwitch.Core.Services;

using static Vanara.PInvoke.User32;

namespace KeyboardSwitch.Windows.Services
{
    internal class AutoConfigurationService : IAutoConfigurationService
    {
        private enum KeyToCharResultTag { Success, DeadKey, NotMapped, MultipleChars }

        private struct KeyToCharResult
        {
            private const char NullChar = '\0';

            private KeyToCharResult(KeyToCharResultTag tag, char ch, HKL layoutId)
            {
                this.Tag = tag;
                this.Char = ch;
                this.LayoutId = layoutId;
            }

            public KeyToCharResultTag Tag { get; }
            public char Char { get; }
            public HKL LayoutId { get; }

            public static KeyToCharResult Success(char ch, HKL layoutId) =>
                new(KeyToCharResultTag.Success, ch, layoutId);

            public static KeyToCharResult DeadKey() =>
                new(KeyToCharResultTag.DeadKey, NullChar, HKL.NULL);

            public static KeyToCharResult NotMapped() =>
                new(KeyToCharResultTag.NotMapped, NullChar, HKL.NULL);

            public static KeyToCharResult MultipleChars() =>
                new(KeyToCharResultTag.MultipleChars, NullChar, HKL.NULL);
        }

        private struct DistinctCharsState
        {
            public DistinctCharsState(
                ImmutableList<List<KeyToCharResult>> results,
                ImmutableDictionary<HKL, ImmutableList<char>> processedChars)
            {
                this.Results = results;
                this.DistinctChars = processedChars;
            }

            public static DistinctCharsState Initial(List<HKL> layoutIds) =>
                new(
                    ImmutableList<List<KeyToCharResult>>.Empty,
                    layoutIds.ToImmutableDictionary(layoutId => layoutId, _ => ImmutableList<char>.Empty));

            public ImmutableList<List<KeyToCharResult>> Results { get; }
            public ImmutableDictionary<HKL, ImmutableList<char>> DistinctChars { get; }
        }

        private const int VkShift = 0x10;
        private const int VkCtrl = 0x11;
        private const int VkAlt = 0x12;
        private const int Pressed = 0x80;

        private const int ResultSuccess = 1;
        private const int ResultNotMapped = 0;
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

        public Dictionary<string, string> CreateCharMappings(IEnumerable<KeyboardLayout> layouts)
        {
            var layoutIds = layouts
                .Select(layout => (IntPtr)Int32.Parse(layout.Id))
                .Select(id => (HKL)id)
                .ToList();

            return KeyCodesToMap
                .Select(keyCode => this.GetCharsFromKey(keyCode, shift: false, altGr: false, layoutIds))
                .Concat(KeyCodesToMap.Select(keyCode =>
                    this.GetCharsFromKey(keyCode, shift: true, altGr: false, layoutIds)))
                .Concat(KeyCodesToMap.Select(keyCode =>
                    this.GetCharsFromKey(keyCode, shift: false, altGr: true, layoutIds)))
                .Concat(KeyCodesToMap.Select(keyCode =>
                    this.GetCharsFromKey(keyCode, shift: true, altGr: true, layoutIds)))
                .Where(results => results.All(result => result.Tag == KeyToCharResultTag.Success))
                .Aggregate(DistinctCharsState.Initial(layoutIds), this.RemoveDuplicateChars)
                .Results
                .SelectMany(results => results)
                .GroupBy(result => (int)result.LayoutId.DangerousGetHandle(), result => result.Char)
                .ToDictionary(result => result.Key.ToString(), result => new string(result.ToArray()));
        }

        private DistinctCharsState RemoveDuplicateChars(DistinctCharsState state, List<KeyToCharResult> results) =>
            results.Any(result => state.DistinctChars[result.LayoutId].Contains(result.Char))
                ? state
                : new(
                    state.Results.Add(results),
                    results.ToImmutableDictionary(
                        result => result.LayoutId,
                        result => state.DistinctChars[result.LayoutId].Add(result.Char)));

        private List<KeyToCharResult> GetCharsFromKey(KeyCode keyCode, bool shift, bool altGr, List<HKL> layoutIds) =>
            layoutIds
                .Select(layoutId => this.GetCharFromKey(keyCode, shift, altGr, layoutId))
                .ToList();

        private KeyToCharResult GetCharFromKey(KeyCode keyCode, bool shift, bool altGr, HKL layoutId)
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
                return KeyToCharResult.NotMapped();
            }

            int result = ToUnicodeEx(virtualKeyCode, (uint)keyCode, keyboardState, buffer, bufferSize, 0, layoutId);

            if (result == ResultDeadKey)
            {
                result = ToUnicodeEx(
                    MapToVirtualKey(KeyCode.VcSpace),
                    (uint)KeyCode.VcSpace,
                    keyboardState,
                    buffer,
                    bufferSize,
                    0,
                    layoutId);
            }

            return result switch
            {
                ResultSuccess => KeyToCharResult.Success(buffer.ToString()[0], layoutId),
                ResultNotMapped => KeyToCharResult.NotMapped(),
                ResultDeadKey => KeyToCharResult.DeadKey(),
                _ => KeyToCharResult.MultipleChars()
            };
        }

        private uint MapToVirtualKey(KeyCode keyCode) =>
            MapVirtualKey((uint)keyCode, MAPVK.MAPVK_VSC_TO_VK_EX);
    }
}
