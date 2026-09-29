namespace Core.Dto;

public sealed record BookCopyDto(string Id, string Isbn, bool IsIssued);

public sealed record LoanDto(
    string Id,
    string BookCopyId,
    string ReaderId,
    DateTime IssuedOn,
    DateTime? ReturnedOn);
