using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppOpen : MonoBehaviour
{
    [HideInInspector]
    public string adId = "ca-app-pub-3940256099942544/9257395921";

    private AppOpenAd appOpenAd;

    public bool IsAdAvailable
    {
        get
        {
            return appOpenAd != null;
        }
    }

    //private void Awake()
    //{
    //    AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
    //}

    //private void OnDestroy()
    //{
    //    AppStateEventNotifier.AppStateChanged -= OnAppStateChanged;
    //}


    IEnumerator Start()
    {
        yield return new WaitUntil(() => AdsManager.Instance.Initialized);
        Invoke(nameof(LoadAppOpenAd), 3);
    }

    public void LoadAppOpenAd()
    {
        if(Application.internetReachability==NetworkReachability.NotReachable)
        {
            return;
        }
        if (appOpenAd != null)
        {
            appOpenAd.Destroy();
            appOpenAd = null;
        }

        Debug.Log("Loading the app open ad.");

        var adRequest = new AdRequest();

        AppOpenAd.Load(adId, adRequest,
            (AppOpenAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("app open ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("App open ad loaded with response : "
                          + ad.GetResponseInfo());

                appOpenAd = ad;
                RegisterEventHandlers(ad);
            });

    }
    
    public void OnApplicationFocus(bool focus)
    {
        if (focus&&IsAdAvailable && !AdsManager.Instance.OtherAdRunning && AdsManager.Instance.isAppOpen)
        {
            ShowAppOpenAd();
        }
    }

    public void ShowAppOpenAd()
    {

        if (appOpenAd != null && appOpenAd.CanShowAd())
        {
            Debug.Log("Showing app open ad.");
            appOpenAd.Show();
        }
        else
        {
            Debug.LogError("App open ad is not ready yet.");
        }

    }

    private void RegisterEventHandlers(AppOpenAd ad)
    {
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("App open ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("App open ad recorded an impression.");
        };
        ad.OnAdClicked += () =>
        {
            Debug.Log("App open ad was clicked.");
        };
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("App open ad full screen content opened.");
        };
        ad.OnAdFullScreenContentClosed += () =>
        {LoadAppOpenAd();
            Debug.Log("App open ad full screen content closed.");
        };
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            LoadAppOpenAd();
            Debug.LogError("App open ad failed to open full screen content " +
                           "with error : " + error);
        };
    }
    


}
