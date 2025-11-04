using UnityEngine;

public class FlyingObjectSpawnScript : MonoBehaviour
{
    ScreenBoundriesScript screenBoundriesScript;
    public GameObject[] cludsPrefabs;
    public GameObject[] objectPrefabs;
    public Transform spawnPoint;

    public float cloudSpawnInterval = 2f;
    public float objectSpawnInterval = 3f;
    private float minY, maxY, minX, maxX;
    public float cloudMinSpeed = 1.5f;
    public float cloudMaxSpeed = 150f;
    public float objectMinSpeed = 2f;
    public float objectMaxSpeed = 200f;

    // If StartSpawning was already invoked, this prevents duplicate invokes.
    private bool spawningStarted = false;

    void Start()
    {
        screenBoundriesScript = FindFirstObjectByType<ScreenBoundriesScript>();

        if (screenBoundriesScript != null)
        {
            minY = screenBoundriesScript.minY;
            maxY = screenBoundriesScript.maxY;
            minX = screenBoundriesScript.minX;
            maxX = screenBoundriesScript.maxX;
        }
        else
        {
            minY = -200f;
            maxY = 200f;
            minX = -960f;
            maxX = 960f;
        }

        // Start automatic spawning (if not started externally)
        StartSpawning();
    }

    // Make StartSpawning public so other runtime validators can ensure spawning runs on Android builds.
    public void StartSpawning()
    {
        if (spawningStarted) return;
        spawningStarted = true;

        // Safety: avoid calling InvokeRepeating when playmode already invoked it
        if (cludsPrefabs != null && cludsPrefabs.Length > 0)
            InvokeRepeating(nameof(SpawnCloud), 0f, cloudSpawnInterval);

        if (objectPrefabs != null && objectPrefabs.Length > 0)
            InvokeRepeating(nameof(SpawnObject), 0f, objectSpawnInterval);
    }

    // Made public so a startup helper can trigger one-off spawns if Start wasn't executed.
    public void SpawnCloud()
    {
        if (cludsPrefabs == null || cludsPrefabs.Length == 0)
            return;

        GameObject cloudPrefab = cludsPrefabs[Random.Range(0, cludsPrefabs.Length)];

        // Choose random X and Y across world bounds so clouds are distributed over canvas
        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);
        Vector3 spawnPosition = new Vector3(x, y, spawnPoint != null ? spawnPoint.position.z : 0f);

        // Instantiate under spawnPoint (keeps hierarchy) but ensure world position is used
        GameObject cloud = Instantiate(cloudPrefab, spawnPosition, Quaternion.identity, spawnPoint);

        // put clouds in front of other siblings if desired
        cloud.transform.SetAsLastSibling();

        float movementSpeed = Random.Range(cloudMinSpeed, cloudMaxSpeed);
        FlyingObjectsControllerScript controller = cloud.GetComponent<FlyingObjectsControllerScript>();
        if (controller != null) controller.speed = movementSpeed;
    }

    public void SpawnObject()
    {
        if (objectPrefabs == null || objectPrefabs.Length == 0)
            return;

        GameObject objectPrefab = objectPrefabs[Random.Range(0, objectPrefabs.Length)];
        float y = Random.Range(minY, maxY);

        Vector3 spawnPosition = new Vector3(-spawnPoint.position.x, y, spawnPoint.position.z);

        GameObject flyingObject =
            Instantiate(objectPrefab, spawnPosition, Quaternion.identity, spawnPoint);
        float movementSpeed = Random.Range(objectMinSpeed, objectMaxSpeed);
        FlyingObjectsControllerScript controller =
            flyingObject.GetComponent<FlyingObjectsControllerScript>();
        if (controller != null) controller.speed = -movementSpeed;
    }
}