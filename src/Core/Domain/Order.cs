namespace Core.Domain;

public sealed record OrderLine(string ProductId, string Name, decimal Price, int Quantity);

public sealed class Order
{
    private readonly List<OrderLine> _lines = [];

    public string Id { get; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public decimal Total => _lines.Sum(line => line.Price * line.Quantity);

    private Order(string id)
    {
        Id = id;
        Status = OrderStatus.Draft;
    }

    public static Order Create(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Номер замовлення обов'язковий", nameof(id));
        }

        return new Order(id.Trim());
    }

    public void AddLine(string productId, string name, decimal price, int quantity)
    {
        if (Status != OrderStatus.Draft)
        {
            throw new InvalidOperationException(
                $"Замовлення {Id} має стан {Status}, рядки можна додавати лише до Draft");
        }

        if (string.IsNullOrWhiteSpace(productId))
        {
            throw new ArgumentException("Ідентифікатор товару обов'язковий", nameof(productId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Назва товару не може бути порожньою", nameof(name));
        }

        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), price, "Ціна не може бути від'ємною");
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity), quantity, "Кількість у рядку має бути більшою за нуль");
        }

        _lines.Add(new OrderLine(productId.Trim(), name.Trim(), price, quantity));
    }

    public void Confirm()
    {
        if (_lines.Count == 0)
        {
            throw new InvalidOperationException($"Замовлення {Id} не можна підтвердити без рядків");
        }

        EnsureCanTransition(OrderStatus.Confirmed);
        Status = OrderStatus.Confirmed;
    }

    public void Cancel()
    {
        EnsureCanTransition(OrderStatus.Cancelled);
        Status = OrderStatus.Cancelled;
    }

    private void EnsureCanTransition(OrderStatus target)
    {
        _ = (Status, target) switch
        {
            (OrderStatus.Draft, OrderStatus.Confirmed) => true,
            (OrderStatus.Draft, OrderStatus.Cancelled) => true,
            _ => throw new InvalidOperationException(
                $"Замовлення {Id} не можна перевести зі стану {Status} у {target}")
        };
    }
}
