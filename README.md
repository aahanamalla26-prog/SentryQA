# SentryQA

A C#/.NET test automation framework with **self-healing UI locators** and
**flakiness-aware retries**, exercising both the UI and REST API of
[PricePulse](https://ai-price-predictions.vercel.app) end-to-end.

Built to demonstrate SDET fundamentals — not just "can write a Selenium
test," but framework design: layered architecture, resilient locators,
CI-integrated reporting, and API + UI coverage of a real, live system.

## Why it's different from a typical portfolio automation project

Most beginner automation repos are a flat folder of `[Test]` methods with
hard-coded CSS selectors, run once locally, and never touched again. Two
things here specifically push past that:

1. **Self-healing locators** (`SentryQA.Core/SelfHealingLocator.cs`) — every
   UI element is defined by a ranked list of fallback selectors
   (`data-testid` → `aria-label` → visible text → generic CSS), not a single
   brittle one. When a test has to fall back past the primary selector, it
   doesn't just pass silently — it logs a `[SELF-HEAL]` warning naming the
   element, so a selector that's *about* to break shows up in CI output
   before it actually breaks a build.

2. **Flakiness-aware retry** (`SentryQA.Core/RetryTestAttribute.cs`) — a
   `[RetryTest]` test that only passes on a later attempt is reported as
   `Passed (Flaky, attempt 2/2)`, not a clean green pass. This keeps CI
   unblocked without hiding the flakiness that a plain retry-and-forget
   approach sweeps under the rug.

Both directly reflect a real problem test-automation teams have (locator
rot and flaky-test blindness) rather than being framework features for
their own sake — worth mentioning if asked about it in an interview.

## Architecture

```
SentryQA.sln
├── SentryQA.Core          shared framework: config, driver factory,
│                           self-healing locator, retry attribute, logging
├── SentryQA.UITests        Selenium + NUnit, Page Object Model
│   └── Pages/               one class per screen, all element lookups
│                             go through SelfHealingLocator
└── SentryQA.ApiTests       RestSharp + NUnit, schema/status/latency/fuzz
    └── Clients/              typed wrapper over raw REST calls
```

Target system: **PricePulse**, a live price-tracking/prediction platform
(React + FastAPI + PostgreSQL) — see the `Clients/PricePulseApiClient.cs`
and `Pages/PricePulseSearchPage.cs` docstrings for the exact endpoints and
elements under test. Point `appsettings.json`'s `BaseUiUrl`/`BaseApiUrl` at
any other app to reuse the framework — nothing else needs to change.

## What's covered

- **UI**: search flow, result rendering, price-chart rendering, data-driven
  cases across multiple product terms, automatic failure screenshots
- **API**: status codes, response schema validation, latency threshold,
  input-validation edge cases, and a randomized "fuzz" test (via
  [Bogus](https://github.com/bchavez/Bogus)) that throws 15 realistic but
  unseen search terms at the endpoint and asserts none of them 500

## Running locally

Requires the .NET 8 SDK and Chrome installed.

```bash
dotnet restore
dotnet build

# API suite
dotnet test src/SentryQA.ApiTests/SentryQA.ApiTests.csproj

# UI suite (headless by default — set "Headless": false in appsettings.json to watch it run)
dotnet test src/SentryQA.UITests/SentryQA.UITests.csproj
```

## CI

`.github/workflows/ci.yml` runs the full suite on every push/PR, plus a
daily scheduled smoke run against the live deployment, and uploads test
results, logs, and failure screenshots as build artifacts.

## Adapting the endpoints/selectors

The API base paths (`/search`, `/history/{id}`, `/predict/{id}`) and UI
selectors in this repo are written to match PricePulse's actual shape as
described in its own README — swap the strings in `PricePulseApiClient.cs`
and `PricePulseSearchPage.cs` for your target app's real routes/DOM before
running against something else.

## Resume framing

> Built a C#/.NET test automation framework with self-healing locators and
> flakiness-aware retry logic, as measured by full UI + API coverage of a
> live production app with zero manual re-runs for transient failures, by
> implementing a fallback-locator resolution strategy and a custom NUnit
> retry command with flaky-pass reporting.
