using System.Net;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using NUnit.Framework;
using SentryQA.ApiTests.Clients;
using SentryQA.Core;

namespace SentryQA.ApiTests.Tests;

[TestFixture]
public class PricePulseApiTests
{
    private PricePulseApiClient _client = null!;

    [SetUp]
    public void SetUp() => _client = new PricePulseApiClient();

    [Test]
    public async Task Search_WithValidQuery_ReturnsOk()
    {
        var response = await _client.SearchAsync("laptop");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Search_ResponseBody_MatchesExpectedSchema()
    {
        var response = await _client.SearchAsync("laptop");
        response.Content.Should().NotBeNullOrEmpty();

        using var doc = JsonDocument.Parse(response.Content!);
        var root = doc.RootElement;

        root.ValueKind.Should().Be(JsonValueKind.Array, "search should return a list of results");
        if (root.GetArrayLength() > 0)
        {
            var first = root[0];
            first.TryGetProperty("productName", out _).Should().BeTrue("each result must include productName");
        }
    }

    [Test]
    public async Task Search_ResponseTime_IsUnderThreshold()
    {
        var latency = await _client.MeasureLatencyAsync(() => _client.SearchAsync("laptop"));

        // PricePulse's README claims sub-100ms prediction latency for the
        // in-process model; search involves a DB round trip so we allow more
        // headroom while still catching real regressions.
        latency.TotalMilliseconds.Should().BeLessThan(1500,
            "search should stay responsive under normal load");
    }

    [Test]
    public async Task Search_WithEmptyQuery_ReturnsClientError()
    {
        var response = await _client.SearchAsync("");
        ((int)response.StatusCode).Should().BeInRange(400, 499,
            "an empty search term should be rejected, not silently return all products");
    }

    [Test]
    public async Task History_ForUnknownProductId_ReturnsNotFound()
    {
        var response = await _client.GetPriceHistoryAsync("nonexistent-product-id-zzz");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Fuzz-style test: generates a batch of randomized, realistic-looking
    /// search terms with Bogus and asserts the API never 500s on any of
    /// them. This catches the class of bug that hand-written test cases
    /// almost always miss — the search term nobody thought to try.
    /// </summary>
    [Test]
    public async Task Search_WithRandomizedTerms_NeverReturnsServerError()
    {
        var faker = new Faker();
        var randomTerms = Enumerable.Range(0, 15)
            .Select(_ => faker.Commerce.ProductName())
            .ToList();

        var failures = new List<string>();

        foreach (var term in randomTerms)
        {
            var response = await _client.SearchAsync(term);
            if ((int)response.StatusCode >= 500)
                failures.Add($"'{term}' -> {(int)response.StatusCode}");
        }

        failures.Should().BeEmpty(
            $"no search term should cause a 5xx. Failing terms: {string.Join(", ", failures)}");
    }
}
