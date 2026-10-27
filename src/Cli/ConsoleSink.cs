using Core.Abstractions;
using Core.Events;

namespace Cli;

public sealed class ConsoleSink : INotificationSink
{
    public void Write(CatalogChangedEventArgs change)
    {
        Console.WriteLine(Format(change));
    }

    private static string Format(CatalogChangedEventArgs change) =>
        $"{change.At:HH:mm:ss} {change.Kind,-12} {change.CopyId} " +
        $"{change.ReaderId ?? "-"} issued={change.IsIssued}";
}