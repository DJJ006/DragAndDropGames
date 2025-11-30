using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public TextMeshProUGUI timeText;  
    private float elapsedTime = 0f;
    private bool isRunning = false;

    void Awake()
    {
        // Try to auto-assign a TextMeshProUGUI if it wasn't set in the inspector.
        if (timeText == null)
        {
            timeText = GetComponent<TextMeshProUGUI>()
                       ?? GetComponentInChildren<TextMeshProUGUI>()
                       ?? FindObjectOfType<TextMeshProUGUI>();

            if (timeText == null)
            {
                Debug.LogWarning("GameTimer: timeText not assigned. Assign a TextMeshProUGUI in the Inspector or add one to this GameObject (or a child).");
            }
        }
    }

    void Start()
    {
        // Start timer when the game scene loads
        elapsedTime = 0f;
        isRunning = true;
    }

    void Update()
    {
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;

            int hours = Mathf.FloorToInt(elapsedTime / 3600f);
            int minutes = Mathf.FloorToInt((elapsedTime % 3600f) / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);

            // Guard against null before writing to the UI to avoid NRE.
            if (timeText != null)
            {
                timeText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
            }
        }
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
    }
}
