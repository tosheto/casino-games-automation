using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Casino.Games.Drivers
{
    public enum ExecutionProfile
    {
        Desktop,
        Mobile
    }

    public static class PlaywrightDriver
    {
        public static async Task<(IPlaywright playwright, IBrowser browser, IBrowserContext context, IPage page)>
            CreateAsync(ExecutionProfile profile)
        {
            var playwright = await Playwright.CreateAsync();

            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,   // да виждаш какво става
                SlowMo = 100        // малко забавяне за наблюдение
            });

            BrowserNewContextOptions contextOptions;

            if (profile == ExecutionProfile.Mobile)
            {
                var iphone = playwright.Devices["iPhone 13"];
                contextOptions = new BrowserNewContextOptions
                {
                    ViewportSize = iphone.ViewportSize,
                    UserAgent = iphone.UserAgent,
                    DeviceScaleFactor = iphone.DeviceScaleFactor,
                    IsMobile = iphone.IsMobile,
                    HasTouch = iphone.HasTouch
                };
            }
            else
            {
                // 1280x800 за лаптоп екран
                contextOptions = new BrowserNewContextOptions
                {
                    ViewportSize = new ViewportSize
                    {
                        Width = 1280,
                        Height = 800
                    }
                };
            }

            var context = await browser.NewContextAsync(contextOptions);
            var page = await context.NewPageAsync();

            TestContext.WriteLine($"[PlaywrightDriver] Created {profile} context (headed {contextOptions.ViewportSize.Width}x{contextOptions.ViewportSize.Height}).");

            return (playwright, browser, context, page);
        }
    }
}
