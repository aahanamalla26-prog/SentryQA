using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SentryQA.Core;

/// <summary>
/// A resilient element locator that tries an ordered list of fallback
/// <see cref="By"/> strategies instead of a single hard-coded selector.
///
/// Why: the #1 cause of "flaky" UI automation isn't timing, it's brittle
/// locators — a dev renames a CSS class or restructures a div, and every
/// test that depended on it breaks even though the feature works fine.
///
/// This class treats that as an expected failure mode instead of a
/// surprise: each element is described by 2-4 locator strategies ranked
/// from most-specific to most-durable (e.g. data-testid -> aria-label ->
/// visible text -> CSS). It tries them in order, logs whenever it had to
/// fall back past the first one (see TestLogger.LocatorHealed), and only
/// fails the test if every strategy misses. That log line is the "healing
/// signal" — it flags selectors worth fixing before they go fully stale,
/// instead of the team finding out from a red CI run.
/// </summary>
public class SelfHealingLocator
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public SelfHealingLocator(IWebDriver driver, int timeoutSeconds = 10)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
    }

    /// <summary>
    /// Finds an element by trying each locator in order. The first strategy
    /// that resolves to a visible element wins; later strategies are only
    /// attempted if earlier ones fail or time out.
    /// </summary>
    public IWebElement Find(string elementName, params By[] strategies)
    {
        if (strategies.Length == 0)
            throw new ArgumentException("At least one locator strategy is required.", nameof(strategies));

        var exceptions = new List<Exception>();

        for (int i = 0; i < strategies.Length; i++)
        {
            try
            {
                var element = _wait.Until(drv =>
                {
                    var found = drv.FindElements(strategies[i]).FirstOrDefault(e => e.Displayed);
                    return found;
                });

                if (element is not null)
                {
                    TestLogger.LocatorHealed(elementName, i, strategies.Length);
                    return element;
                }
            }
            catch (WebDriverTimeoutException ex)
            {
                exceptions.Add(ex);
            }
            catch (NoSuchElementException ex)
            {
                exceptions.Add(ex);
            }
        }

        throw new NoSuchElementException(
            $"SelfHealingLocator: all {strategies.Length} strategies failed for '{elementName}'. " +
            $"Tried: {string.Join(" | ", strategies.Select(s => s.ToString()))}");
    }

    /// <summary>Convenience overload for click-and-continue interactions.</summary>
    public void Click(string elementName, params By[] strategies) => Find(elementName, strategies).Click();

    /// <summary>Convenience overload for text entry with a clear-first guarantee.</summary>
    public void Type(string elementName, string text, params By[] strategies)
    {
        var el = Find(elementName, strategies);
        el.Clear();
        el.SendKeys(text);
    }
}
