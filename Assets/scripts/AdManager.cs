using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdManager : MonoBehaviour
{
    public AdsInitialize adsInitializer;
    public InterstitialAd interstitialAd;
    [SerializeField] bool turnOffInterstitialAd = false;
    private bool firstAdShown = false;

    public RewardedAds rewardedAds;
    [SerializeField] bool turnOffRewardedAds = false;

    public BannerAd bannerAd;
    [SerializeField] bool turnOffBannerAd = false;

    public static AdManager Instance { get; private set; }


    private void Awake()
    {
        if (adsInitializer == null)
            adsInitializer = FindFirstObjectByType<AdsInitialize>();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        // Ensure we have a persistent InterstitialAd instance so interstitials can be shown
        // when entering any scene (not only when scene contains an InterstitialAd object).
        if (interstitialAd == null)
        {
            interstitialAd = GetComponent<InterstitialAd>();
            if (interstitialAd == null)
            {
                interstitialAd = gameObject.AddComponent<InterstitialAd>();
            }
        }

        adsInitializer.OnAdsInitialized += HandleAdsInitialized;
    }

    private void HandleAdsInitialized()
    {
        if (!turnOffInterstitialAd && interstitialAd != null)
        {
            interstitialAd.OnInterstitialAdReady += HandleInterstitialReady;
            interstitialAd.LoadAd();
        }

        if (!turnOffRewardedAds)
        {
            if (rewardedAds == null)
                rewardedAds = FindFirstObjectByType<RewardedAds>();

            if (rewardedAds != null)
                rewardedAds.LoadAd();
        }

        if (!turnOffBannerAd)
        {
            if (bannerAd == null)
                bannerAd = FindFirstObjectByType<BannerAd>();

            if (bannerAd != null)
                bannerAd.LoadBanner();
        }
    }

    private void HandleInterstitialReady()
    {
        if (!firstAdShown)
        {
            Debug.Log("Showing first time interstitial ad automatically!");
            interstitialAd.ShowAd();
            firstAdShown = true;

        }
        else
        {
            Debug.Log("Next interstitial ad is ready for manual show!");
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Do NOT overwrite the persistent interstitialAd with a scene-local one.
        // If we don't have any persistent interstitial (rare), use the scene-local as fallback.
        InterstitialAd sceneInterstitial = FindFirstObjectByType<InterstitialAd>();
        if (interstitialAd == null && sceneInterstitial != null)
        {
            interstitialAd = sceneInterstitial;
        }

        GameObject interstitialObj = GameObject.FindGameObjectWithTag("InterstitialAdButton");
        Button interstitialButton = interstitialObj != null ? interstitialObj.GetComponent<Button>() : null;

        if (interstitialAd != null && interstitialButton != null)
        {
            // Attach scene button to the persistent interstitial ad so it can be used across scenes
            interstitialAd.SetButton(interstitialButton);
        }


        if (rewardedAds == null)
            rewardedAds = FindFirstObjectByType<RewardedAds>();

        if (bannerAd == null)
            bannerAd = FindFirstObjectByType<BannerAd>();

        GameObject rewardedObj = GameObject.FindGameObjectWithTag("RewardedAdButton");
        Button rewardedAdButton = rewardedObj != null ? rewardedObj.GetComponent<Button>() : null;

        if (rewardedAds != null && rewardedAdButton != null)
        {
            rewardedAds.SetButton(rewardedAdButton);
            // Ensure the rewarded ad begins loading after the button is set so the button becomes interactable
            rewardedAds.LoadAd();
        }


        GameObject bannerObj = GameObject.FindGameObjectWithTag("BannerAdButton");
        Button bannerButton = bannerObj != null ? bannerObj.GetComponent<Button>() : null;
        if (bannerAd != null && bannerButton != null)
        {
            bannerAd.SetButton(bannerButton);
        }

        Debug.Log("Scene loaded!");

        // Attempt to show an interstitial on every scene change.
        if (!turnOffInterstitialAd && interstitialAd != null)
        {
            // Use the persistent interstitial to show the ad.
            interstitialAd.ShowInterstitial();

            // If not ready, attach a one-time handler to show when it becomes ready
            if (!interstitialAd.isReady)
            {
                void OnReadyHandler()
                {
                    interstitialAd.OnInterstitialAdReady -= OnReadyHandler;
                    if (interstitialAd != null && interstitialAd.isReady)
                    {
                        interstitialAd.ShowAd();
                    }
                }

                interstitialAd.OnInterstitialAdReady += OnReadyHandler;

                // Ensure a load is requested in case ShowInterstitial only attempted direct show
                interstitialAd.LoadAd();
            }
        }
    }
}