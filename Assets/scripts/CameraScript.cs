using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public float MaxZoom = 300f;
    public float minZoom = 150f;
    public float panSpeed = 6f;

    Vector3 bottomLeft, topRight;
    float cameraMaxX, cameraMinX, cameraMaxY, cameraMinY, x, y;

    public Camera cam;

    // ✅ Pievienots
    private bool cameraActive = true;

    void Start()
    {
        cam = GetComponent<Camera>();
        topRight = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight, -transform.position.z));
        bottomLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, -transform.position.z));
        cameraMaxX = topRight.x;
        cameraMaxY = topRight.y;
        cameraMinX = bottomLeft.x;
        cameraMinY = bottomLeft.y;
    }

    void Update()
    {
        if (!cameraActive) return; // 🚫 Ja kamera izslēgta — nedarīt neko

        x = Input.GetAxis("Mouse X") * panSpeed;
        y = Input.GetAxis("Mouse Y") * panSpeed;
        transform.Translate(x, y, 0);

        if ((Input.GetAxis("Mouse ScrollWheel") > 0) && cam.orthographicSize > minZoom)
        {
            cam.orthographicSize -= 50f;
        }

        if ((Input.GetAxis("Mouse ScrollWheel") < 0) && cam.orthographicSize < MaxZoom)
        {
            cam.orthographicSize += 50f;
        }

        topRight = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight, -transform.position.z));
        bottomLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, -transform.position.z));

        if (topRight.x > cameraMaxX)
        {
            transform.position = new Vector3(transform.position.x - (topRight.x - cameraMaxX), transform.position.y, transform.position.z);
        }

        if (topRight.y > cameraMaxY)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - (topRight.y - cameraMaxY), transform.position.z);
        }

        if (bottomLeft.x < cameraMinX)
        {
            transform.position = new Vector3(transform.position.x + (cameraMinX - bottomLeft.x), transform.position.y, transform.position.z);
        }

        if (bottomLeft.y < cameraMinY)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + (cameraMinY - bottomLeft.y), transform.position.z);
        }
    }

    // 📌 Šo metodi izsauks no GameManager, kad spēle beigusies
    public void SetCameraActive(bool active)
    {
        cameraActive = active;
    }
}
