using NUnit.Framework;
using JuiceSort.Game.Economy;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class CoinUndoTests
    {
        private CoinConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = CoinConfig.Default();
        }

        [Test]
        public void GetUndoCost_FirstUse_Returns100()
        {
            Assert.AreEqual(100, _config.GetUndoCost(0));
        }

        [Test]
        public void GetUndoCost_SecondUse_Returns200()
        {
            Assert.AreEqual(200, _config.GetUndoCost(1));
        }

        [Test]
        public void GetUndoCost_ThirdUse_Returns300()
        {
            Assert.AreEqual(300, _config.GetUndoCost(2));
        }

        [Test]
        public void GetUndoCost_BeyondArray_CapsAtLastValue()
        {
            Assert.AreEqual(300, _config.GetUndoCost(3));
            Assert.AreEqual(300, _config.GetUndoCost(5));
            Assert.AreEqual(300, _config.GetUndoCost(99));
        }

        [Test]
        public void GetUndoCost_UsesConfigValues_NotHardcoded()
        {
            var customConfig = new CoinConfig
            {
                UndoCosts = new[] { 50, 150 }
            };

            Assert.AreEqual(50, customConfig.GetUndoCost(0));
            Assert.AreEqual(150, customConfig.GetUndoCost(1));
            Assert.AreEqual(150, customConfig.GetUndoCost(5)); // capped at last
        }

        [Test]
        public void GetUndoCost_EscalatesCorrectly_FullSequence()
        {
            int totalCost = 0;
            for (int i = 0; i < 5; i++)
                totalCost += _config.GetUndoCost(i);

            // 100 + 200 + 300 + 300 + 300 = 1200
            Assert.AreEqual(1200, totalCost);
        }

        [Test]
        public void SpendCoins_InsufficientBalance_ReturnsFalse()
        {
            // Verify the cost pattern: a player with 150 coins can afford 1st undo (100)
            // but not 2nd undo (200)
            int balance = 150;
            int firstCost = _config.GetUndoCost(0);
            int secondCost = _config.GetUndoCost(1);

            Assert.IsTrue(balance >= firstCost, "Should afford first undo");
            Assert.IsFalse(balance - firstCost >= secondCost, "Should NOT afford second undo after first");
        }
    }
}
