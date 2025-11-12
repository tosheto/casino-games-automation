using System;
using System.IO;
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

        public static async Task<PlaywrightDriver> CreateDesktopAsync(bool headless)
        {
            var driver = new PlaywrightDriver();

            driver.Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            driver.Browser = await driver.Playwright.Chromium.LaunchAsync(new()
            {
                Headless = headless,
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
            TestContext.WriteLine($"[PlaywrightDriver] Created Desktop context (headless={headless}).");

            return driver;
        }

        public static async Task<PlaywrightDriver> CreateMobileAsync(bool headless)
        {
            var driver = new PlaywrightDriver();

            driver.Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            var device = driver.Playwright.Devices["iPhone 13 Pro"];

            driver.Browser = await driver.Playwright.Chromium.LaunchAsync(new()
            {
                Headless = headless,
            });

            driver.Context = await driver.Browser.NewContextAsync(new BrowserNewContextOptions(device)
            {
                RecordVideoDir = TestSettings.VideoOutputDir
            });

            driver.Page = await driver.Context.NewPageAsync();
            TestContext.WriteLine($"[PlaywrightDriver] Created Mobile context (headless={headless}, device=iPhone 13 Pro).");

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
