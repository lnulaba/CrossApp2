using Core.Abstractions;
using Core.Events;

namespace Cli;

public sealed class FileSink(string path) : INotificationSink
{
    private readonly string _path = path;

    public void Write(CatalogChangedEventArgs change)
    {
        string line = $"{change.At:HH:mm:ss} {change.Kind,-12} {change.CopyId} " +
            $"{change.ReaderId ?? "-"} issued={change.IsIssued}";
        File.AppendAllText(_path, line + Environment.NewLine);
    }
}