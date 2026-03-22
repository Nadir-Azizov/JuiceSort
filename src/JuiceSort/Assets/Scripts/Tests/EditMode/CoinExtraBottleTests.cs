using NUnit.Framework;
using JuiceSort.Game.Economy;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class CoinExtraBottleTests
    {
        private CoinConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = CoinConfig.Default();
        }

        [Test]
        public void GetExtraBottleCost_FirstUse_Returns500()
        {
            Assert.AreEqual(500, _config.GetExtraBottleCost(0));
        }

        [Test]
        public void GetExtraBottleCost_SecondUse_Returns900()
        {
            Assert.AreEqual(900, _config.GetExtraBottleCost(1));
        }

        [Test]
        public void GetExtraBottleCost_BeyondArray_CapsAtLastValue()
        {
            Assert.AreEqual(900, _config.GetExtraBottleCost(2));
            Assert.AreEqual(900, _config.GetExtraBottleCost(5));
            Assert.AreEqual(900, _config.GetExtraBottleCost(99));
        }

        [Test]
        public void GetExtraBottleCost_UsesConfigValues_NotHardcoded()
        {
            var customConfig = new CoinConfig
            {
                ExtraBottleCosts = new[] { 300, 600, 1000 }
            };

            Assert.AreEqual(300, customConfig.GetExtraBottleCost(0));
            Assert.AreEqual(600, customConfig.GetExtraBottleCost(1));
            Assert.AreEqual(1000, customConfig.GetExtraBottleCost(2));
            Assert.AreEqual(1000, customConfig.GetExtraBottleCost(5));
        }

        [Test]
        public void GetExtraBottleCost_FullSequence_TotalIs1400()
        {
            int totalCost = 0;
            for (int i = 0; i < 2; i++)
                totalCost += _config.GetExtraBottleCost(i);

            // 500 + 900 = 1400
            Assert.AreEqual(1400, totalCost);
        }

        [Test]
        public void SpendCoins_InsufficientBalance_BlocksExtraBottle()
        {
            int balance = 400;
            int firstCost = _config.GetExtraBottleCost(0);

            Assert.IsFalse(balance >= firstCost, "400 coins should NOT afford 500-coin extra bottle");
        }

        [Test]
        public void SpendCoins_ExactBalance_AffordsExtraBottle()
        {
            int balance = 500;
            int firstCost = _config.GetExtraBottleCost(0);

            Assert.IsTrue(balance >= firstCost, "500 coins should afford 500-coin extra bottle");
        }
    }
}
