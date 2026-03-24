#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;

namespace JuiceSort.Editor
{
    /// <summary>
    /// One-click TMP font asset generator.
    /// Menu: Tools > JuiceSort > Generate Font Assets
    /// </summary>
    public static class FontAssetGenerator
    {
        private const string FontSourcePath = "Assets/Art/Fonts/Nunito-Regular.ttf";
        private const string OutputFolder = "Assets/Resources/Fonts";
        private const string AssetName = "Nunito-Regular SDF";

        [MenuItem("Tools/JuiceSort/Generate Font Assets")]
        public static void GenerateFontAssets()
        {
            // Ensure output folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder(OutputFolder))
                AssetDatabase.CreateFolder("Assets/Resources", "Fonts");

            var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(FontSourcePath);
            if (sourceFont == null)
            {
                Debug.LogError($"[FontAssetGenerator] Font not found at {FontSourcePath}");
                EditorUtility.DisplayDialog("Font Not Found",
                    $"Could not find font at:\n{FontSourcePath}", "OK");
                return;
            }

            string assetPath = $"{OutputFolder}/{AssetName}.asset";

            // Delete existing corrupt asset if present
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing != null)
            {
                Debug.Log($"[FontAssetGenerator] Deleting existing asset at {assetPath} to regenerate");
                AssetDatabase.DeleteAsset(assetPath);
            }

            // Create font asset with bold simulation enabled
            var fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont, 72, 6,
                UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
                2048, 2048,
                AtlasPopulationMode.Dynamic);
            if (fontAsset == null)
            {
                Debug.LogError("[FontAssetGenerator] TMP_FontAsset.CreateFontAsset returned null");
                return;
            }

            fontAsset.name = AssetName;

            // Save the main asset
            AssetDatabase.CreateAsset(fontAsset, assetPath);

            // CRITICAL: Save the atlas texture as a sub-asset
            // Without this, the atlas reference breaks on reload
            if (fontAsset.atlasTexture != null)
            {
                fontAsset.atlasTexture.name = $"{AssetName} Atlas";
                AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, assetPath);
            }

            // Also save the material as a sub-asset
            if (fontAsset.material != null)
            {
                fontAsset.material.name = $"{AssetName} Material";
                AssetDatabase.AddObjectToAsset(fontAsset.material, assetPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[FontAssetGenerator] Created {assetPath} with atlas and material sub-assets");
            EditorUtility.DisplayDialog("Font Asset Generated",
                $"Created: {assetPath}\n\nTextMeshPro is ready to use!", "OK");
        }
    }
}
#endif
