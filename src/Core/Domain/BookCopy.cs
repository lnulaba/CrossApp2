using Core.Dto;

namespace Core.Domain;

public sealed class BookCopy
{
    // Приватне поле зберігає стан примірника всередині сутності.
    private bool _isIssued;

    // Ідентичність можна читати, але змінити її ззовні не можна.
    public string Id { get; }
    public string Isbn { get; }
    public string Title { get; }
    public int Year { get; }
    public string Genre { get; }

    // Стан доступний для читання; змінюється лише через Issue() і Return().
    public bool IsIssued => _isIssued;

    // Приватний конструктор не дозволяє створити примірник в обхід Create().
    private BookCopy(string id, string isbn, string title, int year, string genre, bool isIssued)
    {
        Id = id;
        Isbn = isbn;
        Title = title;
        Year = year;
        Genre = genre;
        _isIssued = isIssued;
    }

    // Factory Method: створює валідний об'єкт і перевіряє його інваріанти.
    public static BookCopy Create(string id, string isbn)
        => Create(id, isbn, isbn, 0, "невідомий");

    public static BookCopy Create(string id, string isbn, string title, int year, string genre)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Ідентифікатор примірника обов'язковий", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(isbn))
        {
            throw new ArgumentException("ISBN не може бути порожнім", nameof(isbn));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Назва книги обов'язкова", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(genre))
        {
            throw new ArgumentException("Жанр книги обов'язковий", nameof(genre));
        }

        return new BookCopy(id.Trim(), isbn.Trim(), title.Trim(), year, genre.Trim(), false);
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

    public BookCopyDto ToDto() => new(Id, Isbn, IsIssued, Title, Year, Genre);

    public static BookCopy FromDto(BookCopyDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        BookCopy copy = Create(dto.Id, dto.Isbn, dto.Title ?? dto.Isbn, dto.Year, dto.Genre ?? "невідомий");
        if (dto.IsIssued)
        {
            copy.Issue();
        }

        return copy;
    }

    public override string ToString() => $"{Id} [{Isbn}] — {(IsIssued ? "виданий" : "доступний")}";
}
