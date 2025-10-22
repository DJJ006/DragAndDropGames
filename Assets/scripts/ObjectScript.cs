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

    // New: mapping from the actual instance -> saved localPosition (Vector3 to preserve z if needed)
    private Dictionary<GameObject, Vector3> startPositions = new Dictionary<GameObject, Vector3>();

    void Awake()
    {
        // Ensure array length is correct
        if (startCoordinates == null || startCoordinates.Length != vehicles.Length)
            startCoordinates = new Vector2[vehicles.Length];

        // Save initial positions and populate dictionary
        for (int i = 0; i < vehicles.Length; i++)
        {
            if (vehicles[i] != null)
            {
                RectTransform rt = vehicles[i].GetComponent<RectTransform>();
                Vector3 localPos = rt != null ? rt.localPosition : vehicles[i].transform.localPosition;
                startCoordinates[i] = new Vector2(localPos.x, localPos.y);
                startPositions[vehicles[i]] = localPos;

                Debug.Log($"Saved start pos for {vehicles[i].name}: {startPositions[vehicles[i]]}");
            }
        }
    }

    // Call this when external code (e.g. GameManager) updates startCoordinates[] so dictionary stays in sync.
    public void UpdateStartPositionsFromArray()
    {
        if (vehicles == null || startCoordinates == null) return;

        startPositions.Clear();
        for (int i = 0; i < vehicles.Length; i++)
        {
            if (vehicles[i] == null) continue;
            Vector2 sv = startCoordinates.Length > i ? startCoordinates[i] : Vector2.zero;
            RectTransform rt = vehicles[i].GetComponent<RectTransform>();
            float z = rt != null ? rt.localPosition.z : vehicles[i].transform.localPosition.z;
            Vector3 pos3 = new Vector3(sv.x, sv.y, z);
            startPositions[vehicles[i]] = pos3;
            Debug.Log($"UpdateStartPositionsFromArray: {vehicles[i].name} => {pos3}");
        }
    }

    // Try get saved start position for an instance
    public bool TryGetStartPosition(GameObject instance, out Vector3 position)
    {
        if (startPositions != null && instance != null)
        {
            return startPositions.TryGetValue(instance, out position);
        }
        position = Vector3.zero;
        return false;
    }
}