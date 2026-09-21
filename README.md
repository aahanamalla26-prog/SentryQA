<div align="center">

# 🛡️ SentryQA

### A modular .NET 8 test automation framework for API & UI quality assurance

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![NUnit](https://img.shields.io/badge/NUnit-Test%20Runner-06A0CE?style=for-the-badge)](https://nunit.org/)
[![Selenium](https://img.shields.io/badge/Selenium-WebDriver-43B02A?style=for-the-badge&logo=selenium&logoColor=white)](https://www.selenium.dev/)

*Built to test PricePulse end-to-end — API contracts and UI flows, in one clean architecture.*

[Demo](#-demo) • [Features](#-features) • [Tech Stack](#-tech-stack) • [Architecture](#-architecture) • [Getting Started](#-getting-started)

</div>

---

## ✨ Features

- 🧩 **Modular architecture** — three independently testable projects: `Core`, `ApiTests`, `UITests`
- 🌐 **API testing** with RestSharp — full request/response validation against PricePulse endpoints
- 🖥️ **UI automation** with Selenium WebDriver, built on the **Page Object Model** for maintainability
- ✅ **Fluent, readable assertions** via FluentAssertions
- ⚙️ **Environment-aware config** — `appsettings.json` with environment-variable overrides, ready for CI/CD
- 🔁 **CI/CD-ready** — designed to run headless in a pipeline (GitHub Actions example below)

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Language / Runtime | C# · .NET 8.0 |
| Test Runner | NUnit |
| Assertions | FluentAssertions |
| API Testing | RestSharp |
| UI Automation | Selenium WebDriver + ChromeDriver |
| Configuration | Microsoft.Extensions.Configuration |

---

## 🏗️ Architecture

<img width="1408" height="768" alt="Gemini_Generated_Image_mwjbt7mwjbt7mwjb" src="https://github.com/user-attachments/assets/d9305ddf-34db-4ee8-9296-ad49d6242672" />

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Chrome (for UI tests via ChromeDriver)

### Installation

```bash
git clone https://github.com/<your-username>/SentryQA.git
cd SentryQA
dotnet restore
```

### Configuration

Update `appsettings.json` in `SentryQA.Core` with your target base URLs, or override via environment variables:

```bash
export PricePulse__BaseUrl="https://your-api-url.com"
```

### Running Tests

```bash
# Run everything
dotnet test

# Run only API tests
dotnet test SentryQA.ApiTests

# Run only UI tests
dotnet test SentryQA.UITests
```
---

