using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardedAdmob : MonoBehaviour
{

    [HideInInspector]
    public string adId;

    public Action OnContentFailed;
    public Action OnContentClosed;
    [HideInInspector]
    public RewardedAd _rewardedAd;


    IEnumerator Start()
    {
        yield return new WaitUntil(() => AdsManager.Instance.Initialized);
        Invoke(nameof(LoadRewardedAd),2f);
    }

    public void LoadRewardedAd()
    {
        if(Application.internetReachability==NetworkReachability.NotReachable)
        {
            return;
        }
        // Clean up the old ad before loading a new one.
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        Debug.Log("Loading the rewarded ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        RewardedAd.Load(adId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : "
                          + ad.GetResponseInfo());

                _rewardedAd = ad;
                RegisterReloadHandler(_rewardedAd);

            });
    }

    public void ShowRewardedAd(Action Reward, Action onContentFailed = null, Action onContentClosed = null)
    {
        _rewardedAd.Show((Reward reward) =>
        {
            // TODO: Reward the user.
            Reward.Invoke();
        });
        OnContentFailed = onContentFailed;
        OnContentClosed = onContentClosed;

    }

    private void RegisterReloadHandler(RewardedAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            LoadRewardedAd();
            OnContentClosed.Invoke();
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            LoadRewardedAd();
            OnContentFailed.Invoke();
        };
    }



}
