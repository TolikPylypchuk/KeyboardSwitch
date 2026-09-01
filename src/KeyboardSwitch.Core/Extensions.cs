using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace KeyboardSwitch.Core;

public static class Extensions
{
    extension(string? str)
    {
        public string? NullIfEmpty() =>
            String.IsNullOrEmpty(str) ? null : str;

        public string EmptyIfNull() =>
            String.IsNullOrEmpty(str) ? String.Empty : str;
    }

    extension(Expression expression)
    {
        public string GetMemberName() =>
            expression is LambdaExpression lambda && lambda.Body is MemberExpression member
                ? member.Member.Name
                : throw new NotSupportedException("Non-lambda expressions with member access are not supported");
    }

    extension<T>(IEnumerable<T> items)
        where T : struct
    {
        public IEnumerable<T?> AsNullable() =>
            items.Select(item => (T?)item);
    }

    extension<T>(IList<T> list)
    {
        public IEnumerable<IEnumerable<T>> GetPowerSet() =>
            from bit in Enumerable.Range(0, 1 << list.Count)
            select
                from index in Enumerable.Range(0, list.Count)
                where (bit & (1 << index)) != 0
                select list[index];
    }

    extension(Uri uri)
    {
        [ExcludeFromCodeCoverage]
        public void OpenInBrowser() =>
            PlatformDependent(
                windows: () => Process.Start(
                    new ProcessStartInfo { FileName = uri.ToString(), UseShellExecute = true }),
                macos: () => Process.Start("open", uri.ToString()),
                linux: () => Process.Start("xdg-open", uri.ToString()));
    }
}
