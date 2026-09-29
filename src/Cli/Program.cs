using System.Globalization;
using System.Text.Json;
using Core;
using Core.Domain;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = System.Text.Encoding.UTF8;

if (args.Length == 1 && args[0].Equals("--json", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine(JsonSerializer.Serialize(
        EnvironmentInfo.Collect(),
        CliJsonContext.Default.EnvironmentReport));
    return 0;
}

if (args.Contains("--lab4", StringComparer.OrdinalIgnoreCase))
{
    RunLab4();
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

static void RunLab4()
{
    Console.WriteLine("=== Лабораторна 4: доменна модель ===");
    BookCopy copy = BookCopy.Create("C-001", "978-617-001");
    var lending = new LibraryLendingService();
    Loan loan = lending.Issue(copy, "L-001", "reader-001", new DateTime(2026, 9, 1));
    Console.WriteLine($"Успіх: {copy}; видача {loan.Id} відкрита");

    lending.Return(copy, loan, new DateTime(2026, 9, 10));
    Console.WriteLine($"Повернення: {copy}; стан видачі {(loan.IsOpen ? "відкрита" : "закрита")}");

    BookCopyDto copyDto = copy.ToDto();
    BookCopy restoredCopy = BookCopy.FromDto(copyDto);
    Console.WriteLine($"DTO round-trip: {restoredCopy}");

    ImportResult<BookCopy> importedCopies = DomainImportMapper.ToBookCopies(new ImportResult<BookDto>(
        [new BookDto("B-001", "978-617-001", "Кобзар", 1840), new BookDto("B-002", "", "Пошкоджений", 2026)],
        ["рядок 99: демонстраційна помилка"]));
    Console.WriteLine($"Імпорт у домен: прийнято {importedCopies.Items.Count}, помилок {importedCopies.Errors.Count}");

    TryDo("порожній ISBN", () => BookCopy.Create("C-002", " "));
    TryDo("повторне повернення", () => copy.Return());
    TryDo(
        "дата раніше видачі",
        () => Loan.Open(copy, "L-002", "reader-001", new DateTime(2026, 9, 20))
            .Close(new DateTime(2026, 9, 19)));

    var limitedReader = new LibraryLendingService();
    for (int number = 1; number <= 5; number++)
    {
        limitedReader.Issue(
            BookCopy.Create($"C-{number + 1:000}", "978-617-001"),
            $"L-{number + 1:000}",
            "reader-limit",
            new DateTime(2026, 9, 1));
    }

    TryDo(
        "шоста відкрита видача читача",
        () => limitedReader.Issue(
            BookCopy.Create("C-007", "978-617-001"),
            "L-007",
            "reader-limit",
            new DateTime(2026, 9, 1)));

    Order order = Order.Create("O-001");
    order.AddLine("P-001", "Кобзар", 250, 1);
    order.Confirm();
    Console.WriteLine($"Замовлення: {order.Id}, {order.Status}, сума {order.Total}");
    TryDo("рядок підтвердженого замовлення", () => order.AddLine("P-002", "Лісова пісня", 200, 1));

    Console.WriteLine($"Фінальний стан примірника після відмов: {copy}");
}

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($" {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception exception)
    {
        Console.WriteLine($" {title}: {exception.GetType().Name} — {exception.Message}");
    }
}
