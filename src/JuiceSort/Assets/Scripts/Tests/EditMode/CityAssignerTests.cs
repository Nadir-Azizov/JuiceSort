using NUnit.Framework;
using JuiceSort.Game.LevelGen;
using System.Collections.Generic;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class CityAssignerTests
    {
        [Test]
        public void AssignCity_Deterministic()
        {
            var city1 = CityAssigner.AssignCity(5);
            var city2 = CityAssigner.AssignCity(5);

            Assert.AreEqual(city1.CityName, city2.CityName);
            Assert.AreEqual(city1.CountryName, city2.CountryName);
        }

        [Test]
        public void AssignCity_All38CitiesAppear()
        {
            var cityNames = new HashSet<string>();

            for (int level = 1; level <= 38; level++)
            {
                var city = CityAssigner.AssignCity(level);
                cityNames.Add(city.CityName);
            }

            Assert.AreEqual(38, cityNames.Count, "All 38 cities should appear in levels 1-38");
        }

        [Test]
        public void AssignCity_CyclesAfter38()
        {
            var city1 = CityAssigner.AssignCity(1);
            var city39 = CityAssigner.AssignCity(39);

            Assert.AreEqual(city1.CityName, city39.CityName, "City should cycle after 38 levels");
        }

        [Test]
        public void AssignMood_Deterministic()
        {
            var mood1 = CityAssigner.AssignMood(7);
            var mood2 = CityAssigner.AssignMood(7);

            Assert.AreEqual(mood1, mood2);
        }

        [Test]
        public void AssignMood_AlternatesMorningNight()
        {
            Assert.AreEqual(LevelMood.Morning, CityAssigner.AssignMood(1));
            Assert.AreEqual(LevelMood.Night, CityAssigner.AssignMood(2));
            Assert.AreEqual(LevelMood.Morning, CityAssigner.AssignMood(3));
        }

        [Test]
        public void LevelDefinition_CityMoodPopulated()
        {
            var def = DifficultyScaler.GetLevelDefinition(5);

            var city = CityAssigner.AssignCity(5);
            def.CityName = city.CityName;
            def.CountryName = city.CountryName;
            def.Mood = CityAssigner.AssignMood(5);

            Assert.IsNotNull(def.CityName);
            Assert.IsNotNull(def.CountryName);
            Assert.IsTrue(def.Mood == LevelMood.Morning || def.Mood == LevelMood.Night);
        }

        [Test]
        public void CityDatabase_Has38Cities()
        {
            Assert.AreEqual(38, CityDatabase.CityCount);
        }
    }
}
