using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


//CHANGES FOR ANDROID
public class DragAndDropScript : MonoBehaviour, IPointerDownHandler, IBeginDragHandler,
    IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGro;
    private RectTransform rectTra;
    public ObjectScript objectScr;
    public ScreenBoundriesScript screenBou;

    private Vector3 dragOffsetWorld;
    private Camera uiCamera;
    private Canvas canvas;

    // Start is called before the first frame update
    void Awake()
    {
        canvasGro = GetComponent<CanvasGroup>();
        rectTra = GetComponent<RectTransform>();

        if (objectScr == null)
        {
            objectScr = Object.FindFirstObjectByType<ObjectScript>();
        }

        if (screenBou == null)
        {
            screenBou = Object.FindFirstObjectByType<ScreenBoundriesScript>();
        }

        canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
            uiCamera = canvas.worldCamera;

        else
            Debug.LogError("Canvas not found for DragAndDropScript");

    }


    //CHANGES FOR ANDROID
    public void OnPointerDown(PointerEventData eventData)
    {
        
        Debug.Log("OnPointerDown");
        objectScr.effects.PlayOneShot(objectScr.audioCli[0]);
       
    }

    //CHANGES FOR ANDROID
    public void OnBeginDrag(PointerEventData eventData)
    {
            ObjectScript.drag = true;
            canvasGro.blocksRaycasts = false;
            canvasGro.alpha = 0.6f;
            //rectTra.SetAsLastSibling();
            int lastIndex = transform.parent.childCount - 1;
            int position = Mathf.Max(0, lastIndex - 1);
            transform.SetSiblingIndex(position);





        Vector3 pointerWorld;
        if (ScreenPointToWolrd(eventData.position, out pointerWorld))
        {
            dragOffsetWorld = transform.position - pointerWorld;
        }
        else
        {
            dragOffsetWorld = Vector3.zero;
        }

        ObjectScript.lastDragged = eventData.pointerDrag;

    }


    //CHANGES FOR ANDROID
    public void OnDrag(PointerEventData eventData)
    {
        Vector3 pointerWorld;
        if (!ScreenPointToWolrd(eventData.position, out pointerWorld))
            return;

        Vector3 desired = pointerWorld + dragOffsetWorld;
        desired.z = rectTra.position.z;
        screenBou.RecalculateBounds();

        Vector2 clamped = screenBou.GetClampedPosition(desired);
        transform.position = new Vector3(clamped.x, clamped.y, desired.z);
    }


    //CHANGES FOR ANDROID
    public void OnEndDrag(PointerEventData eventData)
    {
     
            ObjectScript.drag = false;
            canvasGro.blocksRaycasts = true;
            canvasGro.alpha = 1.0f;

            if (objectScr.rightPlace)
            {
                canvasGro.blocksRaycasts = false;
                ObjectScript.lastDragged = null;
            }

            objectScr.rightPlace = false;     
    }

    private bool ScreenPointToWolrd(Vector2 screenPoint, out Vector3 worldPoint)
    {
        worldPoint = Vector3.zero;
        if (uiCamera == null)
            return false;

        float z = Mathf.Abs(uiCamera.transform.position.z - transform.position.z);
        Vector3 sp = new Vector3(screenPoint.x, screenPoint.y, z);
        worldPoint = uiCamera.ScreenToWorldPoint(sp);
        return true;
    }

}