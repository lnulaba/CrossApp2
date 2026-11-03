using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IReadOnlyList<BookCopy> Copies() =>
    [
        BookCopy.Create("C-101", "978-617-001", "Кобзар", 1840, "поезія"),
        BookCopy.Create("C-102", "978-617-001", "Кобзар", 1840, "поезія"),
        BookCopy.Create("C-103", "978-617-001", "Кобзар", 1840, "поезія"),
        BookCopy.Create("C-104", "978-617-002", "Лісова пісня", 1911, "драма"),
        BookCopy.Create("C-105", "978-617-002", "Лісова пісня", 1911, "драма"),
        BookCopy.Create("C-106", "978-617-003", "Місто", 1928, "роман"),
        BookCopy.Create("C-107", "978-617-003", "Місто", 1928, "роман"),
        BookCopy.Create("C-108", "978-617-003", "Місто", 1928, "роман"),
        BookCopy.Create("C-109", "978-617-003", "Місто", 1928, "роман"),
        BookCopy.Create("C-110", "978-617-004", "Тигролови", 1944, "роман"),
        BookCopy.Create("C-111", "978-617-004", "Тигролови", 1944, "роман"),
        BookCopy.Create("C-112", "978-617-005", "Захар Беркут", 1883, "історія"),
        BookCopy.Create("C-113", "978-617-005", "Захар Беркут", 1883, "історія"),
        BookCopy.Create("C-114", "978-617-006", "Intermezzo", 1908, "новела"),
        BookCopy.Create("C-115", "978-617-006", "Intermezzo", 1908, "новела"),
        BookCopy.Create("C-116", "978-617-007", "Україна", 1917, "історія"),
        BookCopy.Create("C-117", "978-617-008", "Чорна рада", 1857, "історія")
    ];
}