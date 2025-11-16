using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Casino.Games.Configuration;
using Casino.Games.Utils;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Casino.Games.Pages
{
    public partial class IrishWildsPage
    {
        private readonly IPage _hostPage;
        private IPage _gamePage;
        private IFrame _gameFrame;

        public IrishWildsPage(IPage page)
        {
            _hostPage = page;
            _gamePage = page;
            _gameFrame = page.MainFrame;
        }

        public async Task NavigateToGameAsync()
        {
            try
            {
                // 1) Navigate to Spinberry products page
                await _hostPage.GotoAsync(TestSettings.BaseUrl, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle
                });
                TestContext.WriteLine($"[IrishWildsPage] Navigated to products: {TestSettings.BaseUrl}");

                // 2) Accept cookies if present
                try
                {
                    var agree = _hostPage.Locator("text=I agree");
                    if (await agree.CountAsync() > 0)
                    {
                        await agree.First.ClickAsync(new LocatorClickOptions { Timeout = 5000 });
                        TestContext.WriteLine("[IrishWildsPage] Accepted cookies.");
                    }
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"[IrishWildsPage] Cookie banner error: {ex.Message}");
                }

                // 3) Scroll towards the Irish Wilds section
                await _hostPage.EvaluateAsync("window.scrollBy(0, document.body.scrollHeight / 2);");
                await _hostPage.WaitForTimeoutAsync(1500);

                // 4) 'Play game' link for Irish Wilds
                var playLink = _hostPage.Locator(
                    "xpath=//h4[normalize-space()='Irish Wilds']/following::a[contains(@class,'showreel') and contains(@class,'play')][1]"
                );

                if (await playLink.CountAsync() == 0)
                {
                    await ReportingHelper.CaptureScreenshotAsync(_hostPage, "NoPlayLink_IrishWilds");
                    await ReportingHelper.DumpHtmlAsync(_hostPage, "NoPlayLink_IrishWilds");
                    throw new Exception("Could not find 'Play game' link for Irish Wilds.");
                }

                TestContext.WriteLine("[IrishWildsPage] Found 'Play game' link. Clicking...");

                IPage newPage = null;

                try
                {
                    // Try to capture popup (target=_blank), if any
                    newPage = await _hostPage.Context.RunAndWaitForPageAsync(async () =>
                    {
                        await playLink.First.ClickAsync(new LocatorClickOptions
                        {
                            Timeout = 8000,
                            Force = true
                        });
                    }, new()
                    {
                        Timeout = 10000
                    });

                    TestContext.WriteLine($"[IrishWildsPage] Popup detected after Play game. Url: {newPage.Url}");
                }
                catch (PlaywrightException ex)
                {
                    TestContext.WriteLine($"[IrishWildsPage] No popup after Play game (or timeout): {ex.Message}");
                }
                catch (TimeoutException)
                {
                    TestContext.WriteLine("[IrishWildsPage] No popup after Play game (timeout).");
                }

                if (newPage != null)
                {
                    _gamePage = newPage;
                }
                else
                {
                    // Fallback to the direct demo GameUrl
                    TestContext.WriteLine("[IrishWildsPage] Navigating to fallback GameUrl.");
                    await _hostPage.GotoAsync(TestSettings.GameUrl, new PageGotoOptions
                    {
                        WaitUntil = WaitUntilState.NetworkIdle
                    });
                    _gamePage = _hostPage;
                }

                await _gamePage.WaitForLoadStateAsync(LoadState.NetworkIdle);
                TestContext.WriteLine($"[IrishWildsPage] On game host page: {_gamePage.Url}");

                // 5) Detect game frame (if any)
                _gameFrame = await DetectGameFrameAsync();
                TestContext.WriteLine($"[IrishWildsPage] Initial game frame: {_gameFrame.Url}");

                // 6) Intro Play and wait for actual game UI (balance + spin)
                await WaitForGameLoadedAsync();
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[IrishWildsPage] NavigateToGameAsync FAILED: {ex}");
                try
                {
                    await ReportingHelper.CaptureScreenshotAsync(_hostPage, "NavigateToGame_Failed");
                    await ReportingHelper.DumpHtmlAsync(_hostPage, "NavigateToGame_Failed");
                }
                catch { }
                throw;
            }
        }

        private async Task<IFrame> DetectGameFrameAsync()
        {
            var frames = _gamePage.Frames.ToList();
            var main = _gamePage.MainFrame;

            TestContext.WriteLine($"[IrishWildsPage] Frames on current page: {frames.Count}");

            var byUrl = frames.FirstOrDefault(f =>
                f != main &&
                (f.Url.Contains("spinberry", StringComparison.OrdinalIgnoreCase) ||
                 f.Url.Contains("irishwilds", StringComparison.OrdinalIgnoreCase) ||
                 f.Url.Contains("SB_HTML5_IrishWilds", StringComparison.OrdinalIgnoreCase))
            );

            if (byUrl != null)
            {
                TestContext.WriteLine($"[IrishWildsPage] Selected frame by URL: {byUrl.Url}");
                return byUrl;
            }

            foreach (var frame in frames.Where(f => f != main))
            {
                try
                {
                    var spin = await frame.Locator("[class*=\"spin\" i]").CountAsync();
                    var bal = await frame.Locator("[class*=\"bal\" i], [class*=\"credit\" i]").CountAsync();
                    var bet = await frame.Locator("[class*=\"bet\" i], [class*=\"stake\" i]").CountAsync();

                    if (spin > 0 || bal > 0 || bet > 0)
                    {
                        TestContext.WriteLine($"[IrishWildsPage] Selected frame by content: {frame.Url}");
                        return frame;
                    }
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"[IrishWildsPage] Error probing frame {frame.Url}: {ex.Message}");
                }
            }

            TestContext.WriteLine("[IrishWildsPage] No specific iframe; using main frame.");
            return main;
        }

        private async Task WaitForGameLoadedAsync()
        {
            var ctx = _gamePage.Context;
            var deadline = DateTime.UtcNow.AddSeconds(20);
            var introTried = false;

            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    // 1) If we are on the intro (green) screen – click the yellow Play button once
                    if (!introTried)
                    {
                        // Exact intro button:
                        // <button type="button" class="button button__slider-play button__rounded-xl _visible">
                        var introBtnPage = _gamePage.Locator("button.button__slider-play.button__rounded-xl._visible");
                        var introBtnFrame = _gameFrame.Locator("button.button__slider-play.button__rounded-xl._visible");

                        ILocator introBtn = null;

                        if (await introBtnFrame.CountAsync() > 0)
                            introBtn = introBtnFrame.First;
                        else if (await introBtnPage.CountAsync() > 0)
                            introBtn = introBtnPage.First;

                        if (introBtn != null)
                        {
                            TestContext.WriteLine("[IrishWildsPage] Intro Play button found. Clicking...");

                            try
                            {
                                await introBtn.ClickAsync(new LocatorClickOptions
                                {
                                    Force = true,
                                    Timeout = 5000
                                });
                            }
                            catch (Exception ex)
                            {
                                TestContext.WriteLine($"[IrishWildsPage] Intro Play click failed (continuing anyway): {ex.Message}");
                            }

                            introTried = true;

                            // Give the game a short moment to transition, then re-detect frame
                            try
                            {
                                await _gamePage.WaitForTimeoutAsync(1000);
                            }
                            catch { }

                            _gameFrame = await DetectGameFrameAsync();
                            await _gamePage.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                            continue;
                        }
                    }

                    // 2) Check for real game UI (balance + spin) in the current frame
                    var balCount = await _gameFrame.Locator(TestSettings.Selectors.BalanceLabelCss).CountAsync();
                    var spinCount = await _gameFrame.Locator(TestSettings.Selectors.SpinButtonCss).CountAsync();

                    if (balCount > 0 && spinCount > 0)
                    {
                        TestContext.WriteLine("[IrishWildsPage] Game UI detected (balance & spin present).");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"[IrishWildsPage] WaitForGameLoadedAsync iteration error: {ex.Message}");
                }

                await _gamePage.WaitForTimeoutAsync(300);
            }

            await DumpGameFrameHtmlAsync("GameNotLoaded");
            throw new Exception("Game UI did not load in time (no balance/spin after intro).");
        }

        private async Task<ILocator> FindFirstExistingLocator(string purpose, params string[] selectors)
        {
            foreach (var selector in selectors)
            {
                try
                {
                    var loc = _gameFrame.Locator(selector);
                    var count = await loc.CountAsync();
                    if (count > 0)
                    {
                        TestContext.WriteLine($"[IrishWildsPage] ({purpose}) Using selector '{selector}' (found {count}).");
                        return loc.First;
                    }
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"[IrishWildsPage] ({purpose}) Selector '{selector}' error: {ex.Message}");
                }
            }

            await DumpGameFrameHtmlAsync($"NoSelector_{purpose}");
            throw new Exception($"No matching element found for {purpose} in game frame.");
        }

        private async Task DumpGameFrameHtmlAsync(string purpose)
        {
            try
            {
                var html = await _gameFrame.ContentAsync();
                var fileName = $"IrishWilds_GameFrame_{purpose}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.html";
                var fullPath = Path.Combine(AppContext.BaseDirectory, fileName);
                File.WriteAllText(fullPath, html);
                TestContext.AddTestAttachment(fullPath);
                TestContext.WriteLine($"[IrishWildsPage] Dumped game frame HTML to: {fullPath}");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[IrishWildsPage] Failed to dump game frame HTML: {ex.Message}");
            }
        }

        public async Task<decimal> GetBalanceAsync()
        {
            var locator = await FindFirstExistingLocator(
                "Balance",
                TestSettings.Selectors.BalanceLabelCss,
                "[class*=\"balance\" i]",
                "[id*=\"balance\" i]",
                "[class*=\"credit\" i]"
            );

            var text = await locator.InnerTextAsync(new LocatorInnerTextOptions { Timeout = 8000 });
            text = text?.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                TestContext.WriteLine("[IrishWildsPage] Balance text is empty – returning 0 as fallback.");
                return 0m;
            }
            var balance = BalanceHelper.ParseAmount(text);
            TestContext.WriteLine($"[IrishWildsPage] Balance: {balance} ({text})");
            return balance;
        }

        public async Task<decimal> GetStakePerSpinAsync()
        {
            var locator = await FindFirstExistingLocator(
                "Stake/Bet",
                TestSettings.Selectors.StakeLabelCss,
                "[class*=\"bet\" i]",
                "[class*=\"stake\" i]"
            );

            var text = await locator.InnerTextAsync(new LocatorInnerTextOptions { Timeout = 8000 });
            text = text?.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                TestContext.WriteLine("[IrishWildsPage] Stake text is empty – returning 0 as fallback.");
                return 0m;
            }
            var stake = BalanceHelper.ParseAmount(text);
            TestContext.WriteLine($"[IrishWildsPage] Stake per spin: {stake} ({text})");
            return stake;
        }

        public async Task<decimal> GetLastWinAsync()
        {
            try
            {
                var locator = await FindFirstExistingLocator(
                    "LastWin",
                    TestSettings.Selectors.LastWinLabelCss,
                    "[class*=\"win\" i]"
                );

                var text = await locator.InnerTextAsync(new LocatorInnerTextOptions { Timeout = 5000 });
                var win = BalanceHelper.ParseAmount(text);
                TestContext.WriteLine($"[IrishWildsPage] Last win: {win} ({text})");
                return win;
            }
            catch
            {
                TestContext.WriteLine("[IrishWildsPage] Last win not visible. Assuming 0.");
                return 0m;
            }
        }

        public async Task ClickSpinAsync()
        {
            var locator = await FindFirstExistingLocator(
                "SpinButton",
                TestSettings.Selectors.SpinButtonCss,
                "[class*=\"spin\" i]"
            );

            await locator.ClickAsync(new LocatorClickOptions { Timeout = 8000 });
            TestContext.WriteLine("[IrishWildsPage] Clicked SPIN.");
        }

        public async Task WaitForSpinToCompleteAsync(decimal previousBalance)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(TestSettings.SpinWaitTimeoutMs);

            await _gamePage.WaitForTimeoutAsync(1500);

            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    var currentBalance = await GetBalanceAsync();
                    if (currentBalance != previousBalance)
                    {
                        TestContext.WriteLine("[IrishWildsPage] Balance changed. Spin complete.");
                        return;
                    }
                }
                catch
                {
                    // ignore
                }

                await _gamePage.WaitForTimeoutAsync(500);
            }

            TestContext.WriteLine("[IrishWildsPage] Spin wait timeout reached.");
        }
    }
}
