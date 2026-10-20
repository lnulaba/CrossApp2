using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CachingLibraryStore(ILibraryStore inner) : ILibraryStore
{
    private readonly ILibraryStore _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private IReadOnlyList<BookCopy>? _copies;
    private IReadOnlyList<Loan>? _loans;

    public IReadOnlyList<BookCopy> ListCopies() => _copies ??= _inner.ListCopies();

    public BookCopy? GetCopyById(string id) => ListCopies().FirstOrDefault(copy =>
        string.Equals(copy.Id, id, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<Loan> ListLoans() => _loans ??= _inner.ListLoans();

    public Loan? GetLoanById(string id) => ListLoans().FirstOrDefault(loan =>
        string.Equals(loan.Id, id, StringComparison.OrdinalIgnoreCase));

    public void AddCopy(BookCopy copy)
    {
        _inner.AddCopy(copy);
        InvalidateCopies();
    }

    public void UpdateCopy(BookCopy copy)
    {
        _inner.UpdateCopy(copy);
        InvalidateCopies();
    }

    public void AddLoan(Loan loan)
    {
        _inner.AddLoan(loan);
        InvalidateLoans();
    }

    public void UpdateLoan(Loan loan)
    {
        _inner.UpdateLoan(loan);
        InvalidateLoans();
    }

    private void InvalidateCopies() => _copies = null;

    private void InvalidateLoans() => _loans = null;
}