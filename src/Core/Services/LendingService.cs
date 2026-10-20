using Core.Abstractions;
using Core.Domain;

namespace Core.Services;

public sealed class LendingService(ILibraryStore store)
{
    private const int MaxOpenLoansPerReader = 5;
    private readonly ILibraryStore _store = store ?? throw new ArgumentNullException(nameof(store));

    public BookCopy AddCopy(string id, string isbn)
    {
        BookCopy copy = BookCopy.Create(id, isbn);
        _store.AddCopy(copy);
        return copy;
    }

    public Loan IssueCopy(string copyId, string loanId, string readerId, DateTime issuedOn)
    {
        BookCopy copy = _store.GetCopyById(copyId)
            ?? throw new InvalidOperationException($"Примірник {copyId} не знайдено.");
        int openLoans = _store.ListLoans().Count(loan => loan.ReaderId == readerId && loan.IsOpen);
        if (openLoans >= MaxOpenLoansPerReader)
        {
            throw new InvalidOperationException(
                $"Читач {readerId} уже має {openLoans} відкритих видач, ліміт — {MaxOpenLoansPerReader}");
        }

        if (_store.GetLoanById(loanId) is not null)
        {
            throw new InvalidOperationException($"Видача з id={loanId} уже існує.");
        }

        Loan loan = Loan.Open(copy, loanId, readerId, issuedOn);
        copy.Issue();
        _store.UpdateCopy(copy);
        _store.AddLoan(loan);
        return loan;
    }

    public void ReturnCopy(string loanId, DateTime returnedOn)
    {
        Loan loan = _store.GetLoanById(loanId)
            ?? throw new InvalidOperationException($"Видачу {loanId} не знайдено.");
        BookCopy copy = _store.GetCopyById(loan.BookCopyId)
            ?? throw new InvalidOperationException($"Примірник {loan.BookCopyId} не знайдено.");
        if (!copy.IsIssued)
        {
            throw new InvalidOperationException($"Примірник {copy.Id} уже доступний.");
        }

        loan.Close(returnedOn);
        copy.Return();
        _store.UpdateLoan(loan);
        _store.UpdateCopy(copy);
    }

    public IReadOnlyList<BookCopy> AllCopies() => _store.ListCopies();

    public BookCopy? FindCopy(string id) => _store.GetCopyById(id);

    public IReadOnlyList<BookCopy> Search(Func<BookCopy, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.ListCopies().Where(predicate).ToList();
    }
}