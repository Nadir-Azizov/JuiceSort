#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;

namespace JuiceSort.Editor
{
    /// <summary>
    /// Converts SVG icons to PNG using system browser rendering.
    /// For now: manually convert SVGs to PNGs using any online tool
    /// (e.g., svgtopng.com, cloudconvert.com) at 256x256px.
    ///
    /// Place PNGs in: Assets/Resources/Icons/
    /// Names: icon_shop.png, icon_leaderboard.png, icon_home.png, icon_teams.png, icon_collections.png
    ///
    /// Menu: Tools > JuiceSort > Open Icons Folder
    /// </summary>
    public static class SvgToPngConverter
    {
        [MenuItem("Tools/JuiceSort/Open Icons SVG Folder")]
        public static void OpenSvgFolder()
        {
            string path = Path.GetFullPath("Assets/Art/Icons");
            if (Directory.Exists(path))
                Process.Start(path);
            else
                UnityEngine.Debug.LogError($"Folder not found: {path}");
        }

        [MenuItem("Tools/JuiceSort/Setup Icon Resources Folder")]
        public static void SetupIconsFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Icons"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                    AssetDatabase.CreateFolder("Assets", "Resources");
                AssetDatabase.CreateFolder("Assets/Resources", "Icons");
            }
            UnityEngine.Debug.Log("[Icons] Place your PNG icons (256x256) in Assets/Resources/Icons/");
            UnityEngine.Debug.Log("[Icons] Names: icon_shop.png, icon_leaderboard.png, icon_home.png, icon_teams.png, icon_collections.png");
            EditorUtility.DisplayDialog("Icons Folder Ready",
                "Place PNG icons (256x256) in:\nAssets/Resources/Icons/\n\n" +
                "Names:\n- icon_shop.png\n- icon_leaderboard.png\n- icon_home.png\n- icon_teams.png\n- icon_collections.png\n\n" +
                "Convert your SVGs at svgtopng.com or similar tool.",
                "OK");
        }
    }
}
#endif
