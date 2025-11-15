using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Casino.Games.Pages
{
    public partial class IrishWildsPage
    {
        public async Task PressSpaceForSpinAsync()
        {
            TestContext.WriteLine("[IrishWildsPage] PressSpaceForSpinAsync started.");

            var frame = _gameFrame ?? _gamePage.MainFrame;

            // 1) First click on canvas to ensure focus
            try
            {
                var canvas = frame.Locator("canvas");
                var count = await canvas.CountAsync();
                TestContext.WriteLine($"[IrishWildsPage] Canvas count = {count}");

                if (count > 0)
                {
                    TestContext.WriteLine("[IrishWildsPage] First canvas click for focus.");
                    await canvas.First.ClickAsync(new LocatorClickOptions
                    {
                        Force = true,
                        Timeout = 5000
                    });

                    await _gamePage.WaitForTimeoutAsync(200); // was 500
                }
                else
                {
                    TestContext.WriteLine("[IrishWildsPage] No <canvas> found.");
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[IrishWildsPage] Canvas focus error: {ex.Message}");
            }

            // 2) JS space key injection
            const string js =
                "document.dispatchEvent(new KeyboardEvent('keydown', {key:' ', code:'Space', keyCode:32, which:32, bubbles:true}));" +
                "document.dispatchEvent(new KeyboardEvent('keyup',   {key:' ', code:'Space', keyCode:32, which:32, bubbles:true}));";

            try
            {
                await frame.EvaluateAsync(js);
                TestContext.WriteLine("[IrishWildsPage] JS Space injection executed.");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[IrishWildsPage] JS injection failed: {ex.Message}");
            }

            // 3) Second canvas click + Keyboard.Press(" ")
            try
            {
                var canvas = frame.Locator("canvas");
                var count = await canvas.CountAsync();

                if (count > 0)
                {
                    TestContext.WriteLine("[IrishWildsPage] Second canvas click + Keyboard.Press(\" \").");
                    await canvas.First.ClickAsync(new LocatorClickOptions
                    {
                        Force = true,
                        Timeout = 5000
                    });

                    await _gamePage.Keyboard.PressAsync(" ");
                }
                else
                {
                    TestContext.WriteLine("[IrishWildsPage] No <canvas> for second click.");
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[IrishWildsPage] Second focus + Keyboard.Press error: {ex.Message}");
            }

            // shorter tail wait – WaitForSpinToCompleteAsync will handle the rest
            await _gamePage.WaitForTimeoutAsync(500); // was 3000
            TestContext.WriteLine("[IrishWildsPage] PressSpaceForSpinAsync finished.");
        }
    }
}
