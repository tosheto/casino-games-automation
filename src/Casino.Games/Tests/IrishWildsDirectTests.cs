using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Casino.Games.Tests
{
    [TestFixture]
    public class IrishWildsDirectTests
    {
        private IPlaywright _playwright;
        private IBrowser _browser;
        private IBrowserContext _context;
        private IPage _page;

        // директен линк към играта
        private const string GameUrl =
            "https://d21j22mhfwmuah.cloudfront.net/0Debug/SB_HTML5_IrishWilds94/index.html?gameCode=SB_HTML5_IrishWilds94&token=DEMO_PP_b56f9b8f-728e-4746-ae05-bc91f84dbdef&homeUrl=spinberrysite&rgsUrl=https://rgstorgs.stage.pariplaygames.com&lang=EN&DebugMode=False&currencyCode=USD&disableRotation=False&ExtraData=networkId%3dPP&HideCoins=True&CoinsDefault=True";

        private const string CanvasSelector = "#game_container canvas";

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,                 // да се вижда браузърът
                SlowMo = 250,                     // по-ясна визуализация
                Args = new[] { "--start-maximized" }
            });

            _context = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new() { Width = 1280, Height = 800 },
                IgnoreHTTPSErrors = true,
                BypassCSP = true,
                DeviceScaleFactor = 1
            });

            await _context.Tracing.StartAsync(new()
            {
                Screenshots = true,
                Snapshots = true,
                Sources = true
            });

            _page = await _context.NewPageAsync();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            try { await _context.Tracing.StopAsync(new() { Path = "trace.zip" }); } catch { }
            await _context.CloseAsync();
            await _browser.CloseAsync();
            _playwright.Dispose();
        }

        [Test]
        public async Task IrishWilds_Direct_OpenAndSpin()
        {
            // 1) Отваряме директно хоста на играта
            await _page.GotoAsync(GameUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

            // 2) Изчакваме да се появи canvas
            var canvas = await _page.WaitForSelectorAsync(CanvasSelector, new PageWaitForSelectorOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });
            Assert.That(canvas, Is.Not.Null, "Canvas not found.");

            // 3) Снимка преди spin
            await _page.ScreenshotAsync(new PageScreenshotOptions { Path = "01_before_spin.png", FullPage = true });

            // 4) Клик по относителни координати (център-долу ~ Spin)
            await ClickOnCanvasAsync(_page, CanvasSelector, 0.50f, 0.92f);

            // 5) Малко изчакване за анимации
            await _page.WaitForTimeoutAsync(2000);

            // 6) Снимка след spin
            await _page.ScreenshotAsync(new PageScreenshotOptions { Path = "02_after_spin.png", FullPage = true });

            // 7) Проверка, че страницата е жива (не е затворена)
            Assert.That(_page.IsClosed, Is.False, "Page is closed unexpectedly after spin.");
        }

        private static async Task ClickOnCanvasAsync(IPage page, string canvasSelector, float rx, float ry)
        {
            var bb = await page.EvalOnSelectorAsync<BoundingBoxDto>(canvasSelector, @"el => {
                const r = el.getBoundingClientRect();
                return { x: r.x, y: r.y, w: r.width, h: r.height };
            }");

            // Пресмятаме координати и ги кастваме към float за Mouse.MoveAsync
            var x = (float)(bb.x + bb.w * rx);
            var y = (float)(bb.y + bb.h * ry);

            await page.Mouse.MoveAsync(x, y);
            await page.Mouse.DownAsync();
            await page.Mouse.UpAsync();
        }

        private record BoundingBoxDto(double x, double y, double w, double h);
    }
}
