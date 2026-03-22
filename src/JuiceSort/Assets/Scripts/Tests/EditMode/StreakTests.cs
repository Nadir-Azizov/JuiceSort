using NUnit.Framework;
using JuiceSort.Game.Economy;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class StreakTests
    {
        private CoinConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = CoinConfig.Default();
        }

        [Test]
        public void StreakBonus3_DefaultConfig_Returns200()
        {
            Assert.AreEqual(200, _config.StreakBonus3);
        }

        [Test]
        public void StreakBonus5_DefaultConfig_Returns500()
        {
            Assert.AreEqual(500, _config.StreakBonus5);
        }

        [Test]
        public void GetStreakBonus_AtStreak3_ReturnsStreakBonus3()
        {
            int bonus = StreakBonusCalculator.GetStreakBonus(3, _config);
            Assert.AreEqual(200, bonus);
        }

        [Test]
        public void GetStreakBonus_AtStreak5_ReturnsStreakBonus5()
        {
            int bonus = StreakBonusCalculator.GetStreakBonus(5, _config);
            Assert.AreEqual(500, bonus);
        }

        [Test]
        public void GetStreakBonus_AtStreak0_ReturnsZero()
        {
            int bonus = StreakBonusCalculator.GetStreakBonus(0, _config);
            Assert.AreEqual(0, bonus);
        }

        [Test]
        public void GetStreakBonus_AtStreak1_ReturnsZero()
        {
            int bonus = StreakBonusCalculator.GetStreakBonus(1, _config);
            Assert.AreEqual(0, bonus);
        }

        [Test]
        public void GetStreakBonus_AtStreak2_ReturnsZero()
        {
            int bonus = StreakBonusCalculator.GetStreakBonus(2, _config);
            Assert.AreEqual(0, bonus);
        }

        [Test]
        public void GetStreakBonus_AtStreak4_ReturnsZero()
        {
            int bonus = StreakBonusCalculator.GetStreakBonus(4, _config);
            Assert.AreEqual(0, bonus);
        }

        [Test]
        public void GetStreakBonus_AtStreak6_ReturnsZero()
        {
            int bonus = StreakBonusCalculator.GetStreakBonus(6, _config);
            Assert.AreEqual(0, bonus);
        }

        [Test]
        public void GetStreakBonus_AtStreak10_ReturnsZero()
        {
            int bonus = StreakBonusCalculator.GetStreakBonus(10, _config);
            Assert.AreEqual(0, bonus);
        }

        [Test]
        public void GetStreakBonus_UsesConfigValues_NotHardcoded()
        {
            var customConfig = new CoinConfig
            {
                StreakBonus3 = 300,
                StreakBonus5 = 800
            };

            Assert.AreEqual(300, StreakBonusCalculator.GetStreakBonus(3, customConfig));
            Assert.AreEqual(800, StreakBonusCalculator.GetStreakBonus(5, customConfig));
        }

        // --- Streak counting flow tests (simulate GameplayManager pattern) ---

        [Test]
        public void StreakFlow_StartsAtZero_NoBonusAwarded()
        {
            int streak = 0;
            int bonus = StreakBonusCalculator.GetStreakBonus(streak, _config);
            Assert.AreEqual(0, streak);
            Assert.AreEqual(0, bonus);
        }

        [Test]
        public void StreakFlow_IncrementToThree_AwardsBonus()
        {
            int streak = 0;
            for (int i = 0; i < 3; i++)
                streak++;

            Assert.AreEqual(3, streak);
            Assert.AreEqual(200, StreakBonusCalculator.GetStreakBonus(streak, _config));
        }

        [Test]
        public void StreakFlow_IncrementToFive_AwardsBonus()
        {
            int streak = 0;
            for (int i = 0; i < 5; i++)
                streak++;

            Assert.AreEqual(5, streak);
            Assert.AreEqual(500, StreakBonusCalculator.GetStreakBonus(streak, _config));
        }

        [Test]
        public void StreakFlow_ResetAfterThree_StartsOver()
        {
            int streak = 3;
            Assert.AreEqual(200, StreakBonusCalculator.GetStreakBonus(streak, _config));

            streak = 0; // ResetStreak
            Assert.AreEqual(0, streak);
            Assert.AreEqual(0, StreakBonusCalculator.GetStreakBonus(streak, _config));

            // Rebuild streak
            streak++;
            Assert.AreEqual(0, StreakBonusCalculator.GetStreakBonus(streak, _config));
        }

        [Test]
        public void StreakFlow_FullSequence_BonusOnlyAtThreeAndFive()
        {
            int streak = 0;
            int totalBonus = 0;

            for (int i = 0; i < 7; i++)
            {
                streak++;
                totalBonus += StreakBonusCalculator.GetStreakBonus(streak, _config);
            }

            // Only streak 3 (200) and streak 5 (500) award bonuses
            Assert.AreEqual(700, totalBonus);
            Assert.AreEqual(7, streak);
        }
    }
}
