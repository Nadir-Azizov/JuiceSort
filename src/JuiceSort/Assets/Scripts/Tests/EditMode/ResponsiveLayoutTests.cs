using NUnit.Framework;
using UnityEngine;
using JuiceSort.Game.Layout;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class ResponsiveLayoutTests
    {
        private LayoutConfig _config;

        // Standard camera: orthoSize=5 (height=10), 16:9 aspect
        private const float OrthoSize = 5f;
        private const float Aspect16x9 = 9f / 16f; // portrait: width < height
        private const float Aspect20x9 = 9f / 20f;

        [SetUp]
        public void SetUp()
        {
            _config = LayoutConfig.Default();
        }

        // --- Single-row tests ---

        [Test]
        public void CalculateLayout_4Bottles_SingleRow()
        {
            var layout = ResponsiveLayoutManager.CalculateLayout(4, OrthoSize, Aspect16x9, _config);

            Assert.AreEqual(1, layout.RowCount);
            Assert.AreEqual(4, layout.TopRowCount);
            Assert.AreEqual(0, layout.BottomRowCount);
            Assert.AreEqual(4, layout.Positions.Length);
        }

        [Test]
        public void CalculateLayout_6Bottles_StillSingleRow()
        {
            var layout = ResponsiveLayoutManager.CalculateLayout(6, OrthoSize, Aspect16x9, _config);

            Assert.AreEqual(1, layout.RowCount);
            Assert.AreEqual(6, layout.TopRowCount);
        }

        // --- Two-row transition ---

        [Test]
        public void CalculateLayout_7Bottles_TwoRows()
        {
            var layout = ResponsiveLayoutManager.CalculateLayout(7, OrthoSize, Aspect16x9, _config);

            Assert.AreEqual(2, layout.RowCount);
            Assert.AreEqual(7, layout.Positions.Length);
        }

        // --- Row distribution ---

        [Test]
        public void CalculateLayout_7Bottles_TopRowGetsExtra()
        {
            var layout = ResponsiveLayoutManager.CalculateLayout(7, OrthoSize, Aspect16x9, _config);

            Assert.AreEqual(4, layout.TopRowCount);
            Assert.AreEqual(3, layout.BottomRowCount);
        }

        [Test]
        public void CalculateLayout_9Bottles_TopRowGetsExtra()
        {
            var layout = ResponsiveLayoutManager.CalculateLayout(9, OrthoSize, Aspect16x9, _config);

            Assert.AreEqual(5, layout.TopRowCount);
            Assert.AreEqual(4, layout.BottomRowCount);
        }

        [Test]
        public void CalculateLayout_11Bottles_TopRowGetsExtra()
        {
            var layout = ResponsiveLayoutManager.CalculateLayout(11, OrthoSize, Aspect16x9, _config);

            Assert.AreEqual(6, layout.TopRowCount);
            Assert.AreEqual(5, layout.BottomRowCount);
        }

        [Test]
        public void CalculateLayout_10Bottles_EvenSplit()
        {
            var layout = ResponsiveLayoutManager.CalculateLayout(10, OrthoSize, Aspect16x9, _config);

            Assert.AreEqual(5, layout.TopRowCount);
            Assert.AreEqual(5, layout.BottomRowCount);
        }

        // --- Scale clamping ---

        [Test]
        public void CalculateLayout_14Bottles_ScaleAboveMinimum()
        {
            var layout = ResponsiveLayoutManager.CalculateLayout(14, OrthoSize, Aspect16x9, _config);

            Assert.GreaterOrEqual(layout.Scale, _config.MinScale);
            Assert.LessOrEqual(layout.Scale, _config.MaxScale);
        }

        [Test]
        public void CalculateLayout_4Bottles_ScaleBelowMaximum()
        {
            var layout = ResponsiveLayoutManager.CalculateLayout(4, OrthoSize, Aspect16x9, _config);

            Assert.LessOrEqual(layout.Scale, _config.MaxScale);
            Assert.GreaterOrEqual(layout.Scale, _config.MinScale);
        }

        // --- Edge case: 1 bottle ---

        [Test]
        public void CalculateLayout_1Bottle_CenteredAtOrigin()
        {
            var layout = ResponsiveLayoutManager.CalculateLayout(1, OrthoSize, Aspect16x9, _config);

            Assert.AreEqual(1, layout.Positions.Length);
            Assert.AreEqual(0f, layout.Positions[0].x, 0.001f);
        }

        // --- Symmetry ---

        [TestCase(3)]
        [TestCase(5)]
        [TestCase(7)]
        [TestCase(10)]
        public void CalculateLayout_PositionsSymmetricAroundX0(int count)
        {
            var layout = ResponsiveLayoutManager.CalculateLayout(count, OrthoSize, Aspect16x9, _config);

            for (int i = 0; i < layout.Positions.Length; i++)
            {
                // For every position at +x, there should be one at -x
                bool foundMirror = false;
                for (int j = 0; j < layout.Positions.Length; j++)
                {
                    if (Mathf.Abs(layout.Positions[i].x + layout.Positions[j].x) < 0.001f &&
                        Mathf.Abs(layout.Positions[i].y - layout.Positions[j].y) < 0.001f)
                    {
                        foundMirror = true;
                        break;
                    }
                }
                Assert.IsTrue(foundMirror, $"Position {layout.Positions[i]} has no symmetric mirror");
            }
        }

        // --- Reserve zones ---

        [TestCase(4)]
        [TestCase(7)]
        [TestCase(14)]
        public void CalculateLayout_NoPositionInReserveZones(int count)
        {
            var layout = ResponsiveLayoutManager.CalculateLayout(count, OrthoSize, Aspect16x9, _config);

            float bottleHalfH = _config.BottleSpriteHeight * layout.Scale / 2f;
            float topLimit = OrthoSize - _config.TopReserve;
            float bottomLimit = -OrthoSize + _config.BottomReserve;

            for (int i = 0; i < layout.Positions.Length; i++)
            {
                float worldY = layout.BoardY + layout.Positions[i].y;
                float bottleTop = worldY + bottleHalfH;
                float bottleBottom = worldY - bottleHalfH;

                Assert.LessOrEqual(bottleTop, topLimit + 0.01f,
                    $"Bottle {i} top ({bottleTop:F2}) exceeds top reserve ({topLimit:F2})");
                Assert.GreaterOrEqual(bottleBottom, bottomLimit - 0.01f,
                    $"Bottle {i} bottom ({bottleBottom:F2}) exceeds bottom reserve ({bottomLimit:F2})");
            }
        }

        // --- Aspect ratio ---

        [Test]
        public void CalculateLayout_20x9Aspect_ProducesValidLayout()
        {
            var layout = ResponsiveLayoutManager.CalculateLayout(10, OrthoSize, Aspect20x9, _config);

            Assert.AreEqual(10, layout.Positions.Length);
            Assert.GreaterOrEqual(layout.Scale, _config.MinScale);
            Assert.AreEqual(2, layout.RowCount);
        }

        // --- Zero / edge cases ---

        [Test]
        public void CalculateLayout_0Bottles_ReturnsEmptyLayout()
        {
            var layout = ResponsiveLayoutManager.CalculateLayout(0, OrthoSize, Aspect16x9, _config);

            Assert.AreEqual(0, layout.Positions.Length);
            Assert.AreEqual(0, layout.RowCount);
        }

        [Test]
        public void CalculateLayout_NullConfig_UsesDefaults()
        {
            var layout = ResponsiveLayoutManager.CalculateLayout(4, OrthoSize, Aspect16x9, null);

            Assert.AreEqual(4, layout.Positions.Length);
            Assert.AreEqual(1, layout.RowCount);
        }
    }
}
