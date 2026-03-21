using System;
using UnityEngine;

namespace JuiceSort.Game.Ads
{
    /// <summary>
    /// Placeholder ad manager. Simulates rewarded ad by immediately granting reward.
    /// Replace with Google AdMob integration for production.
    /// </summary>
    public class AdManager : MonoBehaviour, IAdManager
    {
        public bool IsAdAvailable => true; // Always available in placeholder

        public void ShowRewardedAd(Action onRewarded, Action onFailed)
        {
            // Placeholder: immediately reward (simulates watching ad)
            // In production, this would:
            // 1. Request rewarded ad from AdMob
            // 2. Show the ad
            // 3. Call onRewarded when user completes watching
            // 4. Call onFailed if ad fails to load or user skips

            Debug.Log("[AdManager] Placeholder: rewarded ad shown (immediate reward)");
            onRewarded?.Invoke();
        }
    }
}
