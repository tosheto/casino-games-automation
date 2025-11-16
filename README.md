# Casino Games Automation (Playwright + NUnit)

This repository contains a small but production-style UI automation framework built on top of **.NET 9**, **Microsoft Playwright**, and **NUnit**.  
The focus of the sample implementation is the **Irish Wilds** slot game and automated tests that validate **win/loss balance updates after multiple spins** on both desktop and mobile.

---

## Features

- Structured, maintainable automation framework:
  - Centralized Playwright driver for desktop and mobile.
  - Page Object model for the *Irish Wilds* game.
  - Configurable URLs and selectors via `TestSettings`.
  - Rich logging, HTML dumps, and screenshots for troubleshooting.
- Automated regression tests:
  - Verify that the **player balance changes** after each spin.
  - Verify that the **net balance changes** after a sequence of spins.
  - Run on **desktop** and **mobile** emulation.
- Ready for CI:
  - GitHub Actions workflow (matrix for desktop + mobile).
  - `dotnet test`-based pipeline, easy to integrate.

---

## Project structure

The relevant parts of the solution are organized as follows:

```text
.
├─ src/
│  └─ Casino.Games/
│     ├─ Drivers/
│     │  └─ PlaywrightDriver.cs
│     ├─ Pages/
│     │  ├─ IrishWildsPage.cs
│     │  └─ IrishWildsPage.Space.cs
│     ├─ Configuration/
│     │  └─ TestSettings.cs
│     ├─ Utils/
│     │  └─ BalanceHelper.cs
│     │  └─ ReportingHelper.cs
│     └─ Tests/
│        └─ IrishWildsBalanceTests.cs
└─ .github/
   └─ workflows/
      └─ ci.yml
---


- **Prerequisites
Before you run the tests locally, make sure you have: .NET SDK 9.0 (or later) installed
You can verify with:
dotnet --version

Playwright browsers installed
The first dotnet test run will typically download the required browsers automatically.
If needed, you can manually install them with:
dotnet tool install --global Microsoft.Playwright.CLI
playwright install
