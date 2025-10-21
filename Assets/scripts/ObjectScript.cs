using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectScript : MonoBehaviour
{
    public GameObject[] vehicles;
    [HideInInspector]
    public Vector2[] startCoordinates;
    public Canvas can;
    public AudioSource effects;
    public AudioClip[] audioCli;
    [HideInInspector]
    public bool rightPlace = false;
    public static GameObject lastDragged = null;
    public static bool drag = false;

    void Awake()
    {
        // ✅ Pārliecināmies, ka masīvs ir pareizā garuma un nav null
        if (startCoordinates == null || startCoordinates.Length != vehicles.Length)
            startCoordinates = new Vector2[vehicles.Length];

        // ✅ Šeit tikai saglabā sākotnējās pozīcijas, ja tās vēl nav pārrakstītas GameManager'ā
        for (int i = 0; i < vehicles.Length; i++)
        {
            if (vehicles[i] != null)
                startCoordinates[i] = vehicles[i].GetComponent<RectTransform>().localPosition;

            Debug.Log($"Saved start pos for {vehicles[i].name}: {startCoordinates[i]}");
        }

    }
}
