using System.Threading.Tasks;
using Casino.Games.Drivers;
using Casino.Games.Pages;
using NUnit.Framework;

namespace Casino.Games.Tests
{
    [TestFixture]
    public class IrishWildsBalanceTests
    {
        private const int SpinsToPlay = 5;

        [Test]
        public async Task IrishWildsBalance_Desktop()
        {
            await using var driver = await PlaywrightDriver.CreateDesktopAsync(headless: false);
            var game = new IrishWildsPage(driver.Page);

            await game.NavigateToGameAsync();

            var initialBalance = await game.GetBalanceAsync();
            var stake = await game.GetStakePerSpinAsync();

            TestContext.WriteLine(
                $"[IrishWildsBalance_Desktop] Initial balance: {initialBalance}, stake: {stake}");

            var lastBalance = initialBalance;

            for (int i = 0; i < SpinsToPlay; i++)
            {
                var before = await game.GetBalanceAsync();

                await game.PressSpaceForSpinAsync();
                await game.WaitForSpinToCompleteAsync(before);

                var after = await game.GetBalanceAsync();
                TestContext.WriteLine(
                    $"[IrishWildsBalance_Desktop] Spin {i + 1}: before={before}, after={after}");

                lastBalance = after;
            }

            var finalBalance = lastBalance;
            Assert.That(finalBalance, Is.Not.EqualTo(initialBalance),
                "Balance should change after multiple spins.");
        }

        [Test]
        public async Task IrishWildsBalance_Mobile()
        {
            await using var driver = await PlaywrightDriver.CreateMobileAsync(headless: false);
            var game = new IrishWildsPage(driver.Page);

            await game.NavigateToGameAsync();

            var initialBalance = await game.GetBalanceAsync();
            var stake = await game.GetStakePerSpinAsync();

            TestContext.WriteLine(
                $"[IrishWildsBalance_Mobile] Initial balance: {initialBalance}, stake: {stake}");

            var lastBalance = initialBalance;

            for (int i = 0; i < SpinsToPlay; i++)
            {
                var before = await game.GetBalanceAsync();

                await game.PressSpaceForSpinAsync();
                await game.WaitForSpinToCompleteAsync(before);

                var after = await game.GetBalanceAsync();
                TestContext.WriteLine(
                    $"[IrishWildsBalance_Mobile] Spin {i + 1}: before={before}, after={after}");

                lastBalance = after;
            }

            var finalBalance = lastBalance;
            Assert.That(finalBalance, Is.Not.EqualTo(initialBalance),
                "Balance should change after multiple spins.");
        }
    }
}
