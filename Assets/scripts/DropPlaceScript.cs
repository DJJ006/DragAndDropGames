using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropPlaceScript : MonoBehaviour, IDropHandler
{
    private float placeZRot, vehicleZRot, rotDiff;
    private Vector3 placeSiz, vehicleSiz;
    private float xSizeDiff, ySizeDiff;
    public ObjectScript objScript;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null || !Input.GetMouseButtonUp(0) ||
            Input.GetMouseButton(1) || Input.GetMouseButton(2)) return;

        RectTransform dragRect = eventData.pointerDrag.GetComponent<RectTransform>();

        // Pareizā vieta
        if (eventData.pointerDrag.tag.Equals(tag))
        {
            placeZRot = dragRect.eulerAngles.z;
            vehicleZRot = GetComponent<RectTransform>().eulerAngles.z;
            rotDiff = Mathf.Abs(placeZRot - vehicleZRot);

            placeSiz = dragRect.localScale;
            vehicleSiz = GetComponent<RectTransform>().localScale;
            xSizeDiff = Mathf.Abs(placeSiz.x - vehicleSiz.x);
            ySizeDiff = Mathf.Abs(placeSiz.y - vehicleSiz.y);

            if ((rotDiff <= 5 || (rotDiff >= 355 && rotDiff <= 360)) &&
                (xSizeDiff <= 0.1f && ySizeDiff <= 0.1f))
            {
                Debug.Log("Vehicle placed: " + eventData.pointerDrag.name);
                FindObjectOfType<GameManager>().SetVehiclePlaced(eventData.pointerDrag);

                objScript.rightPlace = true;

                // Uzliek tieši DropZone pozīcijā ar DropZone scale
                dragRect.localPosition = GetComponent<RectTransform>().localPosition;
                dragRect.localScale = GetComponent<RectTransform>().localScale;

                // Audio efekti
                PlayAudio(dragRect.tag);
            }
        }
        else // Nepareizā vieta
        {
            objScript.rightPlace = false;
            objScript.effects.PlayOneShot(objScript.audioCli[1]);

            GameObject dragged = eventData.pointerDrag;
            RectTransform draggedRect = dragged.GetComponent<RectTransform>();

            // --- NEW: use GameManager.Vehicles (single source of truth) to find the exact instance index ---
            GameManager gm = FindObjectOfType<GameManager>();
            int foundIndex = -1;
            if (gm != null && gm.Vehicles != null)
            {
                for (int i = 0; i < gm.Vehicles.Length; i++)
                {
                    if (gm.Vehicles[i] == dragged)
                    {
                        foundIndex = i;
                        break;
                    }
                }
            }

            if (foundIndex != -1 && objScript.startCoordinates != null && foundIndex < objScript.startCoordinates.Length)
            {
                // restore the same localPosition that GameManager recorded into ObjectScript.startCoordinates
                Vector2 start = objScript.startCoordinates[foundIndex];
                draggedRect.localPosition = new Vector3(start.x, start.y, draggedRect.localPosition.z);
            }
            else
            {
                // Fallback: try exact instance match in ObjectScript.vehicles (if GameManager wasn't available / arrays differ)
                bool restored = false;
                if (objScript.vehicles != null)
                {
                    for (int i = 0; i < objScript.vehicles.Length; i++)
                    {
                        if (objScript.vehicles[i] == dragged)
                        {
                            Vector2 start = objScript.startCoordinates.Length > i ? objScript.startCoordinates[i] : Vector2.zero;
                            draggedRect.localPosition = new Vector3(start.x, start.y, draggedRect.localPosition.z);
                            restored = true;
                            break;
                        }
                    }
                }

                if (!restored)
                {
                    // Last fallback: match by tag (older behavior)
                    for (int i = 0; i < objScript.vehicles.Length; i++)
                    {
                        if (objScript.vehicles[i] != null && objScript.vehicles[i].tag == dragged.tag)
                        {
                            draggedRect.localPosition = objScript.startCoordinates[i];
                            Debug.LogWarning($"DropPlaceScript: exact instance not found; restored by tag using index {i} for {dragged.name}");
                            restored = true;
                            break;
                        }
                    }
                }

                if (!restored)
                    Debug.LogWarning($"DropPlaceScript: couldn't find saved start position for {dragged.name}.");
            }
        }
    }

    private void PlayAudio(string tag)
    {
        switch (tag)
        {
            case "Garbage": objScript.effects.PlayOneShot(objScript.audioCli[2]); break;
            case "Ambulance": objScript.effects.PlayOneShot(objScript.audioCli[3]); break;
            case "Fire": objScript.effects.PlayOneShot(objScript.audioCli[4]); break;
            case "School": objScript.effects.PlayOneShot(objScript.audioCli[5]); break;
            case "b2": objScript.effects.PlayOneShot(objScript.audioCli[6]); break;
            case "cement": objScript.effects.PlayOneShot(objScript.audioCli[7]); break;
            case "e46": objScript.effects.PlayOneShot(objScript.audioCli[8]); break;
            case "e61": objScript.effects.PlayOneShot(objScript.audioCli[9]); break;
            case "WorkCar": objScript.effects.PlayOneShot(objScript.audioCli[10]); break;
            case "Police": objScript.effects.PlayOneShot(objScript.audioCli[11]); break;
            case "Tractor": objScript.effects.PlayOneShot(objScript.audioCli[12]); break;
            case "Tractor2": objScript.effects.PlayOneShot(objScript.audioCli[13]); break;
        }
    }
}