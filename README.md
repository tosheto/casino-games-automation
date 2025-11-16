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
    
## Prerequisites
 - Before you run the tests locally, make sure you have: .NET SDK 9.0 (or later) installed
You can verify with:
dotnet --version

  - Playwright browsers installed
  - The first dotnet test run will typically download the required browsers automatically. You can manually install them with:
    dotnet tool install --global Microsoft.Playwright.CLI
    playwright install
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
```text
. 
Triggering spins using the space key

The method PressSpaceForSpinAsync encapsulates how a spin is triggered:
Ensure the correct game frame is used (_gameFrame or main frame).

Try to click the <canvas> once to give it focus.

Execute a JavaScript snippet that dispatches keydown and keyup events for the Space key:

document.dispatchEvent(new KeyboardEvent('keydown', { key:' ', code:'Space', keyCode:32, which:32, bubbles:true }));
document.dispatchEvent(new KeyboardEvent('keyup',   { key:' ', code:'Space', keyCode:32, which:32, bubbles:true }));


Click the <canvas> again and call:
await _gamePage.Keyboard.PressAsync(" ");


Wait a short, configurable timeout after the spin is triggered.

All those steps are logged via TestContext.WriteLine, so the test output clearly shows when each spin was initiated.

Detecting when a spin has completed

The method WaitForSpinToCompleteAsync is the core of the win/loss validation flow.
It takes the previous balance as a parameter:

public async Task WaitForSpinToCompleteAsync(decimal previousBalance)

The logic is:
Set a deadline using TestSettings.SpinWaitTimeoutMs.
Immediately wait a short initial delay (for the reels to start spinning).

Inside a loop, until the deadline:
Call GetBalanceAsync() to read the current balance from the UI.
If currentBalance != previousBalance, log:
[IrishWildsPage] Balance changed. Spin complete.


and return.

Otherwise, wait a small interval and try again.

If the deadline is reached without a change:

[IrishWildsPage] Spin wait timeout reached.
