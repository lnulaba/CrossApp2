namespace Core.Dto;

// Result Object: повертає і успішні елементи, і помилки часткового імпорту.
public sealed record ImportResult<T>(
    IReadOnlyList<T> Items,
    IReadOnlyList<string> Errors);