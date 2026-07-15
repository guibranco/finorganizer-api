using System.Net;
using System.Net.Http.Json;
using FinOrganizer.Application.Accounts;
using FinOrganizer.Domain.Enums;

namespace FinOrganizer.IntegrationTests;

public class AccountsEndpointsTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateThenGet_RoundTripsAccountWithComputedCurrentBalance()
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/api/v1/accounts", new CreateAccountRequest("Integration Checking", AccountType.Checking, "USD", 1000m));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<AccountDto>();
        Assert.NotNull(created);
        Assert.Equal(1000m, created!.CurrentBalance);

        var fetched = await _client.GetFromJsonAsync<AccountDto>($"/api/v1/accounts/{created.Id}");
        Assert.NotNull(fetched);
        Assert.Equal("Integration Checking", fetched!.Name);
        Assert.Equal(1000m, fetched.CurrentBalance);
    }

    [Fact]
    public async Task Update_PersistsChanges()
    {
        var created = await CreateAccountAsync("To Rename", 0m);

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/v1/accounts/{created.Id}", new UpdateAccountRequest("Renamed", AccountType.Savings, "USD", true));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<AccountDto>();
        Assert.Equal("Renamed", updated!.Name);
        Assert.Equal(AccountType.Savings, updated.Type);
        Assert.True(updated.IsArchived);
    }

    [Fact]
    public async Task Delete_RemovesAccountWithNoTransactions()
    {
        var created = await CreateAccountAsync("To Delete", 0m);

        var deleteResponse = await _client.DeleteAsync($"/api/v1/accounts/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/v1/accounts/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetById_UnknownId_ReturnsNotFoundProblemDetails()
    {
        var response = await _client.GetAsync($"/api/v1/accounts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Create_InvalidCurrency_ReturnsValidationProblemDetails()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/accounts", new CreateAccountRequest("Bad Currency", AccountType.Checking, "US", 0m));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<AccountDto> CreateAccountAsync(string name, decimal initialBalance)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/accounts", new CreateAccountRequest(name, AccountType.Checking, "USD", initialBalance));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AccountDto>())!;
    }
}
