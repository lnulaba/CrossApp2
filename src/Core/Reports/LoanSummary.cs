namespace Core.Reports;

public sealed record LoanSummary(string Sku, string Name, int LoanCount, int OpenLoanCount);
