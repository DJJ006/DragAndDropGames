using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//CHANGES FOR ANDROID

public class ScreenBoundriesScript : MonoBehaviour
{
    [HideInInspector]
    public Vector3 screenPoint, offset;
    [HideInInspector]
    public float minX, maxX, minY, maxY;

    // Updated default worldBounds to match canvas 3990x2310 (centered)
    public Rect worldBounds = new Rect(-1995f, -1155f, 3990f, 2310f);
    [Range(0f, 0.5f)]
    public float padding = 0.02f;

    public Camera targetCam;

    public float minCamX { get; private set; }
    public float maxCamX { get; private set; }
    public float minCamY { get; private set; }
    public float maxCamY { get; private set; }

    float lastOrthoSize;
    float lastAspect;
    Vector3 lastCamPosition;


    void Awake()
    {
        if (targetCam == null)
        {
            targetCam = Camera.main;
        }
        RecalculateBounds();
    }

    // Keep editor changes in sync
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        if (targetCam == null) targetCam = Camera.main;
        RecalculateBounds();
    }

    private void Update()
    {
        if (targetCam == null)
        {
            return;
        }

        bool changes = false;

        if (targetCam.orthographic)
        {
            if (!Mathf.Approximately(targetCam.orthographicSize, lastOrthoSize))
                changes = true;
        }

        if (!Mathf.Approximately(targetCam.aspect, lastAspect))
            changes = true;

        if (targetCam.transform.position != lastCamPosition)
            changes = true;

        if (changes)
            RecalculateBounds();
    }

    public void RecalculateBounds()
    {
        if (targetCam == null)
        {
            return;
        }

        float wbMinX = worldBounds.xMin;
        float wbMaxX = worldBounds.xMax;
        float wbMinY = worldBounds.yMin;
        float wbMaxY = worldBounds.yMax;

        // Ensure minX/maxX/minY/maxY reflect the actual world/map extents
        minX = wbMinX;
        maxX = wbMaxX;
        minY = wbMinY;
        maxY = wbMaxY;

        if (targetCam.orthographic)
        {
            float halfH = targetCam.orthographicSize;
            float halfW = halfH * targetCam.aspect;

            // Horizontal camera clamped extents based on camera size and world bounds
            if (halfW * 2f >= (wbMaxX - wbMinX))
            {
                minCamX = maxCamX = (wbMinX + wbMaxX) * 0.5f;
            }
            else
            {
                minCamX = wbMinX + halfW;
                maxCamX = wbMaxX - halfW;
            }

            // Vertical (fixed variable names and logic)
            if (halfH * 2f >= (wbMaxY - wbMinY))
            {
                minCamY = maxCamY = (wbMinY + wbMaxY) * 0.5f;
            }
            else
            {
                minCamY = wbMinY + halfH;
                maxCamY = wbMaxY - halfH;
            }
        }
        else
        {
            // For perspective camera use world bounds center as fallback
            minCamX = maxCamX = (wbMinX + wbMaxX) * 0.5f;
            minCamY = maxCamY = (wbMinY + wbMaxY) * 0.5f;
        }

        lastOrthoSize = targetCam.orthographicSize;
        lastAspect = targetCam.aspect;
        lastCamPosition = targetCam.transform.position;
    }

    // FOR DRAGGABLE OBJECTS
    public Vector2 GetClampedPosition(Vector3 curPosition)
    {
        float shrinkW = worldBounds.width * padding;
        float shrinkH = worldBounds.height * padding;
        float wbMinX = worldBounds.xMin + shrinkW;
        float wbMaxX = worldBounds.xMax - shrinkW;
        float wbMinY = worldBounds.yMin + shrinkH;
        float wbMaxY = worldBounds.yMax - shrinkH;

        float cx = Mathf.Clamp(curPosition.x, wbMinX, wbMaxX);
        float cy = Mathf.Clamp(curPosition.y, wbMinY, wbMaxY);

        return new Vector2(cx, cy);
    }

    // FOR CAMERA
    // changed return type to Vector3 so camera Z is preserved (was implicitly set to 0)
    public Vector3 GetClampedCameraPosition(Vector3 curPosition)
    {
        float cx = Mathf.Clamp(curPosition.x, minCamX, maxCamX);
        float cy = Mathf.Clamp(curPosition.y, minCamY, maxCamY);

        return new Vector3(cx, cy, curPosition.z);
    }

    // Helper: return the configured world bounds (map extents)
    public Rect GetWorldBounds()
    {
        return worldBounds;
    }
}