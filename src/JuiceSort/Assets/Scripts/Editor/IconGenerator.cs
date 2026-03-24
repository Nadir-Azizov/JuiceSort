#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace JuiceSort.Editor
{
    /// <summary>
    /// Generates PNG icon textures from code (no SVG dependency).
    /// Each icon is drawn pixel-by-pixel as a simplified version of the SVG designs.
    /// Menu: Tools > JuiceSort > Generate Menu Icons
    /// </summary>
    public static class IconGenerator
    {
        private const int Size = 128;
        private const string OutputFolder = "Assets/Resources/Icons";

        // Colors matching the SVG designs
        static readonly Color DarkBg = new Color(0.08f, 0.05f, 0.19f);
        static readonly Color MidBg = new Color(0.18f, 0.11f, 0.41f);
        static readonly Color GoldDark = new Color(0.48f, 0.36f, 0.07f);
        static readonly Color GoldMid = new Color(0.77f, 0.58f, 0.13f);
        static readonly Color GoldBright = new Color(0.83f, 0.63f, 0.09f);
        static readonly Color PurpleAccent = new Color(0.29f, 0.23f, 0.63f);

        [MenuItem("Tools/JuiceSort/Generate Menu Icons")]
        public static void Generate()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder(OutputFolder))
                AssetDatabase.CreateFolder("Assets/Resources", "Icons");

            SaveIcon("icon_shop", DrawShop);
            SaveIcon("icon_leaderboard", DrawLeaderboard);
            SaveIcon("icon_home", DrawHome);
            SaveIcon("icon_teams", DrawTeams);
            SaveIcon("icon_collections", DrawCollections);

            AssetDatabase.Refresh();

            // Set all icons to Sprite import type
            string[] names = { "icon_shop", "icon_leaderboard", "icon_home", "icon_teams", "icon_collections" };
            foreach (var n in names)
            {
                var imp = AssetImporter.GetAtPath($"{OutputFolder}/{n}.png") as TextureImporter;
                if (imp != null)
                {
                    imp.textureType = TextureImporterType.Sprite;
                    imp.spriteImportMode = SpriteImportMode.Single;
                    imp.filterMode = FilterMode.Bilinear;
                    imp.maxTextureSize = 256;
                    imp.SaveAndReimport();
                }
            }

            Debug.Log("[IconGenerator] 5 menu icons generated in Assets/Resources/Icons/");
            EditorUtility.DisplayDialog("Icons Generated", "5 menu icons created!\nReady to use.", "OK");
        }

        static void SaveIcon(string name, System.Action<Texture2D> draw)
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            // Clear to transparent
            var clear = new Color(0, 0, 0, 0);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                    tex.SetPixel(x, y, clear);

            draw(tex);
            tex.Apply();

            byte[] png = tex.EncodeToPNG();
            File.WriteAllBytes($"{Application.dataPath}/Resources/Icons/{name}.png", png);
            Object.DestroyImmediate(tex);
        }

        // === SHOP: bag with coin ===
        static void DrawShop(Texture2D t)
        {
            // Bag body
            FillRoundedRect(t, 20, 10, 88, 75, 14, DarkBg);
            FillRoundedRect(t, 24, 14, 80, 67, 11, MidBg);
            // Handle
            for (int a = 0; a < 180; a++)
            {
                float rad = a * Mathf.Deg2Rad;
                int x = (int)(64 + 22 * Mathf.Cos(rad));
                int y = (int)(80 + 18 * Mathf.Sin(rad));
                FillCircle(t, x, y, 3, GoldDark);
            }
            for (int a = 0; a < 180; a++)
            {
                float rad = a * Mathf.Deg2Rad;
                int x = (int)(64 + 20 * Mathf.Cos(rad));
                int y = (int)(81 + 16 * Mathf.Sin(rad));
                FillCircle(t, x, y, 1, GoldBright);
            }
            // Coin
            FillCircle(t, 64, 42, 20, GoldDark);
            FillCircle(t, 64, 42, 16, GoldMid);
            FillCircle(t, 64, 42, 12, GoldBright);
        }

        // === LEADERBOARD: trophy ===
        static void DrawLeaderboard(Texture2D t)
        {
            // Cup body
            FillRoundedRect(t, 30, 40, 68, 55, 12, GoldDark);
            FillRoundedRect(t, 34, 44, 60, 47, 10, GoldMid);
            FillRoundedRect(t, 38, 48, 52, 39, 8, GoldBright);
            // Handles
            for (int a = -60; a < 60; a++)
            {
                float rad = a * Mathf.Deg2Rad;
                FillCircle(t, (int)(26 + 10 * Mathf.Cos(rad)), (int)(60 + 14 * Mathf.Sin(rad)), 3, GoldDark);
                FillCircle(t, (int)(102 + 10 * Mathf.Cos(rad)), (int)(60 + 14 * Mathf.Sin(rad)), 3, GoldDark);
            }
            // Base
            FillRoundedRect(t, 46, 12, 36, 8, 2, DarkBg);
            FillRoundedRect(t, 36, 6, 56, 10, 5, MidBg);
            // Star
            FillCircle(t, 64, 60, 8, DarkBg);
        }

        // === HOME: house ===
        static void DrawHome(Texture2D t)
        {
            // Roof triangle
            for (int y = 50; y < 85; y++)
            {
                int w = (int)((85 - y) * 1.5f);
                for (int x = 64 - w; x < 64 + w; x++)
                    if (x >= 0 && x < Size) t.SetPixel(x, y, MidBg);
            }
            // Walls
            FillRoundedRect(t, 22, 10, 84, 48, 4, DarkBg);
            FillRoundedRect(t, 26, 14, 76, 40, 3, MidBg);
            // Door
            FillRoundedRect(t, 48, 10, 32, 36, 7, GoldDark);
            FillRoundedRect(t, 51, 13, 26, 33, 5, GoldMid);
            // Door knob
            FillCircle(t, 71, 30, 3, GoldDark);
            // Window
            FillRoundedRect(t, 30, 28, 18, 16, 4, GoldDark);
            FillRoundedRect(t, 32, 30, 14, 12, 3, GoldBright);
            // Roof line
            for (int x = 14; x < 114; x++)
                FillCircle(t, x, 50, 2, GoldMid);
        }

        // === TEAMS: people with crown ===
        static void DrawTeams(Texture2D t)
        {
            // Back people (darker)
            FillCircle(t, 32, 80, 14, DarkBg);
            FillRoundedRect(t, 18, 30, 28, 40, 10, DarkBg);
            FillCircle(t, 96, 80, 14, DarkBg);
            FillRoundedRect(t, 82, 30, 28, 40, 10, DarkBg);
            // Main person
            FillCircle(t, 64, 78, 18, GoldDark);
            FillCircle(t, 64, 78, 14, GoldMid);
            // Body
            FillRoundedRect(t, 32, 15, 64, 45, 14, GoldDark);
            FillRoundedRect(t, 36, 19, 56, 37, 12, GoldMid);
            // Crown
            for (int i = 0; i < 5; i++)
            {
                int cx = 48 + i * 8;
                FillCircle(t, cx, 96 + (i % 2 == 0 ? 4 : 0), 4, MidBg);
            }
            FillRoundedRect(t, 46, 88, 36, 8, 2, MidBg);
            // Eyes
            FillCircle(t, 56, 76, 3, DarkBg);
            FillCircle(t, 72, 76, 3, DarkBg);
        }

        // === COLLECTIONS: grid of items ===
        static void DrawCollections(Texture2D t)
        {
            // Background card
            FillRoundedRect(t, 14, 10, 100, 108, 13, DarkBg);
            FillRoundedRect(t, 18, 14, 92, 100, 10, MidBg);
            // 4 grid cells
            int[] gx = { 24, 68 };
            int[] gy = { 58, 20 };
            for (int r = 0; r < 2; r++)
            {
                for (int c = 0; c < 2; c++)
                {
                    if (r == 1 && c == 1) // Lock cell
                    {
                        FillRoundedRect(t, gx[c], gy[r], 34, 34, 7, PurpleAccent);
                        FillCircle(t, gx[c] + 17, gy[r] + 20, 6, DarkBg);
                    }
                    else // Gold cell
                    {
                        FillRoundedRect(t, gx[c], gy[r], 34, 34, 7, GoldDark);
                        FillRoundedRect(t, gx[c] + 3, gy[r] + 3, 28, 28, 5, GoldMid);
                        FillCircle(t, gx[c] + 17, gy[r] + 17, 8, DarkBg);
                        FillCircle(t, gx[c] + 17, gy[r] + 17, 5, MidBg);
                    }
                }
            }
        }

        // === Drawing primitives ===
        static void FillCircle(Texture2D t, int cx, int cy, int r, Color c)
        {
            for (int y = cy - r; y <= cy + r; y++)
                for (int x = cx - r; x <= cx + r; x++)
                    if (x >= 0 && x < Size && y >= 0 && y < Size)
                        if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r)
                            t.SetPixel(x, y, c);
        }

        static void FillRoundedRect(Texture2D t, int rx, int ry, int rw, int rh, int rad, Color c)
        {
            for (int y = ry; y < ry + rh && y < Size; y++)
            {
                for (int x = rx; x < rx + rw && x < Size; x++)
                {
                    if (x < 0 || y < 0) continue;
                    // Check corners
                    bool inside = true;
                    if (x < rx + rad && y < ry + rad)
                        inside = Sq(x - rx - rad, y - ry - rad) <= rad * rad;
                    else if (x >= rx + rw - rad && y < ry + rad)
                        inside = Sq(x - rx - rw + rad, y - ry - rad) <= rad * rad;
                    else if (x < rx + rad && y >= ry + rh - rad)
                        inside = Sq(x - rx - rad, y - ry - rh + rad) <= rad * rad;
                    else if (x >= rx + rw - rad && y >= ry + rh - rad)
                        inside = Sq(x - rx - rw + rad, y - ry - rh + rad) <= rad * rad;
                    if (inside) t.SetPixel(x, y, c);
                }
            }
        }

        static int Sq(int a, int b) => a * a + b * b;
    }
}
#endif
