using NUnit.Framework;
using JuiceSort.Game.UI;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class ScreenManagerTests
    {
        [Test]
        public void GameFlowState_HasAllRequiredValues()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameFlowState), GameFlowState.MainMenu));
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameFlowState), GameFlowState.Roadmap));
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameFlowState), GameFlowState.Playing));
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameFlowState), GameFlowState.LevelComplete));
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameFlowState), GameFlowState.Settings));
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameFlowState), GameFlowState.Gate));
        }

        [Test]
        public void GameFlowState_HasSixValues()
        {
            var values = System.Enum.GetValues(typeof(GameFlowState));
            Assert.AreEqual(6, values.Length);
        }
    }
}
