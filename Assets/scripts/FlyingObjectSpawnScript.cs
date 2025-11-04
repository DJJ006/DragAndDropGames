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

    // New tuning fields to control spawn area relative to map bounds
    [Header("Spawn area tuning")]
    public float spawnPadding = 300f;         // horizontal distance outside map to spawn
    public float spawnVerticalExtra = 300f;   // extra vertical range beyond map top/bottom
    public bool randomizeDirection = true;    // allow objects to go left or right

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

        // Choose movement speed and optional random direction
        float movementSpeed = Random.Range(cloudMinSpeed, cloudMaxSpeed);
        if (randomizeDirection && Random.value < 0.5f)
            movementSpeed = -movementSpeed; // negative speed -> moves right (controller negates speed in movement)

        // Spawn outside map on the side that matches the direction so clouds fly over the whole map
        float x;
        if (movementSpeed > 0f)
        {
            // moving left -> spawn to the right of the map
            x = maxX + spawnPadding;
        }
        else
        {
            // moving right -> spawn to the left of the map
            x = minX - spawnPadding;
        }

        // Scatter vertically across an expanded area
        float y = Random.Range(minY - spawnVerticalExtra, maxY + spawnVerticalExtra);

        Vector3 spawnPosition = new Vector3(x, y, spawnPoint != null ? spawnPoint.position.z : 0f);

        // Instantiate under spawnPoint (keeps hierarchy) but ensure world position is used
        GameObject cloud = Instantiate(cloudPrefab, spawnPosition, Quaternion.identity, spawnPoint);

        // put clouds in front of other siblings if desired
        cloud.transform.SetAsLastSibling();

        FlyingObjectsControllerScript controller = cloud.GetComponent<FlyingObjectsControllerScript>();
        if (controller != null) controller.speed = movementSpeed;
    }

    public void SpawnObject()
    {
        if (objectPrefabs == null || objectPrefabs.Length == 0)
            return;

        GameObject objectPrefab = objectPrefabs[Random.Range(0, objectPrefabs.Length)];

        // Choose movement speed and optional random direction
        float movementSpeed = Random.Range(objectMinSpeed, objectMaxSpeed);
        if (randomizeDirection && Random.value < 0.5f)
            movementSpeed = -movementSpeed;

        // Determine spawn x based on direction so object will traverse the map
        float x;
        if (movementSpeed > 0f)
        {
            // moving left -> spawn to the right
            x = maxX + spawnPadding;
        }
        else
        {
            // moving right -> spawn to the left
            x = minX - spawnPadding;
        }

        // Scatter vertically across an expanded area
        float y = Random.Range(minY - spawnVerticalExtra, maxY + spawnVerticalExtra);

        Vector3 spawnPosition = new Vector3(x, y, spawnPoint != null ? spawnPoint.position.z : 0f);

        GameObject flyingObject = Instantiate(objectPrefab, spawnPosition, Quaternion.identity, spawnPoint);

        FlyingObjectsControllerScript controller = flyingObject.GetComponent<FlyingObjectsControllerScript>();
        if (controller != null) controller.speed = movementSpeed;
    }
}