using System.Threading.Tasks;
using Casino.Games.Drivers;
using Casino.Games.Pages;
using NUnit.Framework;
using Allure.NUnit;
using Allure.NUnit.Attributes;

namespace Casino.Games.Tests
{
    [TestFixture]
    [AllureNUnit]
    [AllureEpic("Casino Games")]
    [AllureFeature("Irish Wilds")]
    [AllureSuite("Balance & win/loss validation")]
    public class IrishWildsBalanceTests
    {
        private const int SpinsToPlay = 5;

        private static readonly string[] Browsers = new[]
        {
            "chromium",
            "firefox",
            "webkit"
        };

        // =======================
        // DESKTOP
        // =======================
        [Test]
        [TestCaseSource(nameof(Browsers))]
        [AllureSubSuite("Desktop")]
        [AllureSeverity(SeverityLevel.critical)]
        [AllureStory("Balance changes after multiple spins (Desktop)")]
        public async Task IrishWildsBalance_Desktop(string browser)
        {
            await using var driver = await PlaywrightDriver.CreateDesktopAsync(browser, headless: false);
            var game = new IrishWildsPage(driver.Page);

            await Step_OpenGame(game, browser, "desktop");

            var initialBalance = await Step_GetBalance(game, "initial");
            var stake = await Step_GetStake(game);

            var lastBalance = initialBalance;

            for (int i = 0; i < SpinsToPlay; i++)
                await Step_Spin(game, i + 1, ref lastBalance);

            await Step_AssertBalanceChanged(initialBalance, lastBalance);
        }

        // =======================
        // MOBILE
        // =======================
        [Test]
        [TestCaseSource(nameof(Browsers))]
        [AllureSubSuite("Mobile")]
        [AllureSeverity(SeverityLevel.critical)]
        [AllureStory("Balance changes after multiple spins (Mobile)")]
        public async Task IrishWildsBalance_Mobile(string browser)
        {
            await using var driver = await PlaywrightDriver.CreateMobileAsync(browser, headless: false);
            var game = new IrishWildsPage(driver.Page);

            await Step_OpenGame(game, browser, "mobile");

            var initialBalance = await Step_GetBalance(game, "initial");
            var stake = await Step_GetStake(game);

            var lastBalance = initialBalance;

            for (int i = 0; i < SpinsToPlay; i++)
                await Step_Spin(game, i + 1, ref lastBalance);

            await Step_AssertBalanceChanged(initialBalance, lastBalance);
        }

        // =======================
        // ALLURE STEPS
        // =======================

        [AllureStep("Open Irish Wilds in {mode} mode with {browser}")]
        private async Task Step_OpenGame(IrishWildsPage game, string browser, string mode)
        {
            await game.NavigateToGameAsync();
        }

        [AllureStep("Get {description} balance")]
        private async Task<decimal> Step_GetBalance(IrishWildsPage game, string description)
        {
            return await game.GetBalanceAsync();
        }

        [AllureStep("Get stake per spin")]
        private async Task<decimal> Step_GetStake(IrishWildsPage game)
        {
            return await game.GetStakePerSpinAsync();
        }

        [AllureStep("Spin #{spinNumber}")]
        private async Task Step_Spin(IrishWildsPage game, int spinNumber, ref decimal lastBalance)
        {
            var before = await game.GetBalanceAsync();
            await game.PressSpaceForSpinAsync();
            await game.WaitForSpinToCompleteAsync(before);
            lastBalance = await game.GetBalanceAsync();
        }

        [AllureStep("Validate balance changed")]
        private Task Step_AssertBalanceChanged(decimal initial, decimal final)
        {
            Assert.That(final, Is.Not.EqualTo(initial), "Balance should change after spins.");
            return Task.CompletedTask;
        }
    }
}
