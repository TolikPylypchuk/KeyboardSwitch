using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using KeyboardSwitch.Common.Services;

namespace KeyboardSwitch.Common
{
    public static class Extensions
    {
        public static string? NullIfEmpty(this string? str)
            => String.IsNullOrEmpty(str) ? null : str;

        public static string EmptyIfNull(this string? str)
            => String.IsNullOrEmpty(str) ? String.Empty : str;

        public static string GetMemberName(this Expression expression)
            => expression is LambdaExpression lambda && lambda.Body is MemberExpression member
                ? member.Member.Name
                : throw new NotSupportedException("Non-lambda expressions with member access are not supported.");

        public static IEnumerable<T?> AsNullable<T>(this IEnumerable<T> items)
            where T : struct
            => items.Select(item => (T?)item);

        public static string AsString(this SwitchDirection direction)
            => direction == SwitchDirection.Forward ? "forward" : "backward";

        public static IEnumerable<IEnumerable<T>> GetPowerSet<T>(this IList<T> list)
            => from bit in Enumerable.Range(0, 1 << list.Count)
                select
                    from index in Enumerable.Range(0, list.Count)
                    where (bit & (1 << index)) != 0
                    select list[index];
    }
}
