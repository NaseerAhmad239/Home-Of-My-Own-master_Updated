using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BannerAdmob : MonoBehaviour
{
    private BannerView _bannerView;
    [HideInInspector]
    public string Id;
    // Use this for initialization
    IEnumerator Start()
    {
        yield return new WaitUntil(() => AdsManager.Instance.Initialized);
        RequestBanner();

    }

    private void RequestBanner()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return;
        }
        // Clean up banner ad before creating a new one.
        if (_bannerView != null)
        {
            _bannerView.Destroy();
        }

        AdSize adaptiveSize =
                AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);

        _bannerView = new BannerView(Id, adaptiveSize, AdsManager.Instance.BannerPosition);

        // Register for ad events.
        _bannerView.OnBannerAdLoaded += OnBannerAdLoaded;
        _bannerView.OnBannerAdLoadFailed += OnBannerAdLoadFailed;


        AdRequest adRequest = new AdRequest();

        // Load a banner ad.
        _bannerView.LoadAd(adRequest);

    }

    public void ShowBanner()
    {
        if (_bannerView != null)
        {
            _bannerView.Show();
        }
    }
    public void HideBanner()
    {
        if (_bannerView != null)
        {
            _bannerView.Hide();
        }
    }
    private void OnBannerAdLoaded()
    {
        if (AdsManager.Instance.HideBannerOnStart)
        {
            HideBanner();
        }
        Debug.Log("Banner view loaded an ad with response : "
                 + _bannerView.GetResponseInfo());
        Debug.Log("Ad Height: {0}, width: {1}" +
                _bannerView.GetHeightInPixels() +
                _bannerView.GetWidthInPixels());
    }

    private void OnBannerAdLoadFailed(LoadAdError error)
    {
        Debug.LogError("Banner view failed to load an ad with error : "
                + error);
    }

}
