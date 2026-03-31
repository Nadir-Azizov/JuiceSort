using UnityEngine;
using UnityEngine.UI;

namespace JuiceSort.Game.UI.Components
{
    /// <summary>
    /// Shared factory for 3D beveled buttons — gold ring or no-gold variants.
    /// Extracts the repeated pattern from SettingsScreen into a reusable helper.
    /// Returns the face GameObject so callers can add icons, text, or layout as children.
    /// </summary>
    public static class UI3DButton
    {
        /// <summary>
        /// Creates a 3D button with gold ring bevel, colored border, colored face, gloss, and content area.
        /// Returns the face GameObject (for adding content like icons, text).
        /// </summary>
        public static GameObject Create(
            GameObject parent,
            string name,
            float width,
            float height,
            Color faceColor,
            Color borderColor,
            int goldRadius,
            int borderRadius,
            int faceRadius,
            float glossAlpha = 0.3f,
            UnityEngine.Events.UnityAction onClick = null)
        {
            var btnRoot = new GameObject(name);
            btnRoot.transform.SetParent(parent.transform, false);
            btnRoot.AddComponent<RectTransform>();
            var btnLE = btnRoot.AddComponent<LayoutElement>();
            btnLE.preferredWidth = width;
            btnLE.preferredHeight = height;

            // Gold ring with gradient bevel
            var goldRing = R(btnRoot, "GoldRing");
            goldRing.anchorMin = V(0, 0); goldRing.anchorMax = V(1, 1);
            var goldRingImg = goldRing.gameObject.AddComponent<Image>();
            goldRingImg.sprite = UIShapeUtils.WhiteRoundedRect(goldRadius, 64);
            goldRingImg.type = Image.Type.Sliced;
            goldRingImg.color = Col("#f5dc68");
            goldRingImg.raycastTarget = false;
            AddBevelOverlay(goldRing.gameObject, Col("#786010"));

            // Border (inset 8 from gold ring)
            var border = R(btnRoot, "Border");
            border.anchorMin = V(0, 0); border.anchorMax = V(1, 1);
            border.offsetMin = V(8, 8); border.offsetMax = V(-8, -8);
            var borderImg = border.gameObject.AddComponent<Image>();
            borderImg.sprite = UIShapeUtils.WhiteRoundedRect(borderRadius, 64);
            borderImg.type = Image.Type.Sliced;
            borderImg.color = borderColor;
            borderImg.raycastTarget = false;

            // Face (inset 13 from gold ring = 5 from border)
            var face = R(btnRoot, "Face");
            face.anchorMin = V(0, 0); face.anchorMax = V(1, 1);
            face.offsetMin = V(13, 13); face.offsetMax = V(-13, -13);
            var faceImg = face.gameObject.AddComponent<Image>();
            faceImg.sprite = UIShapeUtils.WhiteRoundedRect(faceRadius, 64);
            faceImg.type = Image.Type.Sliced;
            faceImg.color = faceColor;
            faceImg.raycastTarget = false;

            // Gloss (top 45% of face)
            var gloss = R(face.gameObject, "Gloss");
            gloss.anchorMin = V(0, 0.55f); gloss.anchorMax = V(1, 1);
            gloss.offsetMin = V(0, 0); gloss.offsetMax = V(0, 0);
            var glossImg = gloss.gameObject.AddComponent<Image>();
            glossImg.sprite = UIShapeUtils.WhiteRoundedRect(faceRadius, 64);
            glossImg.type = Image.Type.Sliced;
            glossImg.color = new Color(1f, 1f, 1f, glossAlpha);
            glossImg.raycastTarget = false;

            // Transparent Image for raycast + Button + ButtonBounce
            var btnImg = btnRoot.AddComponent<Image>();
            btnImg.color = Color.clear;
            var btn = btnRoot.AddComponent<Button>();
            if (onClick != null)
                btn.onClick.AddListener(onClick);
            btnRoot.AddComponent<ButtonBounce>();

            return face.gameObject;
        }

        /// <summary>
        /// Creates a 3D button WITHOUT gold ring (colored outer with bevel).
        /// Returns the face GameObject (for adding content like icons, text).
        /// </summary>
        public static GameObject CreateNoGold(
            GameObject parent,
            string name,
            Color outerColor,
            Color outerBevelDark,
            Color borderColor,
            Color faceColor,
            int outerRadius,
            int borderRadius,
            int faceRadius,
            float glossAlpha = 0.15f,
            UnityEngine.Events.UnityAction onClick = null)
        {
            var btnRoot = new GameObject(name);
            btnRoot.transform.SetParent(parent.transform, false);
            btnRoot.AddComponent<RectTransform>();

            // Outer with bevel gradient (replaces gold ring)
            var outer = R(btnRoot, "Outer");
            outer.anchorMin = V(0, 0); outer.anchorMax = V(1, 1);
            var outerImg = outer.gameObject.AddComponent<Image>();
            outerImg.sprite = UIShapeUtils.WhiteRoundedRect(outerRadius, 64);
            outerImg.type = Image.Type.Sliced;
            outerImg.color = outerColor;
            outerImg.raycastTarget = false;
            AddBevelOverlay(outer.gameObject, outerBevelDark);

            // Border (inset 8)
            var border = R(btnRoot, "Border");
            border.anchorMin = V(0, 0); border.anchorMax = V(1, 1);
            border.offsetMin = V(8, 8); border.offsetMax = V(-8, -8);
            var borderImg = border.gameObject.AddComponent<Image>();
            borderImg.sprite = UIShapeUtils.WhiteRoundedRect(borderRadius, 64);
            borderImg.type = Image.Type.Sliced;
            borderImg.color = borderColor;
            borderImg.raycastTarget = false;

            // Face (inset 13)
            var face = R(btnRoot, "Face");
            face.anchorMin = V(0, 0); face.anchorMax = V(1, 1);
            face.offsetMin = V(13, 13); face.offsetMax = V(-13, -13);
            var faceImg = face.gameObject.AddComponent<Image>();
            faceImg.sprite = UIShapeUtils.WhiteRoundedRect(faceRadius, 64);
            faceImg.type = Image.Type.Sliced;
            faceImg.color = faceColor;
            faceImg.raycastTarget = false;

            // Gloss (top 48%)
            var gloss = R(face.gameObject, "Gloss");
            gloss.anchorMin = V(0, 0.52f); gloss.anchorMax = V(1, 1);
            gloss.offsetMin = V(0, 0); gloss.offsetMax = V(0, 0);
            var glossImg = gloss.gameObject.AddComponent<Image>();
            glossImg.sprite = UIShapeUtils.WhiteRoundedRect(faceRadius, 64);
            glossImg.type = Image.Type.Sliced;
            glossImg.color = new Color(1f, 1f, 1f, glossAlpha);
            glossImg.raycastTarget = false;

            // Transparent Image for raycast + Button + ButtonBounce
            var btnImg = btnRoot.AddComponent<Image>();
            btnImg.color = Color.clear;
            var btn = btnRoot.AddComponent<Button>();
            if (onClick != null)
                btn.onClick.AddListener(onClick);
            btnRoot.AddComponent<ButtonBounce>();

            return face.gameObject;
        }

        // --- Local helpers (same pattern as SettingsScreen) ---

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

        private static Color Col(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }

        private static void AddBevelOverlay(GameObject parent, Color darkColor)
        {
            var bevel = R(parent, "Bevel");
            bevel.anchorMin = V(0, 0); bevel.anchorMax = V(1, 1);
            bevel.offsetMin = V(0, 0); bevel.offsetMax = V(0, 0);
            var bevelImg = bevel.gameObject.AddComponent<Image>();
            bevelImg.sprite = ThemeConfig.CreateGradientSprite(Color.clear, darkColor);
            bevelImg.raycastTarget = false;

            if (parent.GetComponent<Mask>() == null)
                parent.AddComponent<Mask>().showMaskGraphic = true;
        }
    }
}
