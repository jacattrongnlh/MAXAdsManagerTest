using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omnilatent.AdsMediation.MAXWrapper
{
    public partial class MAXAdsWrapper : MonoBehaviour, IAdsNetworkHelper
    {
        RewardAdObject currentRewardAd;
        public static float TIMEOUT_LOADREWARDAD = 12f;

        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onRewardAdLoadedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.ErrorInfo> onRewardAdLoadFailedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onRewardAdDisplayedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onRewardAdClickedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onRewardAdRevenuePaidEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onRewardAdHiddenEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.ErrorInfo> onRewardAdDisplayFailedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onRewardAdReceivedRewardEvent;

        RewardAdObject GetCurrentRewardAd(bool makeNewIfNull = true)
        {
            if (currentRewardAd == null)
            {
                Debug.LogError("currentRewardAd is null.");
                if (makeNewIfNull)
                {
                    Debug.Log("New ad will be created");
                    currentRewardAd = new RewardAdObject();
                }
            }
            return currentRewardAd;
        }

        public void Reward(AdPlacement.Type placementType, RewardDelegate onFinish)
        {
            currentRewardAd = new RewardAdObject(placementType, onFinish);
            StartCoroutine(CoReward(placementType, onFinish));
        }

        IEnumerator CoReward(AdPlacement.Type placementType, RewardDelegate onFinish)
        {
            float _timeoutRequestAds = TIMEOUT_LOADREWARDAD;

            string adUnitId = MAXAdID.GetAdID(placementType);
            MaxSdk.LoadRewardedAd(adUnitId);
            GetCurrentRewardAd().State = AdObjectState.Loading;

            float retryInterval = 0.4f;
            WaitForSecondsRealtime delay = new WaitForSecondsRealtime(retryInterval);
            int tryTimes = 0;
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                _timeoutRequestAds = 3f;
            }
            while (!MaxSdk.IsRewardedAdReady(adUnitId) && tryTimes < _timeoutRequestAds / retryInterval)
            {
                yield return delay;
                tryTimes++;
            }
            //.Log("reward ad available:" + (GetCurrentRewardAd().State == AdObjectState.Loading));

            if (MaxSdk.IsRewardedAdReady(adUnitId))
            {
                GetCurrentRewardAd().State = AdObjectState.Showing;
                MaxSdk.ShowRewardedAd(adUnitId);
            }
            else
            {
                onFinish?.Invoke(new RewardResult(RewardResult.Type.LoadFailed, "Self timeout"));
            }
            //if (showLoading)
            //    Manager.LoadingAnimation(false);
        }

        public void InitializeRewardedAds()
        {
            // Attach callback
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
        }

        private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            QueueMainThreadExecution(() =>
            {
                onRewardAdLoadFailedEvent?.Invoke(GetCurrentRewardAd().AdPlacementType, errorInfo);
            });
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            QueueMainThreadExecution(() =>
            {
                onRewardAdDisplayFailedEvent?.Invoke(GetCurrentRewardAd().AdPlacementType, errorInfo);
            });
        }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            QueueMainThreadExecution(() =>
            {
                GetCurrentRewardAd().onAdClosed?.Invoke(new RewardResult(RewardResult.Type.Finished));
                GetCurrentRewardAd().State = AdObjectState.Shown;
                onRewardAdReceivedRewardEvent?.Invoke(GetCurrentRewardAd().AdPlacementType, adInfo);
            });
        }

        private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Ad revenue paid. Use this callback to track user revenue.
        }
    }
}