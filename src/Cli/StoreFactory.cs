using Core;
using Core.Abstractions;
using Core.Storage;

namespace Cli;

public static class StoreFactory
{
    public static ILibraryStore Create(string[] args)
    {
        ILibraryStore store;
        if (args.Contains("--file", StringComparer.OrdinalIgnoreCase))
        {
            string path = Path.GetFullPath(Path.Combine("data", "catalog.json"));
            store = new FileLibraryStore(path);
        }
        else
        {
            store = new InMemoryLibraryStore(SampleData.Copies());
        }

        return args.Contains("--cache", StringComparer.OrdinalIgnoreCase)
            ? new CachingLibraryStore(store)
            : store;
    }
}