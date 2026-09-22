using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using Core;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = System.Text.Encoding.UTF8;

if (args.Length == 1 && args[0].Equals("--json", StringComparison.OrdinalIgnoreCase))
{
    JsonSerializerOptions options = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    Console.WriteLine(JsonSerializer.Serialize(EnvironmentInfo.Collect(), options));
    return 0;
}

string path = args.FirstOrDefault(argument => !argument.StartsWith("--", StringComparison.Ordinal))
    ?? Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

if (args.Contains("--mixed", StringComparer.OrdinalIgnoreCase))
{
    ImportResult<CatalogRecord> mixedResult = MixedCatalogCsvImporter.Load(path);
    Console.WriteLine($"Завантажено записів: {mixedResult.Items.Count}");
    foreach (CatalogRecord item in mixedResult.Items)
    {
        switch (item)
        {
            case BookCatalogRecord book:
                Console.WriteLine($" книга: {book.Value.Id} {book.Value.Title} ({book.Value.Year})");
                break;
            case ReaderCatalogRecord reader:
                Console.WriteLine($" читач: {reader.Value.Id} {reader.Value.Name}");
                break;
        }
    }

    foreach (string error in mixedResult.Errors)
    {
        Console.WriteLine($" ! {error}");
    }

    return 0;
}

ImportResult<BookDto> result = Path.GetExtension(path).ToLowerInvariant() switch
{
    ".csv" => BookCsvImporter.Load(path),
    ".json" => BookJsonImporter.Load(path),
    _ => new ImportResult<BookDto>([], [$"непідтримуваний формат файлу: {Path.GetExtension(path)}"])
};

Console.WriteLine($"Завантажено записів: {result.Items.Count}");
foreach (BookDto book in result.Items.Take(5))
{
    Console.WriteLine($" {book.Id,-6} {book.Isbn,-15} {book.Title,-30} {book.Year,5}");
}

if (result.Errors.Count > 0)
{
    Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");
    foreach (string error in result.Errors)
    {
        Console.WriteLine($" ! {error}");
    }
}

int total = result.Items.Count + result.Errors.Count;
double errorPercent = total == 0 ? 0 : result.Errors.Count * 100.0 / total;
Console.WriteLine(string.Format(
    CultureInfo.InvariantCulture,
    "Статистика: усього {0}, прийнято {1}, пропущено {2}, помилки {3:F1}%",
    total,
    result.Items.Count,
    result.Errors.Count,
    errorPercent));

return 0;
