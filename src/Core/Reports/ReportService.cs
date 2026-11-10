using Core.Domain;
using System.Diagnostics;

namespace Core.Reports;

public static class ReportService
{
    public static IReadOnlyList<StockLine> LowStock(IEnumerable<BookCopy> source, int threshold)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegative(threshold);

        return source
            .GroupBy(copy => copy.Isbn, StringComparer.OrdinalIgnoreCase)
            .Select(group => new StockLine(
                group.Key,
                group.First().Title,
                group.Count(copy => !copy.IsIssued),
                "прим."))
            .Where(line => line.Quantity < threshold)
            .OrderBy(line => line.Quantity)
            .ThenBy(line => line.Sku, StringComparer.Ordinal)
            .ToList();
    }

    public static IReadOnlyList<StockLine> TopByQuantity(IEnumerable<BookCopy> source, int n)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegative(n);

        return source
            .GroupBy(copy => copy.Isbn, StringComparer.OrdinalIgnoreCase)
            .Select(group => new StockLine(group.Key, group.First().Title, group.Count(), "прим."))
            .TopBy(line => line.Quantity, line => line.Sku, n)
            .ToList();
    }

    public static IReadOnlyList<GroupSummary> ByGenre(IEnumerable<BookCopy> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .GroupBy(copy => copy.Genre, StringComparer.OrdinalIgnoreCase)
            .Select(group => new GroupSummary(
                group.Key,
                group.Count(),
                group.Count(copy => !copy.IsIssued)))
            .OrderByDescending(summary => summary.TotalQuantity)
            .ThenBy(summary => summary.Key, StringComparer.Ordinal)
            .ToList();
    }

    public static IReadOnlyList<GroupSummary> TopGenres(IEnumerable<BookCopy> source, int n)
    {
        return ByGenre(source)
            .TopBy(summary => summary.TotalQuantity, summary => summary.Key, n)
            .ToList();
    }

    public static (int Items, int Total, double Average, int Max) Totals(IEnumerable<BookCopy> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        List<int> quantities = source
            .GroupBy(copy => copy.Isbn, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.Count(copy => !copy.IsIssued))
            .ToList();

        return quantities.Count == 0
            ? (0, 0, 0d, 0)
            : (quantities.Count, quantities.Sum(), quantities.Average(), quantities.Max());
    }

    public static IReadOnlyList<LoanSummary> LoansByBook(
        IEnumerable<BookCopy> copies,
        IEnumerable<Loan> loans)
    {
        ArgumentNullException.ThrowIfNull(copies);
        ArgumentNullException.ThrowIfNull(loans);

        return copies
            .GroupJoin(
                loans,
                copy => copy.Id,
                loan => loan.BookCopyId,
                (copy, copyLoans) => new LoanSummary(
                    copy.Id,
                    copy.Title,
                    copyLoans.Count(),
                    copyLoans.Count(loan => loan.IsOpen)))
            .OrderByDescending(summary => summary.LoanCount)
            .ThenBy(summary => summary.Sku, StringComparer.Ordinal)
            .ToList();
    }

    public static IReadOnlyList<GroupSummary> ByGenreQuerySyntax(IEnumerable<BookCopy> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var query =
            from copy in source
            group copy by copy.Genre into genreCopies
            let available = genreCopies.Count(copy => !copy.IsIssued)
            orderby available descending, genreCopies.Key
            select new GroupSummary(genreCopies.Key, genreCopies.Count(), available);

        return query.ToList();
    }

    public static (int Before, int After, int Snapshot) DeferredExecutionDemo(IEnumerable<BookCopy> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        List<BookCopy> localSource = source.ToList();
        IEnumerable<BookCopy> query = localSource.Where(copy => !copy.IsIssued);
        int before = query.Count();

        localSource.Add(BookCopy.Create("DEMO-001", "978-demo", "Демонстрація", 2026, "демо"));
        int after = query.Count();
        List<BookCopy> snapshot = query.ToList();
        localSource.Clear();

        return (before, after, snapshot.Count);
    }

    public static (int Items, long WithoutMaterializationMilliseconds, long WithMaterializationMilliseconds) Benchmark(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
        List<BookCopy> source = Enumerable.Range(0, count)
            .Select(number => BookCopy.Create(
                $"B-{number:000000}",
                $"978-demo-{number % 100:00}",
                $"Книга {number % 100}",
                2000 + number % 25,
                $"жанр-{number % 5}"))
            .ToList();

        Stopwatch withoutMaterializationTimer = Stopwatch.StartNew();
        _ = source
            .GroupBy(copy => copy.Isbn, StringComparer.OrdinalIgnoreCase)
            .Select(group => new StockLine(
                group.Key,
                group.First().Title,
                group.Count(),
                "прим."))
            .OrderByDescending(line => line.Quantity)
            .ThenBy(line => line.Sku, StringComparer.Ordinal)
            .Take(5)
            .ToList();
        withoutMaterializationTimer.Stop();

        Stopwatch withMaterializationTimer = Stopwatch.StartNew();
        _ = source
            .GroupBy(copy => copy.Isbn, StringComparer.OrdinalIgnoreCase)
            .Select(group => new StockLine(
                group.Key,
                group.First().Title,
                group.Count(),
                "прим."))
            .ToList()
            .OrderByDescending(line => line.Quantity)
            .ThenBy(line => line.Sku, StringComparer.Ordinal)
            .Take(5)
            .ToList();
        withMaterializationTimer.Stop();

        return (source.Count, withoutMaterializationTimer.ElapsedMilliseconds, withMaterializationTimer.ElapsedMilliseconds);
    }
}
