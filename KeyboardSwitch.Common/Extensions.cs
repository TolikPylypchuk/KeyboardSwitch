using System;
using System.Collections.Generic;
using System.Linq;

namespace KeyboardSwitch.Common
{
    public static class Extensions
    {
        public static string ToFormattedString(this ModifierKeys modifiers)
            => Enum.GetValues(typeof(ModifierKeys))
                .Cast<ModifierKeys>()
                .Where(modifier => modifiers.HasFlag(modifier))
                .Select(modifier => modifier switch
                {
                    ModifierKeys.Alt => nameof(ModifierKeys.Alt),
                    ModifierKeys.Ctrl => nameof(ModifierKeys.Ctrl),
                    ModifierKeys.Shift => nameof(ModifierKeys.Shift),
                    ModifierKeys.Win => nameof(ModifierKeys.Win),
                    ModifierKeys.NoRepeat => nameof(ModifierKeys.NoRepeat),
                    _ => String.Empty
                })
                .Aggregate(
                    String.Empty,
                    (acc, item) => $"{acc}+{item}",
                    result => result.TrimStart('+').Replace("++", "+"));

        public static IEnumerable<T?> AsNullable<T>(this IEnumerable<T> items)
            where T : struct
            => items.Select(item => (T?)item);
    }
}
