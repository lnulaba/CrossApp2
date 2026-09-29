using Core.Dto;

namespace Core.Domain;

public sealed class Loan
{
    public string Id { get; }
    public string BookCopyId { get; }
    public string ReaderId { get; }
    public DateTime IssuedOn { get; }
    public DateTime? ReturnedOn { get; private set; }
    public bool IsOpen => ReturnedOn is null;

    private Loan(string id, string bookCopyId, string readerId, DateTime issuedOn)
    {
        Id = id;
        BookCopyId = bookCopyId;
        ReaderId = readerId;
        IssuedOn = issuedOn;
    }

    public static Loan Open(BookCopy copy, string id, string readerId, DateTime issuedOn)
    {
        ArgumentNullException.ThrowIfNull(copy);
        return Open(id, copy.Id, readerId, issuedOn);
    }

    private static Loan Open(string id, string bookCopyId, string readerId, DateTime issuedOn)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Номер видачі обов'язковий", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(bookCopyId))
        {
            throw new ArgumentException("Ідентифікатор примірника обов'язковий", nameof(bookCopyId));
        }

        if (string.IsNullOrWhiteSpace(readerId))
        {
            throw new ArgumentException("Ідентифікатор читача обов'язковий", nameof(readerId));
        }

        return new Loan(id.Trim(), bookCopyId.Trim(), readerId.Trim(), issuedOn);
    }

    public void Close(DateTime returnedOn)
    {
        if (!IsOpen)
        {
            throw new InvalidOperationException($"Видача {Id} вже закрита");
        }

        if (returnedOn < IssuedOn)
        {
            throw new ArgumentOutOfRangeException(
                nameof(returnedOn), returnedOn, "Дата повернення не може бути раніше дати видачі");
        }

        ReturnedOn = returnedOn;
    }

    public LoanDto ToDto() => new(Id, BookCopyId, ReaderId, IssuedOn, ReturnedOn);

    public static Loan FromDto(LoanDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        Loan loan = Open(dto.Id, dto.BookCopyId, dto.ReaderId, dto.IssuedOn);
        if (dto.ReturnedOn is DateTime returnedOn)
        {
            loan.Close(returnedOn);
        }

        return loan;
    }
}
