using Serilog;

namespace SentryQA.Core;

/// <summary>
/// Thin wrapper around Serilog so every layer of the framework logs to both
/// the console (for local runs) and a rolling file under /logs (picked up
/// as a CI artifact alongside the HTML test report).
/// </summary>
public static class TestLogger
{
    private static readonly Lazy<Serilog.ILogger> _logger = new(() => new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.File("logs/sentryqa-.log", rollingInterval: RollingInterval.Day)
        .CreateLogger());

    public static void Info(string message) => _logger.Value.Information(message);
    public static void Warn(string message) => _logger.Value.Warning(message);
    public static void Error(string message, Exception? ex = null) => _logger.Value.Error(ex, message);

    /// <summary>Logs which locator strategy a self-healing lookup actually succeeded with,
    /// so flaky selectors show up in reports before they start failing outright.</summary>
    public static void LocatorHealed(string elementName, int strategyIndexUsed, int totalStrategies)
    {
        if (strategyIndexUsed == 0) return; // primary locator worked, nothing to report
        Warn($"[SELF-HEAL] '{elementName}' needed fallback locator {strategyIndexUsed + 1}/{totalStrategies}. " +
             "Primary locator is likely stale — consider updating it before it fails outright.");
    }
}
