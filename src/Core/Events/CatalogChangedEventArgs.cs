namespace Core.Events;

public sealed class CatalogChangedEventArgs(
    string copyId,
    CatalogChangeKind kind,
    string? loanId,
    string? readerId,
    bool isIssued) : EventArgs
{
    public string CopyId { get; } = copyId;
    public CatalogChangeKind Kind { get; } = kind;
    public string? LoanId { get; } = loanId;
    public string? ReaderId { get; } = readerId;
    public bool IsIssued { get; } = isIssued;
    public DateTimeOffset At { get; } = DateTimeOffset.Now;
}