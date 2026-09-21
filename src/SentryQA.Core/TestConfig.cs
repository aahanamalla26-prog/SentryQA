using Microsoft.Extensions.Configuration;

namespace SentryQA.Core;

/// <summary>
/// Central, strongly-typed access to run configuration (appsettings.json,
/// overridable via environment variables so CI can inject different values
/// without touching the file).
/// </summary>
public static class TestConfig
{
    private static readonly IConfigurationRoot _config = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
        .AddEnvironmentVariables(prefix: "SENTRYQA_")
        .Build();

    public static string BaseUiUrl => _config["BaseUiUrl"] ?? "https://ai-price-predictions.vercel.app";
    public static string BaseApiUrl => _config["BaseApiUrl"] ?? "https://ai-price-predictions.vercel.app/api";
    public static string Browser => _config["Browser"] ?? "Chrome";
    public static bool Headless => bool.TryParse(_config["Headless"], out var h) ? h : true;
    public static int DefaultTimeoutSeconds => int.TryParse(_config["DefaultTimeoutSeconds"], out var t) ? t : 10;
    public static int RetryCount => int.TryParse(_config["RetryCount"], out var r) ? r : 2;
}
