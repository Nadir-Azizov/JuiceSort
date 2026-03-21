using NUnit.Framework;
using JuiceSort.Game.Progression;
using JuiceSort.Game.LevelGen;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class StarCalculatorTests
    {
        [Test]
        public void CalculateStars_ExactOptimal_ThreeStars()
        {
            Assert.AreEqual(3, StarCalculator.CalculateStars(10, 10));
        }

        [Test]
        public void CalculateStars_AtThreeStarBoundary_ThreeStars()
        {
            // 1.2x optimal = 12, should still be 3 stars
            Assert.AreEqual(3, StarCalculator.CalculateStars(12, 10));
        }

        [Test]
        public void CalculateStars_JustOverThreeStarBoundary_TwoStars()
        {
            Assert.AreEqual(2, StarCalculator.CalculateStars(13, 10));
        }

        [Test]
        public void CalculateStars_AtTwoStarBoundary_TwoStars()
        {
            // 1.5x optimal = 15, should be 2 stars
            Assert.AreEqual(2, StarCalculator.CalculateStars(15, 10));
        }

        [Test]
        public void CalculateStars_JustOverTwoStarBoundary_OneStar()
        {
            Assert.AreEqual(1, StarCalculator.CalculateStars(16, 10));
        }

        [Test]
        public void CalculateStars_DoubleOptimal_OneStar()
        {
            Assert.AreEqual(1, StarCalculator.CalculateStars(20, 10));
        }

        [Test]
        public void CalculateStars_AlwaysAtLeastOne()
        {
            Assert.AreEqual(1, StarCalculator.CalculateStars(1000, 10));
        }

        [Test]
        public void CalculateStars_NeverMoreThanThree()
        {
            Assert.AreEqual(3, StarCalculator.CalculateStars(1, 100));
        }

        [Test]
        public void CalculateStars_ZeroOptimal_ReturnsOne()
        {
            Assert.AreEqual(1, StarCalculator.CalculateStars(5, 0));
        }

        [Test]
        public void GetStarText_ThreeStars()
        {
            Assert.AreEqual("\u2605\u2605\u2605", StarCalculator.GetStarText(3));
        }

        [Test]
        public void GetStarText_TwoStars()
        {
            Assert.AreEqual("\u2605\u2605\u2606", StarCalculator.GetStarText(2));
        }

        [Test]
        public void GetStarText_OneStar()
        {
            Assert.AreEqual("\u2605\u2606\u2606", StarCalculator.GetStarText(1));
        }

        [Test]
        public void GetStarText_ZeroStars()
        {
            Assert.AreEqual("\u2606\u2606\u2606", StarCalculator.GetStarText(0));
        }

        [Test]
        public void OptimalMoveEstimator_Level1_ReturnsPositive()
        {
            OptimalMoveEstimator.ClearCache();
            var def = DifficultyScaler.GetLevelDefinition(1);
            int estimate = OptimalMoveEstimator.Estimate(1, def);

            Assert.Greater(estimate, 0, "Optimal estimate should be positive");
        }

        [Test]
        public void OptimalMoveEstimator_CachesResults()
        {
            OptimalMoveEstimator.ClearCache();
            var def = DifficultyScaler.GetLevelDefinition(5);

            int first = OptimalMoveEstimator.Estimate(5, def);
            int second = OptimalMoveEstimator.Estimate(5, def);

            Assert.AreEqual(first, second, "Cached result should match");
        }
    }
}
