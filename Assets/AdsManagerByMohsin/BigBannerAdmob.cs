using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigBannerAdmob : MonoBehaviour
{
    [HideInInspector]
    public string adId;

    BannerView _bannerView;


    IEnumerator Start()
    {
        yield return new WaitUntil(() => AdsManager.Instance.Initialized);
        Invoke(nameof(LoadAd), 3);
    }
    public void CreateBannerView()
    {
        Debug.Log("Creating banner view");

        // If we already have a banner, destroy the old one.
        if (_bannerView != null)
        {
            DestroyBannerView();
        }

        // Create a 320x50 banner at top of the screen
        _bannerView = new BannerView(adId, AdSize.MediumRectangle, AdsManager.Instance.BigBannerPosition);
        _bannerView.OnBannerAdLoaded += OnBigBannerAdLoaded;
        _bannerView.OnBannerAdLoadFailed += OnBigBannerAdLoadFailed;

    }
    public void LoadAd()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return;
        }
        // create an instance of a banner view first.
        if (_bannerView == null)
        {
            CreateBannerView();
        }

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        Debug.Log("Loading banner ad.");
        _bannerView.LoadAd(adRequest);
    }
    public void DestroyBannerView()
    {
        if (_bannerView != null)
        {
            Debug.Log("Destroying banner view.");
            _bannerView.Destroy();
            _bannerView = null;
        }
    }

    public void ShowBigBanner()
    {
        if (_bannerView != null)
        {
            _bannerView.Show();
        }
    } public void HideBigBanner()
    {
        if (_bannerView != null)
        {
            _bannerView.Hide();
        }
    }
    private void OnBigBannerAdLoaded()
    {
        if (AdsManager.Instance.HideBigBannerOnStart)
        {
            HideBigBanner();
        }
        Debug.Log("Banner view loaded an ad with response : "
                 + _bannerView.GetResponseInfo());
        Debug.Log("Ad Height: {0}, width: {1}" +
                _bannerView.GetHeightInPixels() +
                _bannerView.GetWidthInPixels());
    }

    private void OnBigBannerAdLoadFailed(LoadAdError error)
    {
        Debug.LogError("Banner view failed to load an ad with error : "
                + error);
    }
}
