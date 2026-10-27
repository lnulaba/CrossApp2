using System.Globalization;
using System.Text.Json;
using Cli;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Core.Events;
using Core.Import;
using Core.Services;
using Core.Storage;

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

if (args.Contains("--lab5", StringComparer.OrdinalIgnoreCase))
{
    RunLab5(args);
    return 0;
}

if (args.Contains("--lab6", StringComparer.OrdinalIgnoreCase))
{
    RunLab6();
    return 0;
}

if (args.Contains("--lab6-sinks", StringComparer.OrdinalIgnoreCase))
{
    RunLab6Sinks();
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

    // BookCopy — доменна сутність
    BookCopy copy = BookCopy.Create("C-001", "978-617-001");
    //copy.IsIssued = true;
    var lending = new LibraryLendingService();
    Loan loan = lending.Issue(copy, "L-001", "reader-001", new DateTime(2026, 9, 1));
    Console.WriteLine($"Успіх: {copy}; видача {loan.Id} відкрита");

    lending.Return(copy, loan, new DateTime(2026, 9, 10));
    Console.WriteLine($"Повернення: {copy}; стан видачі {(loan.IsOpen ? "відкрита" : "закрита")}");

    BookCopyDto copyDto = copy.ToDto();
    BookCopy restoredCopy = BookCopy.FromDto(copyDto);
    Console.WriteLine($"DTO round-trip: {restoredCopy}");

    // Додаткове завдання 1: DTO перетворюються на сутності, помилки зберігаються.
    ImportResult<BookCopy> importedCopies = DomainImportMapper.ToBookCopies(new ImportResult<BookDto>(
        [new BookDto("B-001", "978-617-001", "Кобзар", 1840), new BookDto("B-002", "", "Пошкоджений", 2026)],
        ["рядок 99: демонстраційна помилка"]));
    Console.WriteLine($"Імпорт у домен: прийнято {importedCopies.Items.Count}, помилок {importedCopies.Errors.Count}");

    TryDo("порожній ISBN", () => BookCopy.Create("C-002", " "));
    Console.WriteLine(" стан: об'єкт не створено, існуючі сутності не змінені");

    Console.WriteLine("Інваріант: доступний примірник не можна повернути повторно.");
    TryDoAndShowUnchanged("повторне повернення", () => copy.ToString(), () => copy.Return());

    // Додаткове завдання 2: сервіс перевіряє правило між BookCopy, Loan і читачем.
    Loan invalidLoan = Loan.Open(copy, "L-002", "reader-001", new DateTime(2026, 9, 20));
    TryDoAndShowUnchanged(
        "дата раніше видачі",
        () => invalidLoan.IsOpen ? "видача відкрита, ReturnedOn = null" : $"видача закрита, ReturnedOn = {invalidLoan.ReturnedOn}",
        () => invalidLoan.Close(new DateTime(2026, 9, 19)));

    // Після п'яти відкритих видач шоста операція має бути відхилена.
    var limitedReader = new LibraryLendingService();
    for (int number = 1; number <= 5; number++)
    {
        limitedReader.Issue(
            BookCopy.Create($"C-{number + 1:000}", "978-617-001"),
            $"L-{number + 1:000}",
            "reader-limit",
            new DateTime(2026, 9, 1));
    }

    TryDoAndShowUnchanged(
        "шоста відкрита видача читача",
        () => $"відкритих видач: {limitedReader.Loans.Count}",
        () => limitedReader.Issue(
            BookCopy.Create("C-007", "978-617-001"),
            "L-007",
            "reader-limit",
            new DateTime(2026, 9, 1)));

    // Додаткове завдання 3: OrderStatus захищає допустимі переходи стану.
    Order order = Order.Create("O-001");
    order.AddLine("P-001", "Кобзар", 250, 1);
    order.Confirm();
    Console.WriteLine($"Замовлення: {order.Id}, {order.Status}, сума {order.Total}");
    TryDoAndShowUnchanged(
        "рядок підтвердженого замовлення",
        () => $"{order.Status}, рядків: {order.Lines.Count}, сума: {order.Total}",
        () => order.AddLine("P-002", "Лісова пісня", 200, 1));

    Console.WriteLine($"Фінальний стан примірника після відмов: {copy}");
}

static void RunLab5(string[] args)
{
    // Composition root передає готове сховище в сервіс через constructor DI.
    ILibraryStore store = StoreFactory.Create(args);
    // Сервіс отримує залежність через інтерфейс, а не створює сховище сам.
    var service = new LendingService(store);
    Console.WriteLine("=== Лабораторна 5: сервісний шар і ручний DI ===");
    Console.WriteLine($"Сховище: {store.GetType().Name}");

    int copyNumber = 201;
    while (service.FindCopy($"C-{copyNumber}") is not null)
    {
        copyNumber++;
    }

    string copyId = $"C-{copyNumber}";
    BookCopy copy = service.AddCopy(copyId, $"978-617-{copyNumber}");
    string loanId = $"L-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
    Loan loan = service.IssueCopy(copy.Id, loanId, "reader-201", DateTime.Today);
    service.ReturnCopy(loan.Id, DateTime.Today);
    Console.WriteLine($"Додано і повернуто: {copy.Id}, видача {loan.Id}");

    Console.WriteLine("Список примірників:");
    IReadOnlyList<BookCopy> copies = service.AllCopies();
    foreach (BookCopy item in copies)
    {
        Console.WriteLine($" {item}");
    }

    Console.WriteLine($"Всього примірників: {copies.Count}");

    const string searchText = "201";
    IReadOnlyList<BookCopy> search = service.Search(item =>
        item.Isbn.Contains(searchText, StringComparison.OrdinalIgnoreCase));
    Console.WriteLine($"Пошук: ISBN містить \"{searchText}\"; знайдено {search.Count}");
    foreach (BookCopy match in search)
    {
        Console.WriteLine($" знайдено: {match}");
    }

    TryDo("дубль примірника", () => service.AddCopy(copy.Id, copy.Isbn));
    TryDo(
        "видача неіснуючого примірника",
        () => service.IssueCopy("missing", "L-missing", "reader-201", DateTime.Today));
}

static void RunLab6()
{
    ILibraryStore store = new InMemoryLibraryStore(SampleData.Copies());
    var service = new LendingService(store);
    string logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
    Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
    File.WriteAllText(logPath, string.Empty);

    void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
    {
        string line = $"{e.At:HH:mm:ss} {e.Kind,-12} {e.CopyId} {e.ReaderId ?? "-"} " +
            $"issued={e.IsIssued}";
        Console.WriteLine(line);
        File.AppendAllText(logPath, line + Environment.NewLine);
    }

    int changes = 0;
    service.Changed += OnCatalogChanged;
    service.Changed += (_, _) => changes++;

    Console.WriteLine("=== Лабораторна 6: доменні події ===");
    string copyId = $"L6-{Guid.NewGuid():N}"[..10];
    BookCopy created = service.AddCopy(copyId, "978-617-006");
    Loan loan = service.IssueCopy(created.Id, "L6-loan", "reader-006", DateTime.Today);
    Console.WriteLine($"Подій отримано: {changes}");

    service.Changed -= OnCatalogChanged;
    service.ReturnCopy(loan.Id, DateTime.Today);
    Console.WriteLine($"Після -= подій отримано: {changes}");

    TryDo("видача неіснуючого примірника", () =>
        service.IssueCopy("missing", "L6-missing", "reader-006", DateTime.Today));
    Console.WriteLine($"Після відмови подій отримано: {changes}");

    IReadOnlyList<BookCopy> available = service.FindAll(copy => !copy.IsIssued);
    Console.WriteLine($"Func-пошук доступних примірників: {available.Count}");
    Console.WriteLine($"Лог: {logPath}");
}

static void RunLab6Sinks()
{
    string logPath = Path.Combine(AppContext.BaseDirectory, "logs", "sinks.log");
    Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
    File.WriteAllText(logPath, string.Empty);

    INotificationSink[] sinks = [new ConsoleSink(), new FileSink(logPath)];
    ILibraryStore store = new InMemoryLibraryStore(SampleData.Copies());
    var service = new LendingService(store, sinks);
    BookCopy copy = service.AddCopy("L6-SINK", "978-617-006");
    service.IssueCopy(copy.Id, "L6-sink-loan", "reader-sink", DateTime.Today);

    Console.WriteLine($"Sink-лог: {logPath}");
    Console.WriteLine("Звіт імпорту:");
    ImportResult<BookDto> import = BookCsvImporter.Load(
        Path.Combine("data", "test_errors.csv"),
        error => Console.WriteLine($" ! {error}"));
    Console.WriteLine($"Прийнято: {import.Items.Count}, помилок: {import.Errors.Count}");
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

static void TryDoAndShowUnchanged(string title, Func<string> readState, Action action)
{
    string before = readState();
    TryDo(title, action);
    string after = readState();

    Console.WriteLine($" до відмови: {before}");
    Console.WriteLine($" після відмови: {after}");
    Console.WriteLine($" стан не змінився: {(before == after ? "так" : "НІ")}");
}

