using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

namespace SentryQA.Core;

/// <summary>Builds a browser driver from TestConfig, so CI (headless) and
/// local dev (headed, for debugging) need zero code changes — only an
/// appsettings.json or env-var flip.</summary>
public static class DriverFactory
{
    public static IWebDriver Create()
    {
        return TestConfig.Browser.ToLowerInvariant() switch
        {
            "firefox" => CreateFirefox(),
            _ => CreateChrome(),
        };
    }

    private static IWebDriver CreateChrome()
    {
        var options = new ChromeOptions();
        if (TestConfig.Headless)
        {
            options.AddArgument("--headless=new");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
        }
        options.AddArgument("--window-size=1440,900");

        var driver = new ChromeDriver(options);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero; // explicit waits only — see SelfHealingLocator
        TestLogger.Info($"ChromeDriver started (headless={TestConfig.Headless})");
        return driver;
    }

    private static IWebDriver CreateFirefox()
    {
        var options = new FirefoxOptions();
        if (TestConfig.Headless) options.AddArgument("-headless");

        var driver = new FirefoxDriver(options);
        TestLogger.Info($"FirefoxDriver started (headless={TestConfig.Headless})");
        return driver;
    }
}
