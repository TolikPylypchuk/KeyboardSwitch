using System;
using System.Collections.Generic;
using System.Linq;

namespace KeyboardSwitch.Common.Keyboard
{
    public static class Extensions
    {
        public static ModifierKey Flatten(this IEnumerable<ModifierKey> keys) =>
            keys.Aggregate(ModifierKey.None, (acc, key) => acc | key);

        public static string ToFormattedString(this IEnumerable<ModifierKey> modifiers) =>
            modifiers
                .Select(Enum.GetName)
                .Aggregate(
                    String.Empty,
                    (acc, item) => $"{acc}+{item}",
                    result => result.TrimStart('+').Replace("++", "+", StringComparison.InvariantCulture));

    }
}
