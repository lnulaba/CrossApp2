using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class InMemoryLibraryStore(IEnumerable<BookCopy>? seed = null) : ILibraryStore
{
    private readonly Dictionary<string, BookCopy> _copies =
        (seed ?? []).ToDictionary(copy => copy.Id, StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Loan> _loans = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<BookCopy> ListCopies() => _copies.Values.ToList();

    public BookCopy? GetCopyById(string id) => _copies.GetValueOrDefault(id);

    public IReadOnlyList<Loan> ListLoans() => _loans.Values.ToList();

    public Loan? GetLoanById(string id) => _loans.GetValueOrDefault(id);

    public void AddCopy(BookCopy copy)
    {
        ArgumentNullException.ThrowIfNull(copy);
        if (!_copies.TryAdd(copy.Id, copy))
        {
            throw new InvalidOperationException($"Примірник з id={copy.Id} уже існує.");
        }
    }

    public void UpdateCopy(BookCopy copy)
    {
        ArgumentNullException.ThrowIfNull(copy);
        _copies[copy.Id] = copy;
    }

    public void AddLoan(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);
        if (!_loans.TryAdd(loan.Id, loan))
        {
            throw new InvalidOperationException($"Видача з id={loan.Id} уже існує.");
        }
    }

    public void UpdateLoan(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);
        _loans[loan.Id] = loan;
    }
}