namespace Core.Reports;

public static class EnumerableReportExtensions
{
    public static IEnumerable<T> TopBy<T>(
        this IEnumerable<T> source,
        Func<T, int> selector,
        int n)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentOutOfRangeException.ThrowIfNegative(n);

        return source.OrderByDescending(selector).Take(n);
    }

    public static IEnumerable<T> TopBy<T>(
        this IEnumerable<T> source,
        Func<T, int> selector,
        Func<T, string> tieBreaker,
        int n)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(tieBreaker);
        ArgumentOutOfRangeException.ThrowIfNegative(n);

        return source
            .OrderByDescending(selector)
            .ThenBy(tieBreaker, StringComparer.Ordinal)
            .Take(n);
    }
}
