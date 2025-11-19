using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
public class TouchDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    private RectTransform rect;
    private Canvas canvas;
    private Vector2 originalPos;
    private Transform originalParent;
    private Peg originPeg;
    private CanvasGroup canvasGroup;

    // Cached raycast components for UI hit testing
    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null) Debug.LogWarning("TouchDrag needs a Canvas parent.");
        raycaster = canvas != null ? canvas.GetComponent<GraphicRaycaster>() : null;
        eventSystem = EventSystem.current;
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // required to receive drag events on Android reliably
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = rect.parent;
        originalPos = rect.anchoredPosition;
        originPeg = originalParent != null ? originalParent.GetComponent<Peg>() : null;

        // Only allow dragging if this disk is the top of the peg
        if (originPeg != null)
        {
            Disk thisDisk = GetComponent<Disk>();
            if (originPeg.Peek() != thisDisk)
            {
                // Cancel drag by marking canvasGroup blocksRaycasts false briefly
                canvasGroup.blocksRaycasts = true;
                return;
            }
        }

        // lift to top
        rect.SetParent(canvas.transform, worldPositionStays: false);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.9f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, eventData.position, eventData.pressEventCamera, out pos);
        rect.anchoredPosition = pos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // detect which Peg (UI) is under pointer
        Peg targetPeg = RaycastForPeg(eventData);
        HanoiGameManager gm = FindObjectOfType<HanoiGameManager>();

        if (originPeg != null && targetPeg != null && gm != null)
        {
            bool moved = gm.TryMoveDisk(originPeg, targetPeg);
            if (!moved)
                ReturnToOrigin();
        }
        else
        {
            ReturnToOrigin();
        }
    }

    private Peg RaycastForPeg(PointerEventData eventData)
    {
        if (raycaster == null || eventSystem == null) return null;
        List<RaycastResult> results = new List<RaycastResult>();
        eventData.position = Input.touchCount > 0 ? (Vector2)Input.touches[0].position : eventData.position;
        raycaster.Raycast(eventData, results);
        foreach (var r in results)
        {
            var go = r.gameObject;
            var peg = go.GetComponentInParent<Peg>();
            if (peg != null) return peg;
        }
        return null;
    }

    private void ReturnToOrigin()
    {
        // snap back under original peg
        rect.SetParent(originalParent, worldPositionStays: false);
        rect.anchoredPosition = originalPos;
    }
}