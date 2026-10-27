using Core.Domain;

namespace Core.Abstractions;

public interface ILibraryStore
{
    // Читання колекції без можливості змінити її напряму.
    IReadOnlyList<BookCopy> ListCopies();
    // Пошук примірника; null означає, що запис не знайдено.
    BookCopy? GetCopyById(string id);
    // Читання всіх операцій видачі.
    IReadOnlyList<Loan> ListLoans();
    // Пошук видачі за ідентифікатором.
    Loan? GetLoanById(string id);
    // Додавання нового примірника.
    void AddCopy(BookCopy copy);
    // Оновлення стану примірника після видачі або повернення.
    void UpdateCopy(BookCopy copy);
    // Додавання нової видачі.
    void AddLoan(Loan loan);
    // Оновлення видачі після повернення книги.
    void UpdateLoan(Loan loan);
}