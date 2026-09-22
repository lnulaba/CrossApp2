using System.Globalization;
using Core.Dto;

namespace Core.Import;

public static class MixedCatalogCsvImporter
{
    private const char Separator = ';';

    public static ImportResult<CatalogRecord> Load(string path)
    {
        var items = new List<CatalogRecord>();
        var errors = new List<string>();
        string[] lines = File.ReadAllLines(path, System.Text.Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new ImportResult<CatalogRecord>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            ["B", var id, var isbn, var title, var year]
                when !string.IsNullOrWhiteSpace(id)
                    && !string.IsNullOrWhiteSpace(isbn)
                    && !string.IsNullOrWhiteSpace(title)
                    && int.TryParse(year, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedYear)
                => new ParseOk(new BookCatalogRecord(new BookDto(id, isbn, title, parsedYear))),
            ["R", var readerId, var name, var email]
                when !string.IsNullOrWhiteSpace(readerId) && !string.IsNullOrWhiteSpace(name)
                => new ParseOk(new ReaderCatalogRecord(new ReaderDto(readerId, name, email))),
            [var kind, ..] => new ParseFailed($"невідомий або пошкоджений тип '{kind}'"),
            _ => new ParseFailed("очікую рядок типу B;... або R;...")
        };
    }

    private abstract record ParseOutcome;

    private sealed record ParseOk(CatalogRecord Value) : ParseOutcome;

    private sealed record ParseFailed(string Reason) : ParseOutcome;
}