using System;

namespace JuiceSort.Game.Ads
{
    /// <summary>
    /// Ad service interface. Placeholder for Google AdMob integration.
    /// </summary>
    public interface IAdManager
    {
        void ShowRewardedAd(Action onRewarded, Action onFailed);
        bool IsAdAvailable { get; }
    }
}
