using Core.Events;

namespace Core.Abstractions;

public interface INotificationSink
{
    void Write(CatalogChangedEventArgs change);
}