using System;
using System.Threading.Tasks;
using Casino.Games.Drivers;
using Casino.Games.Pages;
using Casino.Games.Utils;
using NUnit.Framework;

namespace Casino.Games.Tests
{
    [TestFixture]
    public class IrishWildsBalanceTests
    {
        private async Task RunBalanceTestAsync(ExecutionProfile profile, int spinsToPlay)
        {
            var (playwright, browser, context, page) = await PlaywrightDriver.CreateAsync(profile);

            try
            {
                var game = new IrishWildsPage(page);

                // 1) Навигация към играта
                await game.NavigateToGameAsync();

                // 2) Начален баланс и ставка
                var initialBalance = await game.GetBalanceAsync();
                var stake = await game.GetStakePerSpinAsync();

                TestContext.WriteLine($"[Test] Initial balance: {initialBalance}");
                TestContext.WriteLine($"[Test] Stake per spin: {stake}");

                Assert.That(initialBalance, Is.GreaterThan(0m), "Initial balance should be positive.");
                Assert.That(stake, Is.GreaterThan(0m), "Stake per spin should be positive.");

                var currentBalance = initialBalance;

                // 3) Въртим няколко спина и валидираме формулата
                for (int i = 1; i <= spinsToPlay; i++)
                {
                    TestContext.WriteLine($"[Test] --- Spin {i} ---");

                    var beforeSpin = await game.GetBalanceAsync();
                    TestContext.WriteLine($"[Test] Balance before spin {i}: {beforeSpin}");

                    await game.ClickSpinAsync();
                    await game.WaitForSpinToCompleteAsync(beforeSpin);

                    var afterSpin = await game.GetBalanceAsync();
                    var lastWin = await game.GetLastWinAsync();

                    TestContext.WriteLine($"[Test] Last win after spin {i}: {lastWin}");
                    TestContext.WriteLine($"[Test] Balance after spin {i}: {afterSpin}");

                    var expected = beforeSpin - stake + lastWin;

                    BalanceHelper.AssertBalance(
                        expected,
                        afterSpin,
                        $"Spin {i}: Balance formula mismatch (before={beforeSpin}, stake={stake}, win={lastWin})",
                        tolerance: 1.00m
                    );

                    currentBalance = afterSpin;
                }

                // 4) Финален sanity check – балансът е >= 0
                Assert.That(currentBalance, Is.GreaterThanOrEqualTo(0m), "Final balance should not be negative.");
            }
            finally
            {
                try { await context.CloseAsync(); } catch { }
                try { await browser.CloseAsync(); } catch { }
                playwright.Dispose();
            }
        }

        [Test]
        public async Task IrishWildsBalance_Desktop()
        {
            await RunBalanceTestAsync(ExecutionProfile.Desktop, spinsToPlay: 5);
        }

        [Test]
        public async Task IrishWildsBalance_Mobile()
        {
            await RunBalanceTestAsync(ExecutionProfile.Mobile, spinsToPlay: 3);
        }
    }
}
