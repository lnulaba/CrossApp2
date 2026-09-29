namespace Core.Domain;

public sealed class LibraryLendingService
{
    private const int MaxOpenLoansPerReader = 5;
    private readonly List<Loan> _loans = [];

    public IReadOnlyList<Loan> Loans => _loans.AsReadOnly();

    public Loan Issue(BookCopy copy, string loanId, string readerId, DateTime issuedOn)
    {
        ArgumentNullException.ThrowIfNull(copy);

        int openLoans = _loans.Count(loan => loan.ReaderId == readerId && loan.IsOpen);
        if (openLoans >= MaxOpenLoansPerReader)
        {
            throw new InvalidOperationException(
                $"Читач {readerId} вже має {openLoans} відкритих видач, ліміт — {MaxOpenLoansPerReader}");
        }

        Loan loan = Loan.Open(copy, loanId, readerId, issuedOn);
        copy.Issue();
        _loans.Add(loan);
        return loan;
    }

    public void Return(BookCopy copy, Loan loan, DateTime returnedOn)
    {
        ArgumentNullException.ThrowIfNull(copy);
        ArgumentNullException.ThrowIfNull(loan);

        if (loan.BookCopyId != copy.Id)
        {
            throw new InvalidOperationException($"Видача {loan.Id} не належить примірнику {copy.Id}");
        }

        if (!loan.IsOpen)
        {
            throw new InvalidOperationException($"Видача {loan.Id} вже закрита");
        }

        loan.Close(returnedOn);
        copy.Return();
    }
}
