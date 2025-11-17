using System;
using System.Threading.Tasks;
using Casino.Games.Configuration;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Casino.Games.Drivers
{
    public class PlaywrightDriver : IAsyncDisposable
    {
        public IPlaywright Playwright { get; private set; } = default!;
        public IBrowser Browser { get; private set; } = default!;
        public IBrowserContext Context { get; private set; } = default!;
        public IPage Page { get; private set; } = default!;

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
            return browserName?.ToLowerInvariant() switch
            {
                "firefox" => playwright.Firefox,
                "webkit"  => playwright.Webkit,
                _         => playwright.Chromium
            };
        }

        public static async Task<PlaywrightDriver> CreateDesktopAsync(string browser, bool headless)
        {
            var driver = new PlaywrightDriver();

            driver.Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            var headlessFinal = ResolveHeadless(headless);
            var browserType = ResolveBrowserType(driver.Playwright, browser);

            driver.Browser = await browserType.LaunchAsync(new()
            {
                Headless = headlessFinal
            });

            driver.Context = await driver.Browser.NewContextAsync(new()
            {
                ViewportSize = new ViewportSize
                {
                    Width = 1366,
                    Height = 540
                },
                RecordVideoDir = TestSettings.VideoOutputDir
            });

            driver.Page = await driver.Context.NewPageAsync();

            var mode = headlessFinal ? "headless" : "headed";
            TestContext.WriteLine($"[PlaywrightDriver] Created Desktop context ({browser}, {mode}).");

            return driver;
        }

        public static async Task<PlaywrightDriver> CreateMobileAsync(string browser, bool headless)
        {
            var driver = new PlaywrightDriver();

            driver.Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            var device = driver.Playwright.Devices["iPhone 13 Pro"];

            var headlessFinal = ResolveHeadless(headless);
            var browserType = ResolveBrowserType(driver.Playwright, browser);

            driver.Browser = await browserType.LaunchAsync(new()
            {
                Headless = headlessFinal
            });

            driver.Context = await driver.Browser.NewContextAsync(
                new BrowserNewContextOptions(device)
                {
                    RecordVideoDir = TestSettings.VideoOutputDir
                });

            driver.Page = await driver.Context.NewPageAsync();

            var mode = headlessFinal ? "headless" : "headed";
            TestContext.WriteLine($"[PlaywrightDriver] Created Mobile context ({browser}, iPhone 13 Pro emulation, {mode}).");

            return driver;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (Context != null)
                    await Context.CloseAsync();
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[PlaywrightDriver] Error closing context: {ex.Message}");
            }

            try
            {
                if (Browser != null)
                    await Browser.CloseAsync();
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[PlaywrightDriver] Error closing browser: {ex.Message}");
            }

            Playwright?.Dispose();
        }
    }
}
