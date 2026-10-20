using Core.Domain;

namespace Core.Abstractions;

public interface ILibraryStore
{
    IReadOnlyList<BookCopy> ListCopies();
    BookCopy? GetCopyById(string id);
    IReadOnlyList<Loan> ListLoans();
    Loan? GetLoanById(string id);
    void AddCopy(BookCopy copy);
    void UpdateCopy(BookCopy copy);
    void AddLoan(Loan loan);
    void UpdateLoan(Loan loan);
}