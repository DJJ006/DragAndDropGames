using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class HanoiUIManager : MonoBehaviour
{
    public static HanoiUIManager Instance { get; private set; }

    public TextMeshProUGUI MovesText;
    public TextMeshProUGUI TimerText;

    // Win panel fields
    public TextMeshProUGUI WinTimeText; // shows final time on win panel
    public TextMeshProUGUI WinMovesText; // shows final moves on win panel
    public GameObject WinPanel;

    public Button RestartButton;
    public Button MainMenuButton; // button on win panel to go to main menu
    public Button UndoButton; // optional: not implemented in manager

    private int moves;
    private float elapsed;
    private bool running;

    // reference to rewarded ads to subscribe/unsubscribe
    private RewardedAds _rewardedAds;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;

        // Enforce landscape-only mode at runtime
        EnforceLandscapeMode();
    }

    void Start()
    {
        ResetUI();
        if (RestartButton != null)
            RestartButton.onClick.AddListener(OnRestartClicked);
        if (MainMenuButton != null)
            MainMenuButton.onClick.AddListener(OnMainMenuClicked);

        // Subscribe to rewarded ad event if available
        _rewardedAds = FindObjectOfType<RewardedAds>();
        if (_rewardedAds != null)
        {
            _rewardedAds.OnUserRewarded += HandleUserRewarded;
        }
    }

    void OnDestroy()
    {
        if (_rewardedAds != null)
        {
            _rewardedAds.OnUserRewarded -= HandleUserRewarded;
        }
    }

    void Update()
    {
        if (!running) return;
        elapsed += Time.deltaTime;
        UpdateTimerText();
    }

    public void ResetUI()
    {
        moves = 0;
        elapsed = 0f;
        running = true;
        UpdateMovesText();
        UpdateTimerText();

        // hide win panel and clear its texts
        if (WinPanel != null) WinPanel.SetActive(false);
        if (WinTimeText != null) WinTimeText.text = string.Empty;
        if (WinMovesText != null) WinMovesText.text = string.Empty;
    }

    public void OnMoveMade()
    {
        moves++;
        UpdateMovesText();
    }

    // Handler invoked when user earns reward from rewarded ad
    private void HandleUserRewarded()
    {
        // Remove up to 5 moves, but not below zero
        moves = Mathf.Max(0, moves - 5);
        UpdateMovesText();
    }

    void UpdateMovesText()
    {
        if (MovesText != null)
            MovesText.text = $"Moves: {moves}";
    }

    void UpdateTimerText()
    {
        if (TimerText == null) return;
        TimeSpan t = TimeSpan.FromSeconds(elapsed);
        TimerText.text = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
    }

    public void OnWin()
    {
        running = false;

        // set final time on win panel
        if (WinTimeText != null)
        {
            TimeSpan t = TimeSpan.FromSeconds(elapsed);
            WinTimeText.text = string.Format("Time: {0:D2}:{1:D2}", t.Minutes, t.Seconds);
        }

        // set final moves on win panel
        if (WinMovesText != null)
        {
            WinMovesText.text = $"Moves: {moves}";
        }

        if (WinPanel != null) WinPanel.SetActive(true);
    }

    void OnRestartClicked()
    {
        HanoiGameManager gm = FindObjectOfType<HanoiGameManager>();
        gm?.Restart();
    }

    void OnMainMenuClicked()
    {
        // Load main menu scene. Make sure a scene named "BeginScene" exists in Build Settings.
        // If not, change the scene name as appropriate.
        SceneManager.LoadScene("BeginScene");
    }

    // Enforces landscape-only orientation at runtime.
    // - Disables portrait autorotate, enables landscape autorotate,
    //   and uses AutoRotation so device will only use landscape orientations.
    // You can also lock to a specific landscape by setting Screen.orientation = ScreenOrientation.LandscapeLeft.
    private void EnforceLandscapeMode()
    {
#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        // Use auto-rotation but only landscape is enabled above.
        Screen.orientation = ScreenOrientation.AutoRotation;

        // Optionally force a landscape orientation immediately:
        if (Screen.width < Screen.height)
        {
            // If currently portrait, force a landscape orientation switch.
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }
#endif
    }
}