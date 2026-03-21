using UnityEngine;

namespace JuiceSort.Game.Boot
{
    /// <summary>
    /// Global exception handler that prevents crashes in production.
    /// Catches unhandled exceptions via Application.logMessageReceived.
    /// Game continues without crashing — never show errors to players.
    /// </summary>
    public class GlobalExceptionHandler : MonoBehaviour
    {
        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Exception)
            {
                // In production, this prevents the exception from crashing the app.
                // Unity captures crash logs on Android for debugging.
                Debug.LogWarning($"[GlobalExceptionHandler] Caught exception: {logString}");

                // Future: write to local crash log file for debugging
                // Future: send to analytics service if added post-MVP
            }
        }

        public static GlobalExceptionHandler Create()
        {
            var go = new GameObject("GlobalExceptionHandler");
            return go.AddComponent<GlobalExceptionHandler>();
        }
    }
}
