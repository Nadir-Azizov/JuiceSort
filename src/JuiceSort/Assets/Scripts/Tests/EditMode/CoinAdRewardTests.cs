using NUnit.Framework;
using JuiceSort.Game.Economy;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class CoinAdRewardTests
    {
        private CoinConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = CoinConfig.Default();
        }

        [Test]
        public void AdRewardAmount_Default_Is250()
        {
            Assert.AreEqual(250, _config.AdRewardAmount);
        }

        [Test]
        public void AdRewardAmount_CustomValue_Respected()
        {
            var customConfig = new CoinConfig { AdRewardAmount = 400 };
            Assert.AreEqual(400, customConfig.AdRewardAmount);
        }

        [Test]
        public void AdRewardAmount_IsPositive()
        {
            Assert.Greater(_config.AdRewardAmount, 0, "Ad reward should be a positive amount");
        }
    }
}
