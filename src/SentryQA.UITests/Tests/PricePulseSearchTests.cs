using NUnit.Framework;
using OpenQA.Selenium;
using SentryQA.Core;
using SentryQA.UITests.Pages;

namespace SentryQA.UITests.Tests;

[TestFixture]
public class PricePulseSearchTests
{
    private IWebDriver _driver = null!;
    private PricePulseSearchPage _searchPage = null!;

    [SetUp]
    public void SetUp()
    {
        _driver = DriverFactory.Create();
        _searchPage = new PricePulseSearchPage(_driver);
        _searchPage.Navigate();
    }

    [TearDown]
    public void TearDown()
    {
        if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            var shotDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "screenshots");
            Directory.CreateDirectory(shotDir);
            var path = Path.Combine(shotDir, $"{TestContext.CurrentContext.Test.Name}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.png");
            ((ITakesScreenshot)_driver).GetScreenshot().SaveAsFile(path);
            TestLogger.Info($"Saved failure screenshot: {path}");
        }
        
        _driver?.Quit();
        _driver?.Dispose();
    }

    [Test]
    [RetryTest(2)]
    public void Search_ForKnownProduct_ReturnsAtLeastOneResult()
    {
        _searchPage.SearchProduct("laptop");
        var results = _searchPage.GetResultCards();

        Assert.That(results.Count, Is.GreaterThan(0),
            "Expected at least one result card after searching for a common product term.");
    }

    [Test]
    public void Search_ForKnownProduct_RendersPriceChart()
    {
        _searchPage.SearchProduct("laptop");

        Assert.That(_searchPage.IsPriceChartVisible(), Is.True,
            "Price history chart should render once a product is selected/searched.");
    }

    [TestCase("laptop")]
    [TestCase("headphones")]
    [TestCase("monitor")]
    public void Search_ForVariousProducts_FirstResultTitleContainsSearchTerm(string term)
    {
        _searchPage.SearchProduct(term);
        var title = _searchPage.GetFirstResultTitle();

        Assert.That(title.ToLowerInvariant(), Does.Contain(term.ToLowerInvariant()).IgnoreCase,
            $"Expected the top result for '{term}' to reference the search term.");
    }
}