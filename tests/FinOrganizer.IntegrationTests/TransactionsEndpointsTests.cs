using System.Net;
using System.Net.Http.Json;
using FinOrganizer.Application.Accounts;
using FinOrganizer.Application.Categories;
using FinOrganizer.Application.Common.Models;
using FinOrganizer.Application.Transactions;
using FinOrganizer.Domain.Enums;

namespace FinOrganizer.IntegrationTests;

public class TransactionsEndpointsTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateExpense_ReducesAccountCurrentBalance()
    {
        var account = await CreateAccountAsync("Txn Checking", 500m);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/transactions", new CreateTransactionRequest(
            account.Id, TransactionType.Expense, 75m, "USD", new DateOnly(2026, 3, 1), "Test expense", null, null, null));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var updatedAccount = await _client.GetFromJsonAsync<AccountDto>($"/api/v1/accounts/{account.Id}");
        Assert.Equal(425m, updatedAccount!.CurrentBalance);
    }

    [Fact]
    public async Task ListWithAccountFilter_ReturnsOnlyThatAccountsTransactions()
    {
        var account = await CreateAccountAsync("Filter Target", 0m);
        var otherAccount = await CreateAccountAsync("Filter Other", 0m);

        await _client.PostAsJsonAsync("/api/v1/transactions", new CreateTransactionRequest(
            account.Id, TransactionType.Income, 100m, "USD", new DateOnly(2026, 3, 5), "In scope", null, null, null));
        await _client.PostAsJsonAsync("/api/v1/transactions", new CreateTransactionRequest(
            otherAccount.Id, TransactionType.Income, 999m, "USD", new DateOnly(2026, 3, 5), "Out of scope", null, null, null));

        var page = await _client.GetFromJsonAsync<PagedResult<TransactionDto>>(
            $"/api/v1/transactions?accountId={account.Id}&page=1&pageSize=50");

        Assert.NotNull(page);
        Assert.All(page!.Items, t => Assert.Equal(account.Id, t.AccountId));
        Assert.Contains(page.Items, t => t.Description == "In scope");
    }

    [Fact]
    public async Task Delete_RemovesTransactionAndRestoresBalance()
    {
        var account = await CreateAccountAsync("Delete Txn", 200m);
        var createResponse = await _client.PostAsJsonAsync("/api/v1/transactions", new CreateTransactionRequest(
            account.Id, TransactionType.Expense, 50m, "USD", new DateOnly(2026, 3, 1), null, null, null, null));
        var created = await createResponse.Content.ReadFromJsonAsync<TransactionDto>();

        var deleteResponse = await _client.DeleteAsync($"/api/v1/transactions/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var restoredAccount = await _client.GetFromJsonAsync<AccountDto>($"/api/v1/accounts/{account.Id}");
        Assert.Equal(200m, restoredAccount!.CurrentBalance);
    }

    [Fact]
    public async Task ImportCsv_ResolvesCategoryByNameAndPersistsTransactions()
    {
        var account = await CreateAccountAsync("Import Target", 0m);
        var category = await _client.GetFromJsonAsync<List<CategoryDto>>("/api/v1/categories");
        var groceries = category!.Single(c => c.Name == "Groceries");

        const string csv = "Date,Amount,Description,Category\n2026-03-10,-42.50,Weekly shop,Groceries";
        var request = new ImportTransactionsRequest(
            csv, account.Id, new CsvColumnMapping(0, 1, 2, CategoryColumn: 3), HasHeaderRow: true, Currency: "USD");

        var response = await _client.PostAsJsonAsync("/api/v1/transactions/import", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var summary = await response.Content.ReadFromJsonAsync<TransactionImportSummary>();
        Assert.Equal(1, summary!.Imported);
        Assert.Empty(summary.Errors);

        var page = await _client.GetFromJsonAsync<PagedResult<TransactionDto>>($"/api/v1/transactions?accountId={account.Id}");
        var imported = Assert.Single(page!.Items);
        Assert.Equal(42.50m, imported.Amount);
        Assert.Equal(TransactionType.Expense, imported.Type);
        Assert.Equal(groceries.Id, imported.CategoryId);
    }

    private async Task<AccountDto> CreateAccountAsync(string name, decimal initialBalance)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/accounts", new CreateAccountRequest(name, AccountType.Checking, "USD", initialBalance));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AccountDto>())!;
    }
}
