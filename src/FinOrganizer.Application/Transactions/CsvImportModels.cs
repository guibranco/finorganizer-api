using FinOrganizer.Domain.Enums;

namespace FinOrganizer.Application.Transactions;

/// <summary>Maps CSV column indexes (0-based) to transaction fields.</summary>
public sealed record CsvColumnMapping(
    int DateColumn,
    int AmountColumn,
    int? DescriptionColumn = null,
    int? TypeColumn = null,
    int? CategoryColumn = null,
    int? TagsColumn = null);

public sealed record ImportTransactionsRequest(
    string CsvContent,
    Guid AccountId,
    CsvColumnMapping Mapping,
    bool HasHeaderRow = true,
    char Delimiter = ',',
    string? DateFormat = null,
    string Currency = "USD",
    char TagsSeparator = '|');

public sealed record ParsedTransaction(
    TransactionType Type,
    decimal Amount,
    DateOnly Date,
    string? Description,
    string? CategoryName,
    IReadOnlyList<string> Tags);

public sealed record CsvImportRowResult(int RowNumber, ParsedTransaction? Transaction, string? Error);

public sealed record CsvImportResult(IReadOnlyList<CsvImportRowResult> Rows)
{
    public int SuccessCount => Rows.Count(r => r.Transaction is not null);

    public int ErrorCount => Rows.Count(r => r.Error is not null);
}

public sealed record TransactionImportSummary(int Imported, IReadOnlyList<CsvImportRowResult> Errors);
