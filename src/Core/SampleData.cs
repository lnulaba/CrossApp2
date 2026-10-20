using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IReadOnlyList<BookCopy> Copies() =>
    [
        BookCopy.Create("C-101", "978-617-001"),
        BookCopy.Create("C-102", "978-617-002"),
        BookCopy.Create("C-103", "978-617-003")
    ];
}