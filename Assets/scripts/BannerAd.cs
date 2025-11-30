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

    // Track whether we have a loaded banner (OnBannerLoaded sets this).
    private bool _bannerLoaded = false;

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
        _bannerLoaded = true;

        if (_bannerButton != null)
            _bannerButton.interactable = true;

        // Automatically show banner when loaded
        ForceShowBanner();
    }

    void OnBannerError(string message)
    {
        Debug.LogWarning("Banner Error: " + message);
        _bannerLoaded = false;
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
        // If not loaded yet, start load and wait; otherwise show immediately.
        if (!_bannerLoaded)
        {
            Debug.Log("ForceShowBanner: banner not loaded yet, calling LoadBanner and waiting to show.");
            LoadBanner();
            StartCoroutine(ForceShowWhenLoaded());
            return;
        }

        BannerOptions options = new BannerOptions
        {
            clickCallback = OnBannerClicked,
            hideCallback = OnBannerHidden,
            showCallback = OnBannerShown
        };

        Advertisement.Banner.Show(_adUnitId, options);
    }

    private IEnumerator ForceShowWhenLoaded()
    {
        // Wait until banner reports loaded (OnBannerLoaded sets _bannerLoaded).
        float timeout = 8f;
        float elapsed = 0f;
        while (!_bannerLoaded && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.2f);
            elapsed += 0.2f;
        }

        if (_bannerLoaded)
        {
            Debug.Log("ForceShowWhenLoaded: banner loaded, showing now.");
            ForceShowBanner();
        }
        else
        {
            Debug.LogWarning("ForceShowWhenLoaded: banner failed to load within timeout.");
        }
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

    // Public helper to ensure the banner is visible (load & show if necessary).
    public void EnsureBannerShown()
    {
        if (isBannerVisible)
            return;

        if (!Advertisement.isInitialized)
        {
            Debug.Log("EnsureBannerShown: Ads not initialized yet; banner will be shown after init.");
            return;
        }

        if (_bannerLoaded)
        {
            ForceShowBanner();
        }
        else
        {
            LoadBanner();
            StartCoroutine(ForceShowWhenLoaded());
        }
    }
}