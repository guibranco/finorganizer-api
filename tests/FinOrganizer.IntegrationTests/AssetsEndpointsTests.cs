using System.Net;
using System.Net.Http.Json;
using FinOrganizer.Application.Accounts;
using FinOrganizer.Application.Assets;
using FinOrganizer.Domain.Enums;

namespace FinOrganizer.IntegrationTests;

public class AssetsEndpointsTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task BuyEventAndPriceSnapshot_ProduceComputedPositionWithMarketValue()
    {
        var brokerAccount = await CreateAccountAsync("Broker");
        var asset = await CreateAssetAsync("ACME");

        var buyResponse = await _client.PostAsJsonAsync("/api/v1/asset-events", new CreateAssetEventRequest(
            asset.Id, brokerAccount.Id, AssetEventType.Buy, Quantity: 10m, UnitPrice: 100m, Fees: 5m, Date: new DateOnly(2026, 1, 1), Notes: null));
        Assert.Equal(HttpStatusCode.Created, buyResponse.StatusCode);

        var priceResponse = await _client.PostAsJsonAsync(
            "/api/v1/assets/prices", new UpsertAssetPriceSnapshotRequest(asset.Id, new DateOnly(2026, 2, 1), 150m));
        Assert.Equal(HttpStatusCode.OK, priceResponse.StatusCode);

        var position = await _client.GetFromJsonAsync<AssetPositionDto>(
            $"/api/v1/assets/{asset.Id}/position?asOf=2026-02-01");

        Assert.NotNull(position);
        Assert.Equal(10m, position!.Quantity);
        Assert.Equal(100.5m, position.AverageCost); // (10*100 + 5) / 10
        Assert.Equal(150m, position.MarketPrice);
        Assert.Equal(1500m, position.MarketValue);
        Assert.Equal(495m, position.UnrealizedPnL); // 10 * (150 - 100.5)
    }

    [Fact]
    public async Task Create_DuplicateTicker_ReturnsServerErrorOrIsRejectedByUniqueIndex()
    {
        var ticker = $"DUP{Guid.NewGuid():N}"[..10];
        var first = await _client.PostAsJsonAsync("/api/v1/assets", new CreateAssetRequest(ticker, "First", AssetClass.Stock, "USD", null));
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await _client.PostAsJsonAsync("/api/v1/assets", new CreateAssetRequest(ticker, "Second", AssetClass.Stock, "USD", null));
        Assert.False(second.IsSuccessStatusCode);
    }

    private async Task<AccountDto> CreateAccountAsync(string name)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/accounts", new CreateAccountRequest(name, AccountType.Broker, "USD", 0m));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AccountDto>())!;
    }

    private async Task<AssetDto> CreateAssetAsync(string tickerPrefix)
    {
        var ticker = $"{tickerPrefix}{Guid.NewGuid():N}"[..10];
        var response = await _client.PostAsJsonAsync(
            "/api/v1/assets", new CreateAssetRequest(ticker, "Acme Corp", AssetClass.Stock, "USD", "NYSE"));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AssetDto>())!;
    }
}
