using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class HanoiUIManager : MonoBehaviour
{
    public static HanoiUIManager Instance { get; private set; }

    public TextMeshProUGUI MovesText;
    public TextMeshProUGUI TimerText;
    public GameObject WinPanel;
    public Button RestartButton;
    public Button UndoButton; // optional: not implemented in manager

    private int moves;
    private float elapsed;
    private bool running;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    void Start()
    {
        ResetUI();
        if (RestartButton != null)
            RestartButton.onClick.AddListener(OnRestartClicked);
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
        if (WinPanel != null) WinPanel.SetActive(false);
    }

    public void OnMoveMade()
    {
        moves++;
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
        if (WinPanel != null) WinPanel.SetActive(true);
    }

    void OnRestartClicked()
    {
        HanoiGameManager gm = FindObjectOfType<HanoiGameManager>();
        gm?.Restart();
    }
}