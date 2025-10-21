using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Spēles objekti")]
    public GameObject[] Vehicles;        // Visas transportlīdzekļu GameObject
    public TextMeshProUGUI TimerText;    // Timer TextMeshPro (parādās tikai beigu logā)
    public GameObject WinPanel;          // Win Panel
    public GameObject LosePanel;         // Lose Panel
    public GameObject[] Stars;           // Zvaigznes (3)
    public RectTransform[] DropZones;

    private float elapsedTime = 0f;
    private bool timerRunning = true;
    private bool gameOver = false;
    private int vehiclesPlaced = 0;

    // novērš dubultu pieskaitīšanu: vienu transportlīdzekli skaita tikai reizi
    private HashSet<GameObject> placedSet = new HashSet<GameObject>();

    void Start()
    {
        // Paslēpj Win/Lose panelus un zvaigznes
        WinPanel.SetActive(false);
        LosePanel.SetActive(false);
        if (TimerText != null) TimerText.gameObject.SetActive(false);

        if (Stars != null)
        {
            foreach (GameObject star in Stars)
                if (star != null) star.SetActive(false);
        }

        // Nodrošina, ka spēle ir "normālā" laika režīmā
        Time.timeScale = 1f;
        timerRunning = true;
        gameOver = false;
        elapsedTime = 0f;
        vehiclesPlaced = 0;
        placedSet.Clear();

        // Random spawn sākumā (ja tev ir šī funkcija)
        RandomizeDropZones();
        RandomSpawnVehicles();

        // Atjauno start pozīcijas ObjectScript'ā, lai tās kļūst par jaunajām "pareizajām"
        ObjectScript objScript = FindObjectOfType<ObjectScript>();
        if (objScript != null)
        {
            for (int i = 0; i < Vehicles.Length; i++)
            {
                if (Vehicles[i] != null)
                    objScript.startCoordinates[i] = Vehicles[i].GetComponent<RectTransform>().localPosition;
            }
        }

    }

    void Update()
    {
        if (timerRunning && !gameOver)
        {
            elapsedTime += Time.deltaTime;

            // Ja laiks sasniedz 3 minūtes -> automātiska zaudēšana
            if (elapsedTime >= 180f)
            {
                LoseGame();
            }
        }
    }

    // Saukt no DropPlaceScript, kad transportlīdzeklis pareizi novietots
    public void SetVehiclePlaced(GameObject vehicle)
    {
        if (gameOver || vehicle == null) return;

        // nepieļauj dubultu pieskaitīšanu vienam vehicle
        if (placedSet.Contains(vehicle)) return;

        placedSet.Add(vehicle);
        vehiclesPlaced++;

        // Ja visi novietoti — uzvara (zvaigznes pēc laika)
        if (vehiclesPlaced >= Vehicles.Length)
        {
            WinGame();
        }
    }

    // Ja kāda mašīna tiek iznīcināta -> tūlītējs zaudējums
    public void OnVehicleDestroyed(GameObject vehicle)
    {
        if (gameOver) return;

        // tūlītējs lose
        LoseGame();
    }

    private void WinGame()
    {
        if (gameOver) return;

        gameOver = true;
        timerRunning = false;

        // parāda taimeri (formāts hh:mm:ss)
        if (TimerText != null)
        {
            TimerText.gameObject.SetActive(true);
            UpdateTimerUI(true); // true -> parādīt hh:mm:ss
        }

        // parāda Win paneli
        if (WinPanel != null)
        {
            WinPanel.SetActive(true);
            WinPanel.transform.SetAsLastSibling();

            TimerText.gameObject.SetActive(true);
            TimerText.transform.SetAsLastSibling();
        }

        // nodrošina, ka mašīnas ir zem paneliem (ja UI hierarchy tiek izmantots)
        foreach (GameObject vehicle in Vehicles)
        {
            if (vehicle != null)
                vehicle.transform.SetAsFirstSibling();
        }

        // aptur visu (fiziku/animācijas) un kameru
        Time.timeScale = 0f;
        CameraScript camScript = FindObjectOfType<CameraScript>();
        if (camScript != null) camScript.SetCameraActive(false);

        // aprēķina zvaigznes TIKAI pēc laika
        int starCount = CalculateStarsByTime(elapsedTime);

        // aktivizē zvaigznes
        if (Stars != null)
        {
            for (int i = 0; i < Stars.Length; i++)
            {
                if (Stars[i] != null)
                    Stars[i].SetActive(i < starCount);
            }
        }
    }

    private void LoseGame()
    {
        if (gameOver) return;

        gameOver = true;
        timerRunning = false;

        if (TimerText != null)
        {
            TimerText.gameObject.SetActive(true);
            UpdateTimerUI(true);
        }

        if (LosePanel != null)
        {
            LosePanel.SetActive(true);
            LosePanel.transform.SetAsLastSibling();

            TimerText.gameObject.SetActive(true);
            TimerText.transform.SetAsLastSibling();
        }

        // aptur spēli un kameru
        Time.timeScale = 0f;
        CameraScript camScript = FindObjectOfType<CameraScript>();
        if (camScript != null) camScript.SetCameraActive(false);

        // par lose nieko zvaigznes neraida (varbūt tu gribi null)
        if (Stars != null)
        {
            foreach (GameObject star in Stars)
                if (star != null) star.SetActive(false);
        }
    }

    // Atjaunina laika tekstu; ja showHours true -> hh:mm:ss, citādi mm:ss
    private void UpdateTimerUI(bool showHours = false)
    {
        int hours = Mathf.FloorToInt(elapsedTime / 3600f);
        int minutes = Mathf.FloorToInt((elapsedTime % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);

        if (TimerText == null) return;

        if (showHours)
            TimerText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        else
            TimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Aprēķina zvaigznes tikai pēc laika (win-only)
    private int CalculateStarsByTime(float time)
    {
        if (time <= 60f) return 3;
        else if (time <= 120f) return 2;
        else return 1; // laiks > 120 un < 180 (ja >=180, lose jau notika)
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("BeginScene");
    }

    // -------------------
    // Random spawn funkcija (World Space)
    // (ievieto savu esošo implementāciju — to mēs atstājām kā ir)
    // -------------------
    private void RandomSpawnVehicles()
    {
        List<Rect> existingRects = new List<Rect>();

        foreach (GameObject vehicle in Vehicles)
        {
            if (vehicle == null) continue;

            RectTransform vehicleRect = vehicle.GetComponent<RectTransform>();
            Vector2 randomPos = Vector2.zero;
            bool validPosition = false;

            int maxAttempts = 200; // palielināts mēģinājumu skaits
            int attempts = 0;

            while (!validPosition && attempts < maxAttempts)
            {
                attempts++;

                float randomX = Random.Range(-650f, 650f);
                float randomY = Random.Range(-450f, 450f);
                randomPos = new Vector2(randomX, randomY);

                Rect vehicleWorldRect = new Rect(
                    randomPos.x - vehicleRect.rect.width / 2f,
                    randomPos.y - vehicleRect.rect.height / 2f,
                    vehicleRect.rect.width,
                    vehicleRect.rect.height
                );

                if (!IsInsideAnyDropZoneWorld(vehicleWorldRect) && !IsOverlappingOtherVehicles(vehicleWorldRect, existingRects))
                {
                    vehicleRect.localPosition = randomPos; // ✅ mainīts no position uz localPosition

                    float randomScale = Random.Range(0.8f, 1.2f);
                    vehicleRect.localScale = new Vector3(randomScale, randomScale, 1f);

                    float randomRotation = Random.Range(-10f, 10f);
                    vehicleRect.localRotation = Quaternion.Euler(0f, 0f, randomRotation);

                    existingRects.Add(vehicleWorldRect);
                    validPosition = true;
                }

                Debug.Log($"{vehicle.name} spawn at {vehicleRect.localPosition}");


            }

            // Ja pēc maxAttempts nav atrasta derīga pozīcija → piespiež spawnēt pēdējā random pozīcijā
            if (!validPosition)
            {
                vehicleRect.position = randomPos;
                existingRects.Add(new Rect(
                    randomPos.x - vehicleRect.rect.width / 2f,
                    randomPos.y - vehicleRect.rect.height / 2f,
                    vehicleRect.rect.width,
                    vehicleRect.rect.height
                ));
            }

            vehicle.transform.SetAsLastSibling();
        }
    }


    private bool IsInsideAnyDropZoneWorld(Rect vehicleRect)
    {
        if (DropZones == null || DropZones.Length == 0) return false;

        foreach (RectTransform zone in DropZones)
        {
            if (zone == null) continue;

            Rect zoneWorldRect = new Rect(
                zone.position.x + zone.rect.xMin,
                zone.position.y + zone.rect.yMin,
                zone.rect.width,
                zone.rect.height
            );

            if (zoneWorldRect.Overlaps(vehicleRect))
                return true;
        }
        return false;
    }

    private bool IsOverlappingOtherVehicles(Rect vehicleRect, List<Rect> existingRects)
    {
        foreach (Rect rect in existingRects)
        {
            if (rect.Overlaps(vehicleRect))
                return true;
        }
        return false;
    }



    // Pievieno pie GameManager.cs
    private void RandomizeDropZones()
    {
        List<Rect> existingRects = new List<Rect>();

        foreach (RectTransform zone in DropZones)
        {
            Vector2 randomPos = Vector2.zero;
            bool validPosition = false;
            int maxAttempts = 100;
            int attempts = 0;

            while (!validPosition && attempts < maxAttempts)
            {
                attempts++;
                float randomX = Random.Range(-650f, 650f); // pielāgo pēc scēnas
                float randomY = Random.Range(-450f, 450f);
                randomPos = new Vector2(randomX, randomY);

                Rect newRect = new Rect(
                    randomPos.x - zone.rect.width / 2,
                    randomPos.y - zone.rect.height / 2,
                    zone.rect.width,
                    zone.rect.height
                );

                // Pārbauda, vai nepārklājas ar jau izvietotām DropZones
                bool overlaps = false;
                foreach (Rect r in existingRects)
                {
                    if (r.Overlaps(newRect))
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps)
                {
                    zone.position = randomPos;
                    existingRects.Add(newRect);
                    validPosition = true;
                }
            }
        }
    }


}