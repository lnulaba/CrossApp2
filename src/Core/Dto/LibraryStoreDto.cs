namespace Core.Dto;

public sealed record LibraryStoreDto(
    List<BookCopyDto> Copies,
    List<LoanDto> Loans);