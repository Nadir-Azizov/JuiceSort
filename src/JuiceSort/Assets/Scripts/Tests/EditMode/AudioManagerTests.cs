using NUnit.Framework;
using JuiceSort.Game.Audio;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class AudioManagerTests
    {
        [Test]
        public void AudioClipType_HasAllRequiredValues()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(AudioClipType), AudioClipType.Pour));
            Assert.IsTrue(System.Enum.IsDefined(typeof(AudioClipType), AudioClipType.Select));
            Assert.IsTrue(System.Enum.IsDefined(typeof(AudioClipType), AudioClipType.Deselect));
            Assert.IsTrue(System.Enum.IsDefined(typeof(AudioClipType), AudioClipType.LevelComplete));
            Assert.IsTrue(System.Enum.IsDefined(typeof(AudioClipType), AudioClipType.StarAwarded));
            Assert.IsTrue(System.Enum.IsDefined(typeof(AudioClipType), AudioClipType.UITap));
        }

        [Test]
        public void AudioClipType_HasSixValues()
        {
            var values = System.Enum.GetValues(typeof(AudioClipType));
            Assert.AreEqual(6, values.Length);
        }
    }
}
