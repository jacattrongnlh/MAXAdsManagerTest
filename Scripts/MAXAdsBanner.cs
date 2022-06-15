using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omnilatent.AdsMediation.MAXWrapper
{
    public partial class MAXAdsWrapper : MonoBehaviour, IAdsNetworkHelper
    {
        public void ShowBanner(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdLoaded = null)
        {
            throw new System.NotImplementedException();
        }

        public void ShowBanner(AdPlacement.Type placementType, BannerTransform bannerTransform, AdsManager.InterstitialDelegate onAdLoaded = null)
        {
            throw new System.NotImplementedException();
        }

        public void HideBanner()
        {
            throw new System.NotImplementedException();
        }
    }
}