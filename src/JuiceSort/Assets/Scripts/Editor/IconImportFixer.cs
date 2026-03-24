#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace JuiceSort.Editor
{
    /// <summary>
    /// Fixes icon PNG import settings to Sprite type.
    /// Menu: Tools > JuiceSort > Fix Icon Import Settings
    /// </summary>
    public static class IconImportFixer
    {
        [MenuItem("Tools/JuiceSort/Fix Icon Import Settings")]
        public static void Fix()
        {
            string[] icons = {
                "Assets/Resources/Icons/icon_shop.png",
                "Assets/Resources/Icons/icon_leaderboard.png",
                "Assets/Resources/Icons/icon_home.png",
                "Assets/Resources/Icons/icon_teams.png",
                "Assets/Resources/Icons/icon_collections.png"
            };

            int fixed_count = 0;
            foreach (var path in icons)
            {
                var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp != null)
                {
                    imp.textureType = TextureImporterType.Sprite;
                    imp.spriteImportMode = SpriteImportMode.Single;
                    imp.filterMode = FilterMode.Bilinear;
                    imp.maxTextureSize = 256;
                    imp.alphaIsTransparency = true;
                    imp.SaveAndReimport();
                    fixed_count++;
                    Debug.Log($"[IconImport] Fixed: {path}");
                }
                else
                {
                    Debug.LogWarning($"[IconImport] Not found: {path}");
                }
            }

            Debug.Log($"[IconImport] Fixed {fixed_count} icons to Sprite type");
            EditorUtility.DisplayDialog("Icons Fixed",
                $"{fixed_count} icons set to Sprite import type.\nReady to use!", "OK");
        }
    }
}
#endif
