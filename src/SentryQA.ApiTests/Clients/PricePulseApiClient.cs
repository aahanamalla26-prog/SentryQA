using RestSharp;
using SentryQA.Core;

namespace SentryQA.ApiTests.Clients;

public record SearchResult(string ProductName, decimal? PredictedPrice, DateTime? Timestamp);

/// <summary>
/// Thin, typed wrapper around the raw REST calls so test classes read like
/// business intent ("SearchAsync('laptop')") instead of re-building
/// RestRequest objects in every test. Centralizing this also means the
/// framework only needs one place to update if the API's base path or auth
/// scheme ever changes.
/// </summary>
public class PricePulseApiClient
{
    private readonly RestClient _client;

    public PricePulseApiClient()
    {
        _client = new RestClient(TestConfig.BaseApiUrl);
    }

    public async Task<RestResponse> SearchAsync(string productName)
    {
        var request = new RestRequest("/search", Method.Get)
            .AddQueryParameter("q", productName);

        TestLogger.Info($"GET /search?q={productName}");
        var response = await _client.ExecuteAsync(request);
        return response;
    }

    public async Task<RestResponse> GetPriceHistoryAsync(string productId)
    {
        var request = new RestRequest($"/history/{productId}", Method.Get);
        TestLogger.Info($"GET /history/{productId}");
        return await _client.ExecuteAsync(request);
    }

    public async Task<RestResponse> GetPredictionAsync(string productId)
    {
        var request = new RestRequest($"/predict/{productId}", Method.Get);
        TestLogger.Info($"GET /predict/{productId}");
        return await _client.ExecuteAsync(request);
    }

    public async Task<TimeSpan> MeasureLatencyAsync(Func<Task<RestResponse>> call)
    {
        var start = DateTime.UtcNow;
        await call();
        return DateTime.UtcNow - start;
    }
}
