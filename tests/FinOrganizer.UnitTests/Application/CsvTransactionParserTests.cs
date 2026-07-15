using FinOrganizer.Application.Transactions;
using FinOrganizer.Domain.Enums;

namespace FinOrganizer.UnitTests.Application;

public class CsvTransactionParserTests
{
    [Fact]
    public void Parse_SkipsHeaderRowAndInfersTypeFromAmountSign()
    {
        const string csv = "Date,Amount,Description\n2026-01-15,-50.00,Groceries\n2026-01-20,2000.00,Paycheck";
        var mapping = new CsvColumnMapping(DateColumn: 0, AmountColumn: 1, DescriptionColumn: 2);

        var result = CsvTransactionParser.Parse(csv, mapping);

        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(0, result.ErrorCount);

        var expense = result.Rows[0].Transaction!;
        Assert.Equal(TransactionType.Expense, expense.Type);
        Assert.Equal(50.00m, expense.Amount);
        Assert.Equal(new DateOnly(2026, 1, 15), expense.Date);
        Assert.Equal("Groceries", expense.Description);

        var income = result.Rows[1].Transaction!;
        Assert.Equal(TransactionType.Income, income.Type);
        Assert.Equal(2000.00m, income.Amount);
    }

    [Fact]
    public void Parse_ExplicitTypeColumn_OverridesSignInference()
    {
        const string csv = "2026-01-15,50.00,Expense";
        var mapping = new CsvColumnMapping(DateColumn: 0, AmountColumn: 1, TypeColumn: 2);

        var result = CsvTransactionParser.Parse(csv, mapping, hasHeaderRow: false);

        Assert.Equal(TransactionType.Expense, result.Rows[0].Transaction!.Type);
    }

    [Fact]
    public void Parse_QuotedFieldContainingDelimiter_IsKeptAsOneField()
    {
        const string csv = "2026-01-15,10.00,\"Coffee, tea, and snacks\"";
        var mapping = new CsvColumnMapping(DateColumn: 0, AmountColumn: 1, DescriptionColumn: 2);

        var result = CsvTransactionParser.Parse(csv, mapping, hasHeaderRow: false);

        Assert.Equal("Coffee, tea, and snacks", result.Rows[0].Transaction!.Description);
    }

    [Fact]
    public void Parse_TagsColumn_SplitsOnConfiguredSeparator()
    {
        const string csv = "2026-01-15,10.00,,food|weekly";
        var mapping = new CsvColumnMapping(DateColumn: 0, AmountColumn: 1, DescriptionColumn: 2, TagsColumn: 3);

        var result = CsvTransactionParser.Parse(csv, mapping, hasHeaderRow: false);

        Assert.Equal(["food", "weekly"], result.Rows[0].Transaction!.Tags);
    }

    [Fact]
    public void Parse_UnparseableDate_ProducesRowErrorInsteadOfThrowing()
    {
        const string csv = "not-a-date,10.00";
        var mapping = new CsvColumnMapping(DateColumn: 0, AmountColumn: 1);

        var result = CsvTransactionParser.Parse(csv, mapping, hasHeaderRow: false);

        Assert.Null(result.Rows[0].Transaction);
        Assert.Contains("could not parse date", result.Rows[0].Error);
        Assert.Equal(1, result.ErrorCount);
    }

    [Fact]
    public void Parse_TooFewColumns_ProducesRowError()
    {
        const string csv = "2026-01-15";
        var mapping = new CsvColumnMapping(DateColumn: 0, AmountColumn: 1);

        var result = CsvTransactionParser.Parse(csv, mapping, hasHeaderRow: false);

        Assert.Null(result.Rows[0].Transaction);
        Assert.Equal(1, result.ErrorCount);
    }

    [Fact]
    public void Parse_ExplicitDateFormat_IsRespected()
    {
        const string csv = "15/01/2026,10.00";
        var mapping = new CsvColumnMapping(DateColumn: 0, AmountColumn: 1);

        var result = CsvTransactionParser.Parse(csv, mapping, hasHeaderRow: false, dateFormat: "dd/MM/yyyy");

        Assert.Equal(new DateOnly(2026, 1, 15), result.Rows[0].Transaction!.Date);
    }

    [Fact]
    public void Parse_CategoryColumn_CapturesRawCategoryNameForLaterLookup()
    {
        const string csv = "2026-01-15,10.00,,Groceries";
        var mapping = new CsvColumnMapping(DateColumn: 0, AmountColumn: 1, DescriptionColumn: 2, CategoryColumn: 3);

        var result = CsvTransactionParser.Parse(csv, mapping, hasHeaderRow: false);

        Assert.Equal("Groceries", result.Rows[0].Transaction!.CategoryName);
    }
}
