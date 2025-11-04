using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// StartupValidator v3
/// Garantē, ka visi World Space / Screen Space - Camera canvas saņem Camera.main
/// arī prefab instancēm, spawners tiek ieslēgti, EventSystem eksistē.
/// Pievieno šo skriptu pie GameManager vai cita persistenta GameObject.
/// </summary>
public class StartupValidator : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("StartupValidatorV3: Running checks...");

        // Ensure time is running
        Time.timeScale = 1f;

        // Ensure EventSystem exists
        if (EventSystem.current == null)
        {
            GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            es.hideFlags = HideFlags.DontSaveInBuild;
            Debug.Log("StartupValidatorV3: Created missing EventSystem");
        }

        // Ensure Main Camera exists and is enabled
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("StartupValidatorV3: Camera.main is null! Make sure a camera has MainCamera tag.");
        }
        else
        {
            mainCam.enabled = true;
            Debug.Log($"StartupValidatorV3: Main camera found: {mainCam.name}, enabled={mainCam.enabled}");
        }

        // Assign Camera.main to all canvas in scene (runtime objects)
        FixCanvasWorldCamera(mainCam);

        // Enable and start all spawners
        FlyingObjectSpawnScript[] spawners = FindObjectsOfType<FlyingObjectSpawnScript>();
        foreach (var s in spawners)
        {
            if (!s.enabled) s.enabled = true;
            s.StartSpawning();
            Debug.Log($"StartupValidatorV3: Enabled and started spawner {s.name}");
        }

        // Ensure core scripts enabled
        EnableSceneComponent<FlyingObjectsControllerScript>("FlyingObjectsControllerScript");
        EnableSceneComponent<DragAndDropScript>("DragAndDropScript");
        EnableSceneComponent<GameManager>("GameManager");
        EnableSceneComponent<ScreenBoundriesScript>("ScreenBoundriesScript");
        EnableSceneComponent<CameraScript>("CameraScript");

        // Subscribe to prefab instantiation for dynamically spawned canvas
        CanvasSpawnObserver.OnCanvasSpawned += (canvas) =>
        {
            FixSingleCanvas(canvas, mainCam);
        };

        Debug.Log("StartupValidatorV3: Checks complete!");
    }

    private void FixCanvasWorldCamera(Camera cam)
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas c in canvases)
        {
            FixSingleCanvas(c, cam);
        }
    }

    private void FixSingleCanvas(Canvas c, Camera cam)
    {
        if (c.renderMode == RenderMode.WorldSpace || c.renderMode == RenderMode.ScreenSpaceCamera)
        {
            if (c.worldCamera != cam)
            {
                c.worldCamera = cam;
                Debug.Log($"StartupValidatorV3: Assigned worldCamera to canvas {c.name}");
            }
        }

        // Ensure GraphicRaycaster exists
        if (c.GetComponent<GraphicRaycaster>() == null)
        {
            c.gameObject.AddComponent<GraphicRaycaster>();
            Debug.Log($"StartupValidatorV3: Added GraphicRaycaster to canvas {c.name}");
        }
    }

    private void EnableSceneComponent<T>(string compName) where T : MonoBehaviour
    {
        T[] all = FindObjectsOfType<T>();
        foreach (var c in all)
        {
            if (!c.enabled)
            {
                c.enabled = true;
                Debug.Log($"StartupValidatorV3: Enabled {compName} on {c.gameObject.name}");
            }
        }
    }
}

/// <summary>
/// Optional helper to notify about newly instantiated canvas prefabs
/// Add this to a prefab canvas as a component
/// </summary>
public class CanvasSpawnObserver : MonoBehaviour
{
    public static event System.Action<Canvas> OnCanvasSpawned;

    void Awake()
    {
        Canvas c = GetComponent<Canvas>();
        OnCanvasSpawned?.Invoke(c);
    }
}
