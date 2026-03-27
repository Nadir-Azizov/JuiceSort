using UnityEngine;

namespace JuiceSort.Game.UI.Components
{
    /// <summary>
    /// Shared haptic feedback utility. Checks VibrationEnabled PlayerPrefs
    /// and fires a short 20ms tap on Android.
    /// </summary>
    public static class HapticUtils
    {
        public static void TryVibrate()
        {
            if (PlayerPrefs.GetInt("VibrationEnabled", 1) != 1) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var vib = activity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
                {
                    using (var effectClass = new AndroidJavaClass("android.os.VibrationEffect"))
                    {
                        var effect = effectClass.CallStatic<AndroidJavaObject>(
                            "createOneShot", 20L, 80); // 20ms, medium amplitude
                        vib.Call("vibrate", effect);
                    }
                }
            }
            catch { Handheld.Vibrate(); }
#endif
        }
    }
}
