using System.Threading.Tasks;
using Casino.Games.Drivers;
using Casino.Games.Pages;
using NUnit.Framework;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using Allure.Net.Commons;

namespace Casino.Games.Tests
{
    [TestFixture]
    [AllureNUnit]
    [AllureEpic("Casino Games")]
    [AllureFeature("Irish Wilds")]
    [AllureSuite("Irish Wilds")]
    [AllureSubSuite("Balance & win/loss validation")]
    public class IrishWildsBalanceTests
    {
        private const int SpinsToPlay = 5;

        [Test]
        [Category("desktop")]
        [AllureSuite("Desktop")]
        [AllureSeverity(SeverityLevel.critical)]
        [AllureStory("Balance changes after multiple spins - Desktop")]
        [TestCase("chromium")]
        [TestCase("firefox")]
        [TestCase("webkit")]
        public async Task IrishWildsBalance_Desktop(string browser)
        {
            await using var driver = await PlaywrightDriver.CreateDesktopAsync(browser, headless: false);
            await RunBalanceScenarioAsync("desktop", browser, driver);
        }

        [Test]
        [Category("mobile")]
        [AllureSuite("Mobile")]
        [AllureSeverity(SeverityLevel.critical)]
        [AllureStory("Balance changes after multiple spins - Mobile")]
        [TestCase("chromium")]
        [TestCase("firefox")]
        [TestCase("webkit")]
        public async Task IrishWildsBalance_Mobile(string browser)
        {
            await using var driver = await PlaywrightDriver.CreateMobileAsync(browser, headless: false);
            await RunBalanceScenarioAsync("mobile", browser, driver);
        }

        [AllureStep("Run Irish Wilds balance scenario on {0} / {1}")]
        private async Task RunBalanceScenarioAsync(string platform, string browser, PlaywrightDriver driver)
        {
            var game = new IrishWildsPage(driver.Page);

            await OpenGameAsync(platform, browser, game);

            var initialBalance = await GetInitialBalanceAsync(platform, browser, game);
            var finalBalance = await SpinAndGetFinalBalanceAsync(platform, browser, SpinsToPlay, game, initialBalance);

            await VerifyBalanceChangedAsync(platform, browser, initialBalance, finalBalance);
        }

        [AllureStep("Open Irish Wilds game ({0} / {1})")]
        private async Task OpenGameAsync(string platform, string browser, IrishWildsPage game)
        {
            await game.NavigateToGameAsync();
        }

        [AllureStep("Read initial balance ({0} / {1})")]
        private async Task<decimal> GetInitialBalanceAsync(string platform, string browser, IrishWildsPage game)
        {
            var initialBalance = await game.GetBalanceAsync();
            var stake = await game.GetStakePerSpinAsync();

            TestContext.WriteLine(
                $"[IrishWildsBalance_{platform}] ({browser}) Initial balance: {initialBalance}, stake: {stake}");

            return initialBalance;
        }

        [AllureStep("Spin {2} times and track balance ({0} / {1})")]
        private async Task<decimal> SpinAndGetFinalBalanceAsync(
            string platform,
            string browser,
            int spinsToPlay,
            IrishWildsPage game,
            decimal initialBalance)
        {
            var lastBalance = initialBalance;

            for (int i = 0; i < spinsToPlay; i++)
            {
                var before = await game.GetBalanceAsync();

                await game.PressSpaceForSpinAsync();
                await game.WaitForSpinToCompleteAsync(before);

                var after = await game.GetBalanceAsync();

                TestContext.WriteLine(
                    $"[IrishWildsBalance_{platform}] ({browser}) Spin {i + 1}: before={before}, after={after}");

                lastBalance = after;
            }

            return lastBalance;
        }

        [AllureStep("Verify balance changed from {2} to {3} ({0} / {1})")]
        private Task VerifyBalanceChangedAsync(
            string platform,
            string browser,
            decimal initialBalance,
            decimal finalBalance)
        {
            Assert.That(finalBalance, Is.Not.EqualTo(initialBalance),
                $"[{platform}] ({browser}) Balance should change after multiple spins.");

            return Task.CompletedTask;
        }
    }
}
