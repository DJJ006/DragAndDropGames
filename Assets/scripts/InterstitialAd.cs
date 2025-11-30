using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class InterstitialAd : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string _androidAdUnitId = "Interstitial_Android";
    string _adUnitId;

    public event Action OnInterstitialAdReady;
    public bool isReady = false;
    [SerializeField] Button _interstitialAdButton;

    // Track whether a banner was visible before showing the interstitial so we can restore it.
    private bool _bannerWasVisible = false;

    void Awake()
    {
        _adUnitId = _androidAdUnitId;
    }

    private void Update()
    {
        // Only update the button if it exists; avoid depending on AdManager instance here.
        if (_interstitialAdButton != null)
        {
            _interstitialAdButton.interactable = isReady;
        }
    }

    public void OnInterstitialAdButtonClicked()
    {
        Debug.Log("Interstitial ad button clicked!");
        ShowInterstitial();
    }

    public void LoadAd()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.LogWarning("Tried to load interstitial ad before Unity ads was initialized!");
            return;
        }

        Debug.Log("Loading interstitial ad");
        Advertisement.Load(_adUnitId, this);
    }

    public void ShowAd()
    {
        if (isReady)
        {
            // Hide banner ad on interstitial ad show...
            if (BannerAd.Instance != null)
            {
                _bannerWasVisible = BannerAd.Instance.isBannerVisible;
                if (_bannerWasVisible)
                    BannerAd.Instance.HideBannerAd();
            }

            Advertisement.Show(_adUnitId, this);
            isReady = false;

        }
        else
        {
            Debug.LogWarning("Interstitial ad is not ready yet!");
            LoadAd();
        }
    }

    public void ShowInterstitial()
    {
        if (AdManager.Instance != null && AdManager.Instance.interstitialAd != null && isReady)
        {
            Debug.Log("Showing interstitial ad manually!");
            ShowAd();
        }
        else
        {
            Debug.Log("Interstitial ad not ready yet, loading again!");
            LoadAd();
        }
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log("Interstitial ad loaded!");
        isReady = true;
        if (_interstitialAdButton != null)
            _interstitialAdButton.interactable = true;
        OnInterstitialAdReady?.Invoke();
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogWarning($"Failed to load interstitial ad: {error} - {message}");
        // Retry with a short delay to avoid immediate recursion / spamming loads
        StartCoroutine(RetryLoadWithDelay(5f));
    }

    private IEnumerator RetryLoadWithDelay(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        if (Advertisement.isInitialized)
        {
            Debug.Log("Retrying interstitial ad load...");
            Advertisement.Load(_adUnitId, this);
        }
        else
        {
            Debug.LogWarning("Advertisement not initialized yet; skipping retry.");
        }
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("User clicked on interstitial ad!");
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            Debug.Log("Interstitial ad watched completely!");
            StartCoroutine(SlowDownTimeTemporarily(30f));
            LoadAd();
        }
        else
        {
            Debug.Log("Interstitial ad skipped or status ir unknown!");
            LoadAd();

            // Restore time scale when ad is skipped or not completed.
            if (Time.timeScale == 0f)
            {
                Time.timeScale = 1.0f;
                Debug.Log("Time restored to normal after skipped/unknown interstitial.");
            }
        }

        // Restore banner if it was visible before the interstitial
        if (_bannerWasVisible && BannerAd.Instance != null)
        {
            BannerAd.Instance.ForceShowBanner();
            _bannerWasVisible = false;
        }
    }

    private IEnumerator SlowDownTimeTemporarily(float seconds)
    {
        // Use realtime wait so the coroutine progresses even if timeScale is 0.
        Time.timeScale = 0.4f;
        Debug.Log("Time slowed down to 0.4x for " + seconds + " sec (real-time)");
        yield return new WaitForSecondsRealtime(seconds);

        Time.timeScale = 1.0f;
        Debug.Log("Time restored to normal!");
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.Log("Error showing interstitial ad!");
        StartCoroutine(RetryLoadWithDelay(5f));

        // Ensure time is restored if showing the ad failed while time was frozen.
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1.0f;
            Debug.Log("Time restored to normal after ad show failure.");
        }

        // Restore banner if it was visible before the attempt
        if (_bannerWasVisible && BannerAd.Instance != null)
        {
            BannerAd.Instance.ForceShowBanner();
            _bannerWasVisible = false;
        }
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Debug.Log("Showing interstitial ad at this moment!");
        Time.timeScale = 0f;
    }

    public void SetButton(Button button)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnInterstitialAdButtonClicked);
        _interstitialAdButton = button;
        _interstitialAdButton.interactable = false;
    }
}