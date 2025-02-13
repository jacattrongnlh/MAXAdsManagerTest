using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omnilatent.AdsMediation.MAXWrapper
{
    public partial class MAXAdsWrapper : MonoBehaviour, IAdsNetworkHelper
    {
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onBannerAdLoadedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.ErrorInfo> onBannerAdLoadFailedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onBannerAdClickedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onBannerAdRevenuePaidEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onBannerAdExpandedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onBannerAdCollapsedEvent;

        BannerAdObject currentBannerAd;

        BannerAdObject GetCurrentBannerAd(bool makeNewIfNull = true)
        {
            if (currentBannerAd == null)
            {
                Debug.LogError("currentBannerAd is null.");
                if (makeNewIfNull)
                {
                    Debug.Log("New ad will be created");
                    currentBannerAd = new BannerAdObject();
                }
            }
            return currentBannerAd;
        }

        public void ShowBanner(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdLoaded = null)
        {
            ShowBanner(placementType, new BannerTransform(AdPosition.Bottom), onAdLoaded);
        }

        public void ShowBanner(AdPlacement.Type placementType, BannerTransform bannerTransform, AdsManager.InterstitialDelegate onAdLoaded = null)
        {
            if (currentBannerAd != null && currentBannerAd.AdPlacementType == placementType && currentBannerAd.State != AdObjectState.LoadFailed)
            {
                if (currentBannerAd.State == AdObjectState.Closed)
                {
                    MaxSdk.ShowBanner(MAXAdID.GetAdID(currentBannerAd.AdPlacementType));
                }
            }
            else
            {
                MaxSdkBase.BannerPosition bannerPosition = MaxSdkBase.BannerPosition.BottomCenter;
                switch (bannerTransform.adPosition)
                {
                    case AdPosition.Top:
                        bannerPosition = MaxSdkBase.BannerPosition.TopCenter;
                        break;
                    case AdPosition.TopLeft:
                        bannerPosition = MaxSdkBase.BannerPosition.TopLeft;
                        break;
                    case AdPosition.TopRight:
                        bannerPosition = MaxSdkBase.BannerPosition.TopRight;
                        break;
                    case AdPosition.BottomLeft:
                        bannerPosition = MaxSdkBase.BannerPosition.BottomLeft;
                        break;
                    case AdPosition.BottomRight:
                        bannerPosition = MaxSdkBase.BannerPosition.BottomRight;
                        break;
                    case AdPosition.Center:
                        bannerPosition = MaxSdkBase.BannerPosition.Centered;
                        break;
                }
                currentBannerAd = new BannerAdObject(placementType);
                currentBannerAd.State = AdObjectState.Loading;
                string bannerAdUnitId = MAXAdID.GetAdID(placementType);

                // Banners are automatically sized to 320�50 on phones and 728�90 on tablets
                // You may call the utility method MaxSdkUtils.isTablet() to help with view sizing adjustments
                MaxSdk.CreateBanner(bannerAdUnitId, bannerPosition);

                // Set background or background color for banners to be fully functional
                MaxSdk.SetBannerBackgroundColor(bannerAdUnitId, Color.black);
            }
        }

        public void HideBanner()
        {
            if (currentBannerAd != null)
            {
                MaxSdk.HideBanner(MAXAdID.GetAdID(GetCurrentBannerAd().AdPlacementType));
                GetCurrentBannerAd().State = AdObjectState.Closed;
            }
        }

        public void InitializeBannerAds()
        {
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnBannerAdExpandedEvent;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnBannerAdCollapsedEvent;
        }

        private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            QueueMainThreadExecution(() =>
            {
                if (GetCurrentBannerAd().State != AdObjectState.Closed)
                {
                    GetCurrentBannerAd().State = AdObjectState.Showing;
                    MaxSdk.ShowBanner(MAXAdID.GetAdID(GetCurrentBannerAd().AdPlacementType));
                }
            });
        }

        private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            QueueMainThreadExecution(() =>
            {
                if (GetCurrentBannerAd().State != AdObjectState.LoadFailed) //this check is required because AppLovin is calling this method too many times
                {
                    GetCurrentBannerAd().State = AdObjectState.LoadFailed;
                    onBannerAdLoadFailedEvent?.Invoke(GetCurrentBannerAd().AdPlacementType, errorInfo);
                }
            });
        }

        private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnBannerAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }
    }
}