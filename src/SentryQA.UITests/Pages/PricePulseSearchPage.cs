using OpenQA.Selenium;
using SentryQA.Core;

namespace SentryQA.UITests.Pages;

/// <summary>
/// Page Object for PricePulse's search/results screen. Every element is
/// described by ranked fallback locators — the primary strategy targets a
/// data-testid where one exists, falling back to role/aria and finally to
/// visible text, so the tests survive routine markup churn (a div becoming
/// a section, a class rename, etc.) instead of breaking on it.
/// </summary>
public class PricePulseSearchPage
{
    private readonly IWebDriver _driver;
    private readonly SelfHealingLocator _locator;

    public PricePulseSearchPage(IWebDriver driver)
    {
        _driver = driver;
        _locator = new SelfHealingLocator(driver, TestConfig.DefaultTimeoutSeconds);
    }

    public void Navigate() => _driver.Navigate().GoToUrl(TestConfig.BaseUiUrl);

    public void SearchProduct(string productName)
    {
        _locator.Type("SearchInput", productName,
            By.CssSelector("[data-testid='product-search-input']"),
            By.CssSelector("input[aria-label='Search products']"),
            By.CssSelector("input[type='search']"),
            By.CssSelector("input[placeholder*='Search']"));

        _locator.Click("SearchSubmitButton",
            By.CssSelector("[data-testid='search-submit']"),
            By.CssSelector("button[type='submit']"),
            By.XPath("//button[contains(., 'Search')]"));
    }

    public IReadOnlyList<IWebElement> GetResultCards()
    {
        var container = _locator.Find("ResultsContainer",
            By.CssSelector("[data-testid='results-list']"),
            By.CssSelector("[role='list'].results"),
            By.CssSelector(".results-container"));

        return container.FindElements(By.CssSelector("[data-testid='result-card'], .result-card"));
    }

    public bool IsPriceChartVisible()
    {
        try
        {
            _locator.Find("PriceChart",
                By.CssSelector("[data-testid='price-chart']"),
                By.CssSelector("svg.recharts-surface"),
                By.CssSelector(".price-chart"));
            return true;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    public string GetFirstResultTitle()
    {
        var cards = GetResultCards();
        if (cards.Count == 0)
            throw new NoSuchElementException("No result cards rendered after search.");
        return cards[0].Text;
    }
}
