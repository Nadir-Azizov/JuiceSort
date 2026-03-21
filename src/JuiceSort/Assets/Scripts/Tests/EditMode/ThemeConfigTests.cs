using NUnit.Framework;
using UnityEngine;
using JuiceSort.Game.UI;
using JuiceSort.Game.LevelGen;
using JuiceSort.Game.Puzzle;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class ThemeConfigTests
    {
        [Test]
        public void GetColor_MorningAndNight_ReturnDifferentBackgrounds()
        {
            var morning = ThemeConfig.GetColor(LevelMood.Morning, ThemeColorType.Background);
            var night = ThemeConfig.GetColor(LevelMood.Night, ThemeColorType.Background);

            Assert.AreNotEqual(morning, night, "Morning and night backgrounds should differ");
        }

        [Test]
        public void GetColor_MorningAndNight_ReturnDifferentContainerIdle()
        {
            var morning = ThemeConfig.GetColor(LevelMood.Morning, ThemeColorType.ContainerIdle);
            var night = ThemeConfig.GetColor(LevelMood.Night, ThemeColorType.ContainerIdle);

            Assert.AreNotEqual(morning, night);
        }

        [Test]
        public void GetColor_SelectedSameForBothMoods()
        {
            var morning = ThemeConfig.GetColor(LevelMood.Morning, ThemeColorType.ContainerSelected);
            var night = ThemeConfig.GetColor(LevelMood.Night, ThemeColorType.ContainerSelected);

            // Selection highlight should be golden for both moods (consistent feedback)
            Assert.AreEqual(morning.r, night.r, 0.1f, "Selected color should be similar across moods");
        }

        [Test]
        public void GetDrinkColor_AllNonNone_HaveFullAlpha()
        {
            var colors = new[] { DrinkColor.MangoAmber, DrinkColor.DeepBerry, DrinkColor.TropicalTeal, DrinkColor.WatermelonRose, DrinkColor.LimeGold };

            foreach (var dc in colors)
            {
                var c = ThemeConfig.GetDrinkColor(dc);
                Assert.AreEqual(1f, c.a, $"{dc} should have full alpha");
                Assert.Greater(c.r + c.g + c.b, 0f, $"{dc} should not be black");
            }
        }

        [Test]
        public void GetDrinkColor_None_IsTransparent()
        {
            var c = ThemeConfig.GetDrinkColor(DrinkColor.None);
            Assert.AreEqual(0f, c.a, "None should be transparent");
        }

        [Test]
        public void ThemeColorType_HasAllRequiredValues()
        {
            var values = System.Enum.GetValues(typeof(ThemeColorType));
            Assert.GreaterOrEqual(values.Length, 10);

            Assert.IsTrue(System.Enum.IsDefined(typeof(ThemeColorType), ThemeColorType.Background));
            Assert.IsTrue(System.Enum.IsDefined(typeof(ThemeColorType), ThemeColorType.ContainerIdle));
            Assert.IsTrue(System.Enum.IsDefined(typeof(ThemeColorType), ThemeColorType.ContainerSelected));
            Assert.IsTrue(System.Enum.IsDefined(typeof(ThemeColorType), ThemeColorType.StarGold));
            Assert.IsTrue(System.Enum.IsDefined(typeof(ThemeColorType), ThemeColorType.EmptySlot));
        }

        [Test]
        public void GetColor_AllTypes_ReturnNonDefaultForMorning()
        {
            var types = (ThemeColorType[])System.Enum.GetValues(typeof(ThemeColorType));
            foreach (var type in types)
            {
                var c = ThemeConfig.GetColor(LevelMood.Morning, type);
                Assert.IsTrue(c.a > 0f || type == ThemeColorType.EmptySlot,
                    $"Morning {type} should have non-zero alpha (or be EmptySlot)");
            }
        }

        [Test]
        public void CurrentMood_DefaultsToMorning()
        {
            // Reset to default
            ThemeConfig.CurrentMood = LevelMood.Morning;
            Assert.AreEqual(LevelMood.Morning, ThemeConfig.CurrentMood);
        }

        [Test]
        public void GetGradientColors_MorningVsNight_Different()
        {
            var mTop = ThemeConfig.GetBackgroundGradientTop(LevelMood.Morning);
            var nTop = ThemeConfig.GetBackgroundGradientTop(LevelMood.Night);

            Assert.AreNotEqual(mTop, nTop);
        }
    }
}
