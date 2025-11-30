using UnityEngine;

public class FlyingObjectManager : MonoBehaviour
{
    public void DestroyAllFlyingObjects()
    {
        FlyingObjectsControllerScript[] flyingObjects =
            Object.FindObjectsByType<FlyingObjectsControllerScript>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (FlyingObjectsControllerScript obj in flyingObjects)
        {
            if (obj == null)
                continue;

            if (SafeCompareTag(obj, "Bomb"))
            {
                obj.TriggerExplosion();
            }
            else
            {
                obj.StartToDestroy(Color.cyan);
            }
        }
    }

    private bool SafeCompareTag(Component obj, string tag)
    {
        // CompareTag throws a UnityException if the tag is not defined.
        // Catch that so runtime doesn't spam the console / break execution.
        try
        {
            return obj.CompareTag(tag);
        }
        catch (UnityException)
        {
            Debug.LogWarning($"Tag: {tag} is not defined. Treating '{obj.name}' as not tagged.");
            return false;
        }
    }
}