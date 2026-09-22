namespace Core.Dto;

public record BookDto(
    string Id,
    string Isbn,
    string Title,
    int Year,
    string? Author = null);

public record ReaderDto(string Id, string Name, string? Email = null);

public abstract record CatalogRecord;

public sealed record BookCatalogRecord(BookDto Value) : CatalogRecord;

public sealed record ReaderCatalogRecord(ReaderDto Value) : CatalogRecord;