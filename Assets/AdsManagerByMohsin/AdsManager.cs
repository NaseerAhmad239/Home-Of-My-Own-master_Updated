using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[RequireComponent(typeof(BannerAdmob))]
[RequireComponent(typeof(AppOpen))]
[RequireComponent(typeof(InterstitialAdmob))]
[RequireComponent(typeof(RewardedAdmob))]
[RequireComponent(typeof(BigBannerAdmob))]
public class AdsManager : MonoBehaviour
{
    public AdPosition BannerPosition;
    public AdPosition BigBannerPosition;
    public Ids AndroidAdIds;
    public Ids IosAdIds;

    public bool IsTesting;
    public GameObject LoadingPanel;
    public TMP_Text LoadingText;
    public InterstitialAdmob Interstitial;
    public RewardedAdmob Rewarded;
    public BannerAdmob Banner;
    public BigBannerAdmob BigBanner;
    public AppOpen AppOpen;
    public bool HideBigBannerOnStart;
    public bool HideBannerOnStart;
    public bool isAppOpen;
    public static AdsManager Instance;
    float Timer;
    public bool Initialized;
    public bool OtherAdRunning;
    public Times AdTimers;

    public bool ShouldShowAd() => Application.internetReachability != NetworkReachability.NotReachable && Initialized;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }


    void Start()
    {
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            Initialized = true;
            // This callback is called once the MobileAds SDK is initialized.
        });

        if (IsTesting)
        {
#if UNITY_ANDROID
            Interstitial.adId = "ca-app-pub-3940256099942544/1033173712";
            Rewarded.adId = "ca-app-pub-3940256099942544/5224354917";
            Banner.Id = "ca-app-pub-3212738706492790/6113697308";
            BigBanner.adId = "ca-app-pub-3940256099942544/6300978111";
            AppOpen.adId = "ca-app-pub-3940256099942544/9257395921";
#elif UNITY_IPHONE
            Interstitial. adId = "ca-app-pub-3940256099942544/4411468910";
            Rewarded.adId= "ca-app-pub-3940256099942544/1712485313";
            Banner.Id = "ca-app-pub-3212738706492790/5381898163";
            BigBannger.adId = "ca-app-pub-3940256099942544/2934735716";
            AppOpen.adId = "ca-app-pub-3940256099942544/5575463023";
#else
            Interstitial. adId = "unused";
            Rewarded.adId="unused";
            Banner.Id="unused";
            BigBanner.adId = "unused";
            AppOpen.adId =  "unused";
#endif
        }
        else
        {
#if UNITY_ANDROID
            Interstitial.adId = AndroidAdIds.InterstitialId;
            Rewarded.adId = AndroidAdIds.RewardedAdId;
            Banner.Id = AndroidAdIds.BannerId;
            BigBanner.adId = AndroidAdIds.BigBannerId;
            AppOpen.adId = AndroidAdIds.AppOpenId;
#elif UNITY_IPHONE
            Interstitial.adId = IosAdIds.InterstitialId;
            Rewarded.adId = IosAdIds.RewardedAdId;
            Banner.Id = IosAdIds.BannerId;
            BigBanner.adId = IosAdIds.BigBannerId;
            AppOpen.adId = IosAdIds.AppOpenId;
#else
            Interstitial.adId = "unused";
            Rewarded.adId = "unused";
            Banner.Id = "unused";
            BigBanner.adId = "unused";
            AppOpen.adId = "unused";
#endif
        }


    }


    public void ShowInterstitialAd()
    {
        if (ShouldShowAd())
        {
            if (Interstitial._interstitialAd != null && Interstitial._interstitialAd.CanShowAd())
            {
                StartCoroutine(InvokeFunctionAfterTime(() => Interstitial.ShowInterstitialAd(OnAdmobContentFailed, OnAdmobContentClosed), AdTimers.InterstitialLoadingTime));
            }
            else
            {
                Interstitial.LoadInterstitialAd();
                Debug.Log("Interstitial Not Available");
            }

        }

    }

    public void ShowRewardedAd(Action Reward)
    {
        if (ShouldShowAd())
        {
            if (Rewarded._rewardedAd != null && Rewarded._rewardedAd.CanShowAd())
            {
                StartCoroutine(InvokeFunctionAfterTime(() => Rewarded.ShowRewardedAd(Reward, OnAdmobContentFailed, OnAdmobContentClosed), AdTimers.RewardedLoadingTime));
            }
            else
            {
                Rewarded.LoadRewardedAd();
                Debug.Log("Rewarded Not Available");
            }

        }
        else
        {
            Debug.Log("Rewarded Not Available");
        }
    }


    public void ShowBannerAd()
    {
        if (ShouldShowAd())
        {
            Banner.ShowBanner();
        }
    }

    public void HideBanner()
    {
        Banner.HideBanner();
    }
    public void ShowBigBannerAd()
    {
        if (ShouldShowAd())
        {
            BigBanner.ShowBigBanner();
        }
    }

    public void HideBigBanner()
    {
        BigBanner.HideBigBanner();
    }

    #region Admob Ads
    private void OnAdmobContentFailed()
    {
        LoadingPanel.SetActive(false);
        StartCoroutine(OtherAdRunningCheck());
    }
    private void OnAdmobContentClosed()
    {
        LoadingPanel.SetActive(false);
        StartCoroutine(OtherAdRunningCheck());
    }

    #endregion

    IEnumerator InvokeFunctionAfterTime(Action method, float time)
    {
        Timer = time;
        LoadingText.text = time.ToString();
        LoadingPanel.SetActive(true);
        while (Timer > 0)
        {
            yield return new WaitForSecondsRealtime(1);
            Timer--;
            LoadingText.text = Timer.ToString();
        }
        method.Invoke();
        OtherAdRunning = true;

    }
    IEnumerator OtherAdRunningCheck()
    {
        OtherAdRunning = true;
        yield return new WaitForSecondsRealtime(2);
        OtherAdRunning = false;

    }


    public void TestReward()
    {
        print("You Got Reward");
    }
    public void ShowTestReward()
    {
        ShowRewardedAd(TestReward);
    }

    [System.Serializable]
    public struct Times
    {
        public float InterstitialLoadingTime;
        public float RewardedLoadingTime;
    }
    [System.Serializable]
    public struct Ids
    {
        public string BannerId;
        public string InterstitialId;
        public string RewardedAdId;
        public string BigBannerId;
        public string AppOpenId;
    }
}
