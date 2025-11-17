using System;
using System.Threading.Tasks;
using Casino.Games.Configuration;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Casino.Games.Drivers
{
    public class PlaywrightDriver : IAsyncDisposable
    {
        public IPlaywright Playwright { get; private set; }
        public IBrowser Browser { get; private set; }
        public IBrowserContext Context { get; private set; }
        public IPage Page { get; private set; }

        private PlaywrightDriver() { }

        private static bool ResolveHeadless(bool requestedHeadless)
        {
            var inCi = string.Equals(
                Environment.GetEnvironmentVariable("GITHUB_ACTIONS"),
                "true",
                StringComparison.OrdinalIgnoreCase);

            return inCi ? true : requestedHeadless;
        }

        private static IBrowserType ResolveBrowserType(IPlaywright playwright, string browserName)
        {
            switch (browserName?.ToLowerInvariant())
            {
                case "firefox":
                    return playwright.Firefox;
                case "webkit":
                    return playwright.Webkit;
                default:
                    return playwright.Chromium;
            }
        }

        public static async Task<PlaywrightDriver> CreateDesktopAsync(string browser, bool headless)
        {
            var driver = new PlaywrightDriver();
            driver.Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

            var headlessFinal = ResolveHeadless(headless);
            var browserType = ResolveBrowserType(driver.Playwright, browser);

            driver.Browser = await browserType.LaunchAsync(new() { Headless = headlessFinal });

            driver.Context = await driver.Browser.NewContextAsync(new()
            {
                ViewportSize = new ViewportSize { Width = 1366, Height = 540 },
                RecordVideoDir = TestSettings.VideoOutputDir
            });

            driver.Page = await driver.Context.NewPageAsync();
            TestContext.WriteLine($"[PlaywrightDriver] Desktop ({browser}) mode: {(headlessFinal ? "headless" : "headed")}");

            return driver;
        }

        public static async Task<PlaywrightDriver> CreateMobileAsync(string browser, bool headless)
        {
            var driver = new PlaywrightDriver();
            driver.Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

            var device = driver.Playwright.Devices["iPhone 13 Pro"];
            var headlessFinal = ResolveHeadless(headless);
            var browserType = ResolveBrowserType(driver.Playwright, browser);

            driver.Browser = await browserType.LaunchAsync(new() { Headless = headlessFinal });

            driver.Context = await driver.Browser.NewContextAsync(new BrowserNewContextOptions(device)
            {
                RecordVideoDir = TestSettings.VideoOutputDir
            });

            driver.Page = await driver.Context.NewPageAsync();
            TestContext.WriteLine($"[PlaywrightDriver] Mobile ({browser}) iPhone 13 mode: {(headlessFinal ? "headless" : "headed")}");

            return driver;
        }

        public async ValueTask DisposeAsync()
        {
            try { if (Context != null) await Context.CloseAsync(); } catch { }
            try { if (Browser != null) await Browser.CloseAsync(); } catch { }

            Playwright?.Dispose();
        }
    }
}
