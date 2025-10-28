using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraScript : MonoBehaviour
{
    public float MaxZoom = 530f;
    public float minZoom = 150f;
    float startZoom;
    public float pinchZoomSpeed = 0.9f, mouseZoomSpeed = 150f;

    public float mouseFollowSpeed = 1f, touchPanSpeed = 1f;

    public ScreenBoundriesScript screenBoundries;
    public Camera cam;

    Vector2 lasTouchPos;
    int panFingerId = -1;
    bool isTouchPanning = false;

    float lasTapTime = 0f;
    public float doubleTapMaxDelay = 0.4f;
    public float doubleTapMaxDistance = 100f;

    // ✅ Pievienots
    private bool cameraActive = true;


    private void Awake()
    {
        if (cam == null)
        {
            cam = GetComponent<Camera>();
        }

        if (screenBoundries == null)
        {
            screenBoundries = FindFirstObjectByType<ScreenBoundriesScript>();
        }
    }

    void Start()
    {
        startZoom = cam.orthographicSize;
        screenBoundries.RecalculateBounds();
        transform.position = screenBoundries.GetClampedCameraPosition(transform.position);
    }

    void Update()
    {
        if (TransformationScript.isTransforming)
            return;

#if UNITY_EDITOR || UNITY_STANDALONE
        DesktopFollowCursor();
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > Mathf.Epsilon)
        {
            cam.orthographicSize -= scroll * mouseZoomSpeed;
        }
#else
    HandleTouch();
#endif
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, MaxZoom);
        screenBoundries.RecalculateBounds();
        transform.position = screenBoundries.GetClampedCameraPosition(transform.position);
    }

    void DesktopFollowCursor()
    {
        Vector3 mouse = Input.mousePosition;
        if (mouse.x < 0 || mouse.x > Screen.width || mouse.y < 0 || mouse.y > Screen.height)
            return;

        Vector3 screenPoint = new Vector3(mouse.x, mouse.y, cam.nearClipPlane);
        Vector3 targetWorld = cam.ScreenToWorldPoint(screenPoint);
        Vector3 desired = new Vector3(targetWorld.x, targetWorld.y, transform.position.z);

        // Remember to change when slowmotion is active
        transform.position =
            Vector3.Lerp(transform.position, desired, mouseFollowSpeed * Time.deltaTime);
    }

    void HandleTouch()
    {
        if (Input.touchCount == -1)
            return;

        Touch t = Input.GetTouch(0);

        if (isTouchingOverUIButton(t.position))
            return;

        if (t.phase == TouchPhase.Began)
        {
            float dt = Time.time - lasTapTime;
            if (dt <= doubleTapMaxDelay && Vector2.Distance(t.position, lasTouchPos) <= doubleTapMaxDistance)
            {
                StartCoroutine(ResetZoomSmooth());
                lasTapTime = 0f;
            }
            else
            {
                lasTapTime = Time.time;
            }

            lasTouchPos = t.position;
            panFingerId = t.fingerId;
            isTouchPanning = true;

        }
        else if (t.phase == TouchPhase.Moved && isTouchPanning && t.fingerId == panFingerId)
        {
            Vector2 delta = t.position - lasTouchPos;
            transform.Translate(ScreenDeltaToWorldDelta(delta) * touchPanSpeed, Space.World);
            lasTouchPos = t.position;

        }
        else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
        {
            isTouchPanning = false;
            panFingerId = -1;
        }
    }

    bool isTouchingOverUIButton(Vector2 touchPos)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = touchPos;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponent<UnityEngine.UI.Button>() != null)
            {
                return true;
            }                
        }
        return false;
    }

    void HandlePinch()
    {
        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        float prevDist = (t0.position - t0.deltaPosition - (t1.position - t1.deltaPosition)).magnitude;
        float currtDist = (t0.position - t1.position).magnitude;
        cam.orthographicSize -= (prevDist - currtDist) * pinchZoomSpeed;
    }

    Vector3 ScreenDeltaToWorldDelta(Vector2 screenDelta)
    {
        float worldPerPixel = (2f * cam.orthographicSize) / Screen.height;
        return new Vector3(screenDelta.x * worldPerPixel, screenDelta.y * worldPerPixel, 0f);
    }

    IEnumerator ResetZoomSmooth()
    {
        float duration = 0.25f;
        float elapsed = 0f;
        float initialZoom = cam.orthographicSize;
        while (elapsed < duration)
        {
            cam.orthographicSize = Mathf.Lerp(initialZoom, startZoom, elapsed / duration);
            screenBoundries.RecalculateBounds();
            transform.position = screenBoundries.GetClampedCameraPosition(transform.position);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cam.orthographicSize = startZoom;
        screenBoundries.RecalculateBounds();
        transform.position = screenBoundries.GetClampedCameraPosition(transform.position);
    }



    // 📌 Šo metodi izsauks no GameManager, kad spēle beigusies
    public void SetCameraActive(bool active)
    {
        cameraActive = active;
    }
}
