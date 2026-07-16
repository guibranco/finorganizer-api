using System.Globalization;
using FinOrganizer.Domain.Enums;

namespace FinOrganizer.Application.Transactions;

/// <summary>
/// Pure CSV-to-transaction parser. Takes no dependency on persistence so it can be unit tested
/// directly against raw CSV text and a column mapping.
/// </summary>
public static class CsvTransactionParser
{
    public static CsvImportResult Parse(
        string csvContent,
        CsvColumnMapping mapping,
        bool hasHeaderRow = true,
        char delimiter = ',',
        string? dateFormat = null,
        char tagsSeparator = '|')
    {
        var lines = csvContent
            .Split(["\r\n", "\n"], StringSplitOptions.None)
            .Where(line => line.Length > 0)
            .ToList();

        var dataLines = hasHeaderRow && lines.Count > 0 ? lines.Skip(1) : lines;

        var results = new List<CsvImportRowResult>();
        var rowNumber = hasHeaderRow ? 1 : 0;

        foreach (var line in dataLines)
        {
            rowNumber++;
            results.Add(ParseRow(line, rowNumber, mapping, delimiter, dateFormat, tagsSeparator));
        }

        return new CsvImportResult(results);
    }

    private static CsvImportRowResult ParseRow(
        string line, int rowNumber, CsvColumnMapping mapping, char delimiter, string? dateFormat, char tagsSeparator)
    {
        var fields = SplitCsvLine(line, delimiter);

        var maxRequiredColumn = Math.Max(mapping.DateColumn, mapping.AmountColumn);
        if (fields.Count <= maxRequiredColumn)
        {
            return new CsvImportRowResult(rowNumber, null, $"Row {rowNumber}: expected at least {maxRequiredColumn + 1} columns, found {fields.Count}.");
        }

        var dateText = fields[mapping.DateColumn].Trim();
        DateOnly date;
        if (dateFormat is not null)
        {
            if (!DateOnly.TryParseExact(dateText, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return new CsvImportRowResult(rowNumber, null, $"Row {rowNumber}: could not parse date '{dateText}' using format '{dateFormat}'.");
            }
        }
        else if (!DateOnly.TryParse(dateText, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            return new CsvImportRowResult(rowNumber, null, $"Row {rowNumber}: could not parse date '{dateText}'.");
        }

        var amountText = fields[mapping.AmountColumn].Trim();
        if (!decimal.TryParse(amountText, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var rawAmount))
        {
            return new CsvImportRowResult(rowNumber, null, $"Row {rowNumber}: could not parse amount '{amountText}'.");
        }

        TransactionType type;
        if (mapping.TypeColumn is { } typeCol && fields.Count > typeCol && !string.IsNullOrWhiteSpace(fields[typeCol]))
        {
            if (!Enum.TryParse(fields[typeCol].Trim(), ignoreCase: true, out type))
            {
                return new CsvImportRowResult(rowNumber, null, $"Row {rowNumber}: unrecognized transaction type '{fields[typeCol]}'.");
            }
        }
        else
        {
            type = rawAmount < 0 ? TransactionType.Expense : TransactionType.Income;
        }

        var description = mapping.DescriptionColumn is { } descCol && fields.Count > descCol
            ? fields[descCol].Trim()
            : null;

        var categoryName = mapping.CategoryColumn is { } catCol && fields.Count > catCol && !string.IsNullOrWhiteSpace(fields[catCol])
            ? fields[catCol].Trim()
            : null;

        var tags = mapping.TagsColumn is { } tagsCol && fields.Count > tagsCol && !string.IsNullOrWhiteSpace(fields[tagsCol])
            ? fields[tagsCol].Split(tagsSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : [];

        var transaction = new ParsedTransaction(type, Math.Abs(rawAmount), date, description, categoryName, tags);
        return new CsvImportRowResult(rowNumber, transaction, null);
    }

    /// <summary>Splits one CSV line honoring double-quoted fields that may contain the delimiter.</summary>
    private static List<string> SplitCsvLine(string line, char delimiter)
    {
        var fields = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (inQuotes)
            {
                if (c == '"' && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else if (c == '"')
                {
                    inQuotes = false;
                }
                else
                {
                    current.Append(c);
                }
            }
            else if (c == '"')
            {
                inQuotes = true;
            }
            else if (c == delimiter)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        fields.Add(current.ToString());
        return fields;
    }
}
