using UnityEngine;

namespace JuiceSort.Game.Effects
{
    /// <summary>
    /// Placeholder for URP bloom post-processing setup.
    /// URP Volume API requires the Universal RP package to be properly referenced.
    /// When URP post-processing is configured in the project, this script can be
    /// updated to create bloom at runtime.
    ///
    /// For now: bloom must be configured manually in Unity Editor via:
    /// 1. Add a Volume component to a GameObject
    /// 2. Create a Volume Profile
    /// 3. Add Bloom override (intensity=0.15, threshold=1.2, scatter=0.5)
    ///
    /// Or enable post-processing in the URP Renderer asset settings.
    /// </summary>
    public class BloomSetup : MonoBehaviour
    {
        [Header("Bloom Settings (for reference — apply in URP Volume)")]
        [SerializeField] private float _intensity = 0.15f;
        [SerializeField] private float _threshold = 1.2f;
        [SerializeField] private float _scatter = 0.5f;

        private void Awake()
        {
            // URP Volume/Bloom API not available in current assembly configuration.
            // Bloom should be set up via Unity Editor Volume component.
            // These values are exposed as SerializeField for documentation.
            Debug.Log($"[BloomSetup] Bloom settings: intensity={_intensity}, threshold={_threshold}, scatter={_scatter}. Configure via URP Volume in Editor.");
        }

        public static BloomSetup Create()
        {
            var go = new GameObject("BloomSetup");
            return go.AddComponent<BloomSetup>();
        }
    }
}
