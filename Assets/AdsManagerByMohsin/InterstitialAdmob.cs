using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterstitialAdmob : MonoBehaviour
{

    [HideInInspector]
    public string adId;
    public Action OnContentFailed;
    public Action OnContentClosed;

    public InterstitialAd _interstitialAd;

    IEnumerator Start()
    {
        yield return new WaitUntil(() => AdsManager.Instance.Initialized);
        LoadInterstitialAd();
    }
    public void LoadInterstitialAd()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return;
        }
        // Clean up the old ad before loading a new one.
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        Debug.Log("Loading the interstitial ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        InterstitialAd.Load(adId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : "
                          + ad.GetResponseInfo());

                _interstitialAd = ad;

                ad.OnAdFullScreenContentClosed += () =>
                {
                    LoadInterstitialAd();
                    OnContentClosed.Invoke();
                    Debug.Log("Interstitial ad full screen content closed.");
                };

                ad.OnAdFullScreenContentFailed += (AdError error) =>
                {
                    LoadInterstitialAd();
                    OnContentFailed.Invoke();
                   
                };
            });
    }

    public void ShowInterstitialAd(Action onContentFailed=null, Action onContentClosed = null)
    {
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            _interstitialAd.Show();
        }
        else
        {
            Debug.LogError("Interstitial ad is not ready yet.");
        }
        OnContentFailed=onContentFailed; 
        OnContentClosed=onContentClosed;
    }



}
