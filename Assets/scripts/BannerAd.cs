using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class BannerAd : MonoBehaviour
{
    [SerializeField] string _androidAdUnitId = "Banner_Android";
    string _adUnitId;

    [SerializeField] Button _bannerButton;
    public bool isBannerVisible = false;

    [SerializeField] BannerPosition _bannerPosition = BannerPosition.BOTTOM_CENTER;

    // Singleton so banner persists across scenes and we don't create duplicates
    public static BannerAd Instance { get; private set; }

    private void Awake()
    {
        // simple singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _adUnitId = _androidAdUnitId;
        Advertisement.Banner.SetPosition(_bannerPosition);

        // Start the automatic banner lifecycle
        StartCoroutine(InitAndShowBanner());
    }

    private IEnumerator InitAndShowBanner()
    {
        // Wait until Unity Ads is initialized, then load banner
        while (!Advertisement.isInitialized)
        {
            yield return new WaitForSeconds(0.5f);
        }

        LoadBanner();
    }

    public void LoadBanner()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.Log("Tried to load banner ad before Unity ads was initialized!");
            return;
        }

        Debug.Log("Loading Banner ad!");
        BannerLoadOptions options = new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };

        Advertisement.Banner.Load(_adUnitId, options);
    }

    void OnBannerLoaded()
    {
        Debug.Log("Banner ad loaded!");
        if (_bannerButton != null)
            _bannerButton.interactable = true;

        // Automatically show banner when loaded
        ForceShowBanner();
    }

    void OnBannerError(string message)
    {
        Debug.LogWarning("Banner Error: " + message);
        // small retry delay to avoid tight loop
        StartCoroutine(RetryLoadBanner());
    }

    private IEnumerator RetryLoadBanner()
    {
        yield return new WaitForSeconds(3f);
        LoadBanner();
    }

    // Keep the existing toggle method for UI, but banner will already be shown automatically
    public void ShowBannerAd()
    {
        if (isBannerVisible)
        {
            HideBannerAd();
        }
        else
        {
            BannerOptions options = new BannerOptions
            {
                clickCallback = OnBannerClicked,
                hideCallback = OnBannerHidden,
                showCallback = OnBannerShown
            };

            Advertisement.Banner.Show(_adUnitId, options);
        }
    }

    public void HideBannerAd()
    {
        Advertisement.Banner.Hide();
        isBannerVisible = false;
    }

    // New helper to force-show the banner (used by InterstitialAd to restore it)
    public void ForceShowBanner()
    {
        BannerOptions options = new BannerOptions
        {
            clickCallback = OnBannerClicked,
            hideCallback = OnBannerHidden,
            showCallback = OnBannerShown
        };

        Advertisement.Banner.Show(_adUnitId, options);
    }

    void OnBannerClicked()
    {
        Debug.Log("User clicked on banner ad!");
    }

    void OnBannerHidden()
    {
        Debug.Log("Banner is hidden!");
        isBannerVisible = false;
    }

    void OnBannerShown()
    {
        Debug.Log("Banner ad is visible!");
        isBannerVisible = true;
    }

    public void SetButton(Button button)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ShowBannerAd);
        _bannerButton = button;
        _bannerButton.interactable = false;
    }
}