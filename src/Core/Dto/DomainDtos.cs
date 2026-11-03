namespace Core.Dto;

public sealed record BookCopyDto(
    string Id,
    string Isbn,
    bool IsIssued,
    string? Title = null,
    int Year = 0,
    string? Genre = null);

public sealed record LoanDto(
    string Id,
    string BookCopyId,
    string ReaderId,
    DateTime IssuedOn,
    DateTime? ReturnedOn);
