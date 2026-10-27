using Core;
using Core.Abstractions;
using Core.Storage;

namespace Cli;

public static class StoreFactory
{
    // Створює сховище за аргументами CLI та повертає його через інтерфейс.
    public static ILibraryStore Create(string[] args)
    {
        // Composition root: вибір реалізації залишається в одному місці.
        ILibraryStore store = args.Contains("--file", StringComparer.OrdinalIgnoreCase)
            // Файловий режим зберігає дані у data/catalog.json.
            ? new FileLibraryStore(
                Path.GetFullPath(Path.Combine("data", "catalog.json")),
                SampleData.Copies())
            // Звичайний режим використовує початкові дані в пам'яті.
            : new InMemoryLibraryStore(SampleData.Copies());

        // Додаткове завдання: декоратор додає кешування без зміни сервісу.
        return args.Contains("--cache", StringComparer.OrdinalIgnoreCase)
            ? new CachingLibraryStore(store)
            : store;
    }
}