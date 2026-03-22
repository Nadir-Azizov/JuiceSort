using NUnit.Framework;
using JuiceSort.Game.Economy;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class CoinConfigTests
    {
        private CoinConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = CoinConfig.Default();
        }

        [Test]
        public void Default_InitialBalance_IsZero()
        {
            Assert.AreEqual(0, _config.InitialBalance);
        }

        [Test]
        public void Default_BaseLevelReward_Is75()
        {
            Assert.AreEqual(75, _config.BaseLevelReward);
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
            Assert.AreEqual(300, _config.GetUndoCost(5));
            Assert.AreEqual(300, _config.GetUndoCost(99));
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
            Assert.AreEqual(900, _config.GetExtraBottleCost(5));
        }

        [Test]
        public void MoveEfficiencyBonusPercent_Default_Is025()
        {
            Assert.AreEqual(0.25f, _config.MoveEfficiencyBonusPercent, 0.001f);
        }

        [Test]
        public void StreakBonuses_DefaultValues()
        {
            Assert.AreEqual(200, _config.StreakBonus3);
            Assert.AreEqual(500, _config.StreakBonus5);
        }

        [Test]
        public void AdRewardAmount_Default_Is250()
        {
            Assert.AreEqual(250, _config.AdRewardAmount);
        }
    }
}
