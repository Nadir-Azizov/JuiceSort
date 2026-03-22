using NUnit.Framework;
using JuiceSort.Game.Economy;
using JuiceSort.Game.LevelGen;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class CoinRewardCalculatorTests
    {
        private CoinConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = CoinConfig.Default();
        }

        private LevelDefinition MakeDefinition(int colorCount)
        {
            return new LevelDefinition(
                levelNumber: 1,
                containerCount: colorCount + 2,
                colorCount: colorCount,
                slotCount: 4,
                emptyContainerCount: 2
            );
        }

        [Test]
        public void CalculateReward_1Star_3Colors_ReturnsBaseReward()
        {
            var def = MakeDefinition(3);
            int reward = CoinRewardCalculator.CalculateReward(1, def, _config);

            // Base 75 * 1.0 difficulty = 75, no efficiency bonus for 1 star
            Assert.AreEqual(75, reward);
        }

        [Test]
        public void CalculateReward_3Stars_3Colors_AddsFullEfficiencyBonus()
        {
            var def = MakeDefinition(3);
            int reward = CoinRewardCalculator.CalculateReward(3, def, _config);

            // Base 75 * 1.0 = 75, bonus = 75 * 0.25 = 18.75 → total 93
            Assert.AreEqual(93, reward);
        }

        [Test]
        public void CalculateReward_2Stars_3Colors_AddsHalfEfficiencyBonus()
        {
            var def = MakeDefinition(3);
            int reward = CoinRewardCalculator.CalculateReward(2, def, _config);

            // Base 75 * 1.0 = 75, bonus = 75 * 0.25 * 0.5 = 9.375 → total 84
            Assert.AreEqual(84, reward);
        }

        [Test]
        public void CalculateReward_5Colors_HigherThan3Colors()
        {
            var def3 = MakeDefinition(3);
            var def5 = MakeDefinition(5);

            int reward3 = CoinRewardCalculator.CalculateReward(1, def3, _config);
            int reward5 = CoinRewardCalculator.CalculateReward(1, def5, _config);

            // 3 colors: 75 * 1.0 = 75
            // 5 colors: 75 * 1.3 = 97
            Assert.Greater(reward5, reward3);
            Assert.AreEqual(97, reward5);
        }

        [Test]
        public void CalculateReward_5Colors_3Stars_FullBonus()
        {
            var def = MakeDefinition(5);
            int reward = CoinRewardCalculator.CalculateReward(3, def, _config);

            // Base 75 * 1.3 = 97.5, bonus = 97.5 * 0.25 = 24.375 → total 121
            Assert.AreEqual(121, reward);
        }

        [Test]
        public void CalculateReward_UsesConfigValues_NotHardcoded()
        {
            var customConfig = new CoinConfig
            {
                BaseLevelReward = 100,
                MoveEfficiencyBonusPercent = 0.5f
            };

            var def = MakeDefinition(3);
            int reward = CoinRewardCalculator.CalculateReward(3, def, customConfig);

            // Base 100 * 1.0 = 100, bonus = 100 * 0.5 = 50 → total 150
            Assert.AreEqual(150, reward);
        }

        [Test]
        public void CalculateReward_NullConfig_UsesDefaults()
        {
            var def = MakeDefinition(3);
            int reward = CoinRewardCalculator.CalculateReward(1, def, null);

            Assert.AreEqual(75, reward);
        }
    }
}
