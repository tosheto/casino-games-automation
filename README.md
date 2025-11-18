# 🎰 Todor Stavrev's Casino Games Automation Project

[![CI - Casino.Games Desktop & Mobile](https://github.com/tosheto/casino-games-automation/actions/workflows/ci.yml/badge.svg)](https://github.com/tosheto/casino-games-automation/actions/workflows/ci.yml)
![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)
![Playwright](https://img.shields.io/badge/Playwright-.NET%20bindings-2EAD33?logo=microsoft-playwright&logoColor=white)
![NUnit](https://img.shields.io/badge/NUnit-tests-13854E)
![Allure](https://img.shields.io/badge/Allure-reporting-FF45A1)
![Status](https://img.shields.io/badge/Status-Portfolio%20Ready-brightgreen)
![Domain](https://img.shields.io/badge/Domain-Casino%20%2F%20Gambling-blueviolet)
**Production-style UI automation framework** built with **.NET 9, Microsoft Playwright, and NUnit**.


The casino slot game under test is the **Irish Wilds** 
The focus of the tests is win/loss balance behavior after multiple spins on both desktop and mobile, with special emphasis on **automating a canvas-based slot game, where traditional DOM locators cannot be used**.
Interaction with the Irish Wilds slot is achieved through **direct canvas event injection, keyboard emulation, and smart synchronization logic, enabling reliable end-to-end validation of game behavior without relying on visible UI elements**.

---

## 🤝 Connect with me

[![LinkedIn](https://img.shields.io/badge/LinkedIn-Profile-0A66C2?logo=linkedin&logoColor=white)](https://www.linkedin.com/in/todor-stavrev-7843105b)

---

## 📊 Live Reports

All reports are generated and published on **every push/PR to `main`** via GitHub Actions.

- **Allure Report** — suites, steps, categories, trends & analytics  
  👉 https://tosheto.github.io/casino-games-automation/

---

## 🧱 Tech stack

- **Language / runtime:** .NET 9 (C#)
- **Test framework:** NUnit
- **Browser automation:** Microsoft Playwright (.NET bindings)
- **Reporting:** Allure .NET + CI-generated static Allure report
- **Architecture:**
  - **Central Playwright driver** (desktop & mobile)  
    `src/Casino.Games/Drivers/PlaywrightDriver.cs`
  - **Page Object Model (POM)** for Irish Wilds  
    `src/Casino.Games/Pages/IrishWildsPage*.cs`
  - **Config layer** — URLs, selectors, timeouts  
    `src/Casino.Games/Configuration/TestSettings.cs`
  - **Helpers & utilities** — balance math, reporting helpers  
    `src/Casino.Games/Utils/*`
- **CI/CD:** GitHub Actions – `.github/workflows/ci.yml`
- **Artifacts:**
  - TRX results (desktop + mobile)
  - Allure results + merged Allure report (published to GitHub Pages)

---

## ⚙️ CI pipeline (GitHub Actions)

**Workflow:** `.github/workflows/ci.yml`  

On every push / PR to `main`, the pipeline:

1. **Restores and builds** the solution (`Casino.Games` project).
2. **Installs Playwright browsers** on the GitHub runner.
3. **Runs NUnit tests in a matrix**:
   - `desktop` (Chromium / Firefox / WebKit)
   - `mobile` (Chromium / WebKit, mobile emulation)
4. **Collects artifacts**:
   - TRX per target (`desktop`, `mobile`)
   - Allure results per target (desktop/mobile)
5. **Merges Allure results** from all runs.
6. **Generates Allure HTML report** from the merged results.
7. **Deploys the Allure report to GitHub Pages** (`gh-pages` branch).

Result:  👉 https://tosheto.github.io/casino-games-automation/

---

## ✅ What this project tests

| Area                     | Test file / suite                                            | What it verifies                                                     | Key assertions / notes                                                                 |
|--------------------------|--------------------------------------------------------------|-----------------------------------------------------------------------|----------------------------------------------------------------------------------------|
| Irish Wilds – Desktop    | `src/Casino.Games/Tests/IrishWildsBalanceTests.cs`<br/>`IrishWildsBalance_Desktop` | Balance changes after multiple spins on desktop browsers              | Runs multiple spins; asserts **final balance ≠ initial balance** across Chromium/FF/WebKit |
| Irish Wilds – Mobile     | `src/Casino.Games/Tests/IrishWildsBalanceTests.cs`<br/>`IrishWildsBalance_Mobile`  | Balance and win/loss behavior in mobile emulation (slot canvas UX)   | iPhone 13 Pro emulation; spins the game via canvas + keyboard; validates balance deltas    |
| Balance helper logic     | `src/Casino.Games/Utils/BalanceHelper.cs`                   | Parsing & comparison of balance / stake values                        | Robust parsing from mixed UI text (e.g. `BALANCE $2,000.00`)                               |
| Reporting integration    | `src/Casino.Games/Utils/ReportingHelper.cs`                 | Allure step structure + diagnostics                                   | Groups console output by step (open game, spins, verification)                            |

> The Irish Wilds tests run against the configured `GameUrl` from `TestSettings`, on both desktop and mobile contexts.

---

## 🚀 Quick start (local)

Prerequisites:

- **.NET SDK 9.0+**
- Playwright CLI and browsers installed

```bash
# From repo root

# 1) Restore & build
dotnet restore src/Casino.Games/Casino.Games.csproj
dotnet build src/Casino.Games/Casino.Games.csproj --configuration Debug

# 2) Install Playwright browsers (first run only)
pwsh ./src/Casino.Games/bin/Debug/net9.0/playwright.ps1 install --with-deps

# 3) Run tests (desktop + mobile categories)
dotnet test src/Casino.Games/Casino.Games.csproj --configuration Debug
