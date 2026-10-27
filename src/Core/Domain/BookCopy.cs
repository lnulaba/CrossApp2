using Core.Dto;

namespace Core.Domain;

public sealed class BookCopy
{
    // Приватне поле зберігає стан примірника всередині сутності.
    private bool _isIssued;

    // Ідентичність можна читати, але змінити її ззовні не можна.
    public string Id { get; }
    public string Isbn { get; }

    // Стан доступний для читання; змінюється лише через Issue() і Return().
    public bool IsIssued => _isIssued;

    // Приватний конструктор не дозволяє створити примірник в обхід Create().
    private BookCopy(string id, string isbn, bool isIssued)
    {
        Id = id;
        Isbn = isbn;
        _isIssued = isIssued;
    }

    // Factory Method: створює валідний об'єкт і перевіряє його інваріанти.
    public static BookCopy Create(string id, string isbn)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Ідентифікатор примірника обов'язковий", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(isbn))
        {
            throw new ArgumentException("ISBN не може бути порожнім", nameof(isbn));
        }

        return new BookCopy(id.Trim(), isbn.Trim(), false);
    }

    public void Issue()
    {
        if (IsIssued)
        {
            throw new InvalidOperationException(
                $"Примірник {Id} вже виданий, повторна видача неможлива");
        }

        _isIssued = true;
    }

    public void Return()
    {
        if (!IsIssued)
        {
            throw new InvalidOperationException(
                $"Примірник {Id} не виданий, повернення неможливе");
        }

        _isIssued = false;
    }

    public BookCopyDto ToDto() => new(Id, Isbn, IsIssued);

    public static BookCopy FromDto(BookCopyDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        BookCopy copy = Create(dto.Id, dto.Isbn);
        if (dto.IsIssued)
        {
            copy.Issue();
        }

        return copy;
    }

    public override string ToString() => $"{Id} [{Isbn}] — {(IsIssued ? "виданий" : "доступний")}";
}
