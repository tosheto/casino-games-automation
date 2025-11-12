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
            await using var driver = await PlaywrightDriver.CreateDesktopAsync(headless: true);
            var game = new IrishWildsPage(driver.Page);

            await game.NavigateToGameAsync();

            var initialBalance = await game.GetBalanceAsync();
            var stake = await game.GetStakePerSpinAsync();

            for (int i = 0; i < SpinsToPlay; i++)
            {
                var before = await game.GetBalanceAsync();

                await game.ClickSpinAsync();
                await game.WaitForSpinToCompleteAsync(before);

                var after = await game.GetBalanceAsync();
                Assert.That(after, Is.Not.EqualTo(before), "Balance should change after spin.");
            }

            var finalBalance = await game.GetBalanceAsync();
            Assert.That(finalBalance, Is.Not.EqualTo(initialBalance),
                "Balance should change after multiple spins.");
            Assert.That(stake, Is.GreaterThan(0m), "Stake per spin should be > 0.");
        }

        [Test]
        public async Task IrishWildsBalance_Mobile()
        {
            await using var driver = await PlaywrightDriver.CreateMobileAsync(headless: true);
            var game = new IrishWildsPage(driver.Page);

            await game.NavigateToGameAsync();

            var initialBalance = await game.GetBalanceAsync();
            var stake = await game.GetStakePerSpinAsync();

            for (int i = 0; i < SpinsToPlay; i++)
            {
                var before = await game.GetBalanceAsync();

                await game.ClickSpinAsync();
                await game.WaitForSpinToCompleteAsync(before);

                var after = await game.GetBalanceAsync();
                Assert.That(after, Is.Not.EqualTo(before), "Balance should change after spin.");
            }

            var finalBalance = await game.GetBalanceAsync();
            Assert.That(finalBalance, Is.Not.EqualTo(initialBalance),
                "Balance should change after multiple spins.");
            Assert.That(stake, Is.GreaterThan(0m), "Stake per spin should be > 0.");
        }
    }
}
