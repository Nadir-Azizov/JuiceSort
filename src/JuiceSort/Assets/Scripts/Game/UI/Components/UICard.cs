using UnityEngine;
using UnityEngine.UI;

namespace JuiceSort.Game.UI.Components
{
    /// <summary>
    /// Reusable card container with border ring, fill, top shadow, and bottom highlight.
    /// Mirrors the visual pattern from SettingsScreen.CreateCard().
    /// </summary>
    public static class UICard
    {
        /// <summary>
        /// Creates a card container with border ring, fill, top shadow, bottom highlight.
        /// Returns the card GameObject. Add content as children.
        /// </summary>
        public static GameObject Create(
            GameObject parent,
            string name,
            Color fillColor,
            Color borderColor,
            int cornerRadius = 20,
            float borderWidth = 8f,
            float shadowHeight = 8f,
            float highlightHeight = 5f)
        {
            var cardGo = new GameObject(name);
            cardGo.transform.SetParent(parent.transform, false);
            cardGo.AddComponent<RectTransform>();

            // Border ring (behind)
            var border = R(cardGo, "Border");
            border.anchorMin = V(0, 0); border.anchorMax = V(1, 1);
            border.offsetMin = V(-borderWidth, -borderWidth);
            border.offsetMax = V(borderWidth, borderWidth);
            var borderImg = border.gameObject.AddComponent<Image>();
            borderImg.sprite = UIShapeUtils.WhiteRoundedRect(cornerRadius + 4, 64);
            borderImg.type = Image.Type.Sliced;
            borderImg.color = borderColor;
            borderImg.raycastTarget = false;

            // Card fill
            var fill = R(cardGo, "Fill");
            fill.anchorMin = V(0, 0); fill.anchorMax = V(1, 1);
            var fillImg = fill.gameObject.AddComponent<Image>();
            fillImg.sprite = UIShapeUtils.WhiteRoundedRect(cornerRadius, 64);
            fillImg.type = Image.Type.Sliced;
            fillImg.color = fillColor;
            fillImg.raycastTarget = false;

            // Top inset shadow
            var sh = R(cardGo, "TopShadow");
            sh.anchorMin = V(0, 1); sh.anchorMax = V(1, 1);
            sh.pivot = V(0.5f, 1); sh.sizeDelta = V(0, shadowHeight);
            var shImg = sh.gameObject.AddComponent<Image>();
            shImg.color = new Color(0, 0, 0, 0.15f);
            shImg.raycastTarget = false;

            // Bottom inset highlight
            var hl = R(cardGo, "BottomHighlight");
            hl.anchorMin = V(0, 0); hl.anchorMax = V(1, 0);
            hl.pivot = V(0.5f, 0); hl.sizeDelta = V(0, highlightHeight);
            var hlImg = hl.gameObject.AddComponent<Image>();
            hlImg.color = new Color(1, 1, 1, 0.04f);
            hlImg.raycastTarget = false;

            return cardGo;
        }

        private static Vector2 V(float x, float y) => new Vector2(x, y);

        private static RectTransform R(GameObject p, string n)
        {
            var g = new GameObject(n);
            g.transform.SetParent(p.transform, false);
            var r = g.AddComponent<RectTransform>();
            r.anchorMin = V(0, 0); r.anchorMax = V(1, 1);
            r.offsetMin = V(0, 0); r.offsetMax = V(0, 0);
            return r;
        }
    }
}
