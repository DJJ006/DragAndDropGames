using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class RewardedAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string _androidAdUnitId = "Rewarded_Android";
    string _adUnitId;

    [SerializeField] Button _rewardedAdButton;
    public FlyingObjectManager flayingObjectManager;

    public event Action OnUserRewarded;
    public event Action OnRewardedAdReady;

    // Track whether an ad is loaded and ready so SetButton can enable the UI when it's assigned.
    public bool isReady = false;

    // Track banner visibility to restore after showing rewarded ad
    private bool _bannerWasVisible = false;

    private void Awake()
    {
        _adUnitId = _androidAdUnitId ?? string.Empty;

        if (flayingObjectManager == null)
            flayingObjectManager = FindFirstObjectByType<FlyingObjectManager>();
    }

    public void LoadAd()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.LogWarning("Tried to load rewarded ad before Unity ads was initialized.");
            return;
        }

        Debug.Log("Loading rewarded ad.");
        Advertisement.Load(_adUnitId, this);
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log("Rewarded ad loaded!");

        // Use null-safe string comparison and guard access to the button to avoid NREs.
        if (string.Equals(placementId, _adUnitId, StringComparison.Ordinal))
        {
            isReady = true;
            if (_rewardedAdButton != null)
            {
                _rewardedAdButton.interactable = true;
            }
            else
            {
                Debug.Log("Rewarded ad loaded but _rewardedAdButton is not assigned yet. The button will be enabled when SetButton(...) is called.");
            }

            OnRewardedAdReady?.Invoke();
        }
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogWarning("Failed to load rewarded ad!");
        StartCoroutine(WaitAndLoad(5f));
    }

    public IEnumerator WaitAndLoad(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadAd();
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogWarning("Failed to show rewarded ad!");
        StartCoroutine(WaitAndLoad(5f));

        // Restore banner if it was visible before the attempt
        if (_bannerWasVisible && BannerAd.Instance != null)
        {
            BannerAd.Instance.ForceShowBanner();
            _bannerWasVisible = false;
        }
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Time.timeScale = 0f;
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("User clicked on rewarded ad");
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        // Only reward when ad was fully watched
        if (string.Equals(placementId, _adUnitId, StringComparison.Ordinal) && showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            Debug.Log("Rewarded ad completed!");

            // Reward listeners (e.g., Hanoi game)
            OnUserRewarded?.Invoke();

            // Existing behavior for flying objects if present
            if (flayingObjectManager != null)
                flayingObjectManager.DestroyAllFlyingObjects();

            if (_rewardedAdButton != null)
            {
                _rewardedAdButton.interactable = false;
            }
            else
            {
                Debug.Log("RewardedAds: _rewardedAdButton is null when trying to disable interactable after completion.");
            }

            // mark not ready and reload after delay
            isReady = false;
            StartCoroutine(WaitAndLoad(10f));
        }
        else
        {
            Debug.Log("Rewarded ad skipped or not completed.");
        }

        // Restore banner if it was visible before showing the rewarded ad
        if (_bannerWasVisible && BannerAd.Instance != null)
        {
            BannerAd.Instance.ForceShowBanner();
            _bannerWasVisible = false;
        }

        Time.timeScale = 1f;
    }

    public void SetButton(Button button)
    {
        if (button == null)
        {
            return;
        }
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ShowAd);
        _rewardedAdButton = button;
        // Ensure the button reflects current readiness immediately when assigned
        _rewardedAdButton.interactable = isReady;
    }

    public void ShowAd()
    {
        if (!isReady)
        {
            Debug.LogWarning("Rewarded ad is not ready to show. Loading ad...");
            LoadAd();
            return;
        }

        // Hide banner ad just before showing the rewarded ad
        if (BannerAd.Instance != null)
        {
            _bannerWasVisible = BannerAd.Instance.isBannerVisible;
            if (_bannerWasVisible)
                BannerAd.Instance.HideBannerAd();
        }

        if (_rewardedAdButton != null)
            _rewardedAdButton.interactable = false;
        else
            Debug.LogWarning("RewardedAds: ShowAd called but _rewardedAdButton is null.");

        // mark not ready while showing
        isReady = false;
        Advertisement.Show(_adUnitId, this);
    }
}