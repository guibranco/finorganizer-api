using System.Net;
using System.Net.Http.Json;
using FinOrganizer.Application.Accounts;
using FinOrganizer.Application.Budgets;
using FinOrganizer.Application.Categories;
using FinOrganizer.Application.Dashboard;
using FinOrganizer.Application.Transactions;
using FinOrganizer.Domain.Enums;

namespace FinOrganizer.IntegrationTests;

public class DashboardAndBudgetsEndpointsTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task NetWorth_ReflectsAccountBalanceAfterTransaction()
    {
        var account = await CreateAccountAsync("Dashboard Checking", 1000m);
        await _client.PostAsJsonAsync("/api/v1/transactions", new CreateTransactionRequest(
            account.Id, TransactionType.Income, 200m, "USD", DateOnly.FromDateTime(DateTime.UtcNow), "Bonus", null, null, null));

        var points = await _client.GetFromJsonAsync<List<NetWorthPointDto>>("/api/v1/dashboard/net-worth?months=1");

        Assert.NotNull(points);
        var latest = Assert.Single(points!);
        Assert.True(latest.AccountsBalance >= 1200m);
    }

    [Fact]
    public async Task IncomeVsExpense_SeparatesIncomeAndExpenseForCurrentMonth()
    {
        var account = await CreateAccountAsync("IvE Checking", 0m);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await _client.PostAsJsonAsync("/api/v1/transactions", new CreateTransactionRequest(
            account.Id, TransactionType.Income, 500m, "USD", today, "Pay", null, null, null));
        await _client.PostAsJsonAsync("/api/v1/transactions", new CreateTransactionRequest(
            account.Id, TransactionType.Expense, 150m, "USD", today, "Bill", null, null, null));

        var months = await _client.GetFromJsonAsync<List<IncomeVsExpenseMonthDto>>("/api/v1/dashboard/income-vs-expense?months=1");

        var currentMonth = Assert.Single(months!);
        Assert.True(currentMonth.Income >= 500m);
        Assert.True(currentMonth.Expense >= 150m);
    }

    [Fact]
    public async Task BudgetVsActual_ComputesRemainingSpendForBudgetedCategory()
    {
        var account = await CreateAccountAsync("Budget Checking", 0m);
        var categories = await _client.GetFromJsonAsync<List<CategoryDto>>("/api/v1/categories");
        var utilities = categories!.Single(c => c.Name == "Utilities");
        var month = new DateOnly(2027, 1, 1);

        var budgetResponse = await _client.PostAsJsonAsync(
            "/api/v1/budgets", new CreateBudgetRequest(utilities.Id, month, 200m));
        Assert.Equal(HttpStatusCode.Created, budgetResponse.StatusCode);

        await _client.PostAsJsonAsync("/api/v1/transactions", new CreateTransactionRequest(
            account.Id, TransactionType.Expense, 80m, "USD", new DateOnly(2027, 1, 15), "Electric bill", utilities.Id, null, null));

        var lines = await _client.GetFromJsonAsync<List<BudgetVsActualDto>>($"/api/v1/budgets/vs-actual?month={month:yyyy-MM-dd}");

        var line = Assert.Single(lines!, l => l.CategoryId == utilities.Id);
        Assert.Equal(200m, line.LimitAmount);
        Assert.Equal(80m, line.ActualAmount);
        Assert.Equal(120m, line.Remaining);
    }

    private async Task<AccountDto> CreateAccountAsync(string name, decimal initialBalance)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/accounts", new CreateAccountRequest(name, AccountType.Checking, "USD", initialBalance));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AccountDto>())!;
    }
}
