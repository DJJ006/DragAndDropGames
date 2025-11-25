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

    // World-space offset between pointer and object when dragging
    private Vector3 dragOffsetWorld;
    private Camera uiCamera;

    // whether this disk is allowed to be dragged (top of its peg)
    private bool canDrag = false;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null) Debug.LogWarning("TouchDrag needs a Canvas parent.");
        raycaster = canvas != null ? canvas.GetComponent<GraphicRaycaster>() : null;
        eventSystem = EventSystem.current;
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (canvas != null)
            uiCamera = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : canvas.worldCamera; // worldCamera may be null for Overlay - handled later
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // required to receive drag events on Android reliably
        // Determine whether this disk is the top disk on its peg so we can prevent dragging lower disks.
        originalParent = rect.parent;
        originPeg = originalParent != null ? originalParent.GetComponent<Peg>() : null;
        Disk thisDisk = GetComponent<Disk>();
        canDrag = originPeg == null || originPeg.Peek() == thisDisk;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Ensure we have peg info even if OnPointerDown wasn't called
        if (originalParent == null)
            originalParent = rect.parent;
        if (originPeg == null)
            originPeg = originalParent != null ? originalParent.GetComponent<Peg>() : null;

        // Fallback compute if needed
        if (!canDrag)
        {
            Disk thisDisk = GetComponent<Disk>();
            canDrag = originPeg == null || originPeg.Peek() == thisDisk;
        }

        if (!canDrag)
        {
            // Not allowed to drag disks below others
            canvasGroup.blocksRaycasts = true;
            return;
        }

        originalPos = rect.anchoredPosition;

        // bring near top of hierarchy so it renders over others
        if (canvas != null)
        {
            rect.SetParent(canvas.transform, worldPositionStays: true);
            int lastIndex = canvas.transform.childCount - 1;
            int position = Mathf.Max(0, lastIndex - 1);
            rect.SetSiblingIndex(position);
        }

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.9f;

        // compute world-space offset between pointer and object so it follows grab point
        Vector3 pointerWorld;
        if (ScreenPointToWorld(eventData.position, out pointerWorld))
        {
            dragOffsetWorld = rect.position - pointerWorld;
        }
        else
        {
            dragOffsetWorld = Vector3.zero;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        if (!canDrag) return;

        Vector3 pointerWorld;
        if (!ScreenPointToWorld(eventData.position, out pointerWorld))
            return;

        Vector3 desired = pointerWorld + dragOffsetWorld;
        // preserve original z
        desired.z = rect.position.z;
        rect.position = desired;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // detect which Peg (UI) is under pointer
        Peg targetPeg = RaycastForPeg(eventData);
        HanoiGameManager gm = FindObjectOfType<HanoiGameManager>();

        if (canDrag && originPeg != null && targetPeg != null && gm != null)
        {
            bool moved = gm.TryMoveDisk(originPeg, targetPeg);
            if (!moved)
                ReturnToOrigin();
        }
        else
        {
            ReturnToOrigin();
        }

        // reset canDrag (will be recalculated on next pointerdown)
        canDrag = false;
    }

    private bool ScreenPointToWorld(Vector2 screenPoint, out Vector3 worldPoint)
    {
        worldPoint = Vector3.zero;
        if (canvas == null)
            return false;

        RectTransform canvasRect = (RectTransform)canvas.transform;
        // Support ScreenSpace-Overlay and Camera and World canvases
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // Camera parameter should be null for overlay
            return RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, screenPoint, null, out worldPoint);
        }
        else
        {
            Camera cam = uiCamera;
            if (cam == null) // fallback to Camera.main
                cam = Camera.main;
            if (cam == null)
                return false;

            return RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, screenPoint, cam, out worldPoint);
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