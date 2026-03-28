using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace JuiceSort.Game.UI.Screens
{
    /// <summary>
    /// Manages the loading scene: reuses LoadingScreen.Create() for the splash,
    /// async-loads the Boot scene, and transitions after a minimum display time.
    /// Attach to a GameObject in LoadingScene (Build Index 0).
    /// </summary>
    public class LoadingSceneManager : MonoBehaviour
    {
        /// <summary>True after LoadingScene has shown the splash. Checked by BootLoader.</summary>
        public static bool SplashCompleted { get; private set; }

        [SerializeField] private string _nextSceneName = "Boot";
        [SerializeField] private float _minimumDisplayTime = 2.0f;

        private void Start()
        {
            // Reuse the same loading screen UI that BootLoader uses for editor Play
            LoadingScreen.Create();
            StartCoroutine(LoadNextScene());
        }

        private IEnumerator LoadNextScene()
        {
            float startTime = Time.unscaledTime;

            var asyncLoad = SceneManager.LoadSceneAsync(_nextSceneName);
            asyncLoad.allowSceneActivation = false;

            while (!asyncLoad.isDone)
            {
                float elapsed = Time.unscaledTime - startTime;

                if (asyncLoad.progress >= 0.9f && elapsed >= _minimumDisplayTime)
                {
                    SplashCompleted = true;
                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }
        }
    }
}
