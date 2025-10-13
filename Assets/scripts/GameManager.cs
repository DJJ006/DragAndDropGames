using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Spēles objekti")]
    public GameObject[] Vehicles;        // Visas transportlīdzekļu GameObject
    public TextMeshProUGUI TimerText;    // Timer TextMeshPro
    public GameObject WinPanel;          // Win Panel
    public GameObject LosePanel;         // Lose Panel
    public GameObject[] Stars;           // Zvaigznes (3)

    private float elapsedTime = 0f;
    private bool timerRunning = true;
    private bool gameOver = false;
    private int vehiclesPlaced = 0;

    void Start()
    {
        WinPanel.SetActive(false);
        LosePanel.SetActive(false);
        foreach (GameObject star in Stars)
            star.SetActive(false);
    }

    void Update()
    {
        if (timerRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    private void UpdateTimerUI()
    {
        int hours = Mathf.FloorToInt(elapsedTime / 3600f);
        int minutes = Mathf.FloorToInt((elapsedTime % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        TimerText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    // Transportlīdzeklis novietots pareizi
    public void SetVehiclePlaced(GameObject vehicle)
    {
        if (gameOver) return;

        vehiclesPlaced++;
        CheckWinCondition();
    }

    // Transportlīdzeklis iznīcināts
    public void OnVehicleDestroyed(GameObject vehicle)
    {
        if (gameOver) return;

        // Pārbauda, vai iznīcinātais objekts ir mūsu sarakstā
        foreach (GameObject v in Vehicles)
        {
            if (v == vehicle)
            {
                GameOver();
                break;
            }
        }
    }

    private void CheckWinCondition()
    {
        if (vehiclesPlaced >= Vehicles.Length)
        {
            WinGame();
        }
    }

    private void GameOver()
    {
        gameOver = true;
        timerRunning = false;
        LosePanel.SetActive(true);
    }

    private void WinGame()
    {
        gameOver = true;
        timerRunning = false;
        WinPanel.SetActive(true);

        int starCount = CalculateStars(elapsedTime);
        for (int i = 0; i < starCount; i++)
            Stars[i].SetActive(true);
    }

    private int CalculateStars(float time)
    {
        if (time <= 60f) return 3;
        else if (time <= 90f) return 2;
        else if (time <= 120f) return 1;
        else return 0;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
