using System.Xml.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlyingObjectsControllerScript : MonoBehaviour
{
    [HideInInspector]
    public float speed = 1f;
    public float fadeDuration = 1.5f;
    public float waveAmplitude = 25f;
    public float waveFrequency = 1f;
    private ObjectScript objectScript;
    private ScreenBoundriesScript scrreenBoundriesScript;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private bool isFadingOut = false;
    private bool isExploading = false;
    private Image image;
    private Color originalColor;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        rectTransform = GetComponent<RectTransform>();

        image = GetComponent<Image>();
        if (image != null)
            originalColor = image.color;
        objectScript = FindFirstObjectByType<ObjectScript>();
        scrreenBoundriesScript = FindFirstObjectByType<ScreenBoundriesScript>();
        StartCoroutine(FadeIn());
    }

    // Safe wrapper for tag checks without calling CompareTag (CompareTag throws when the tag
    // parameter is not defined in the project's Tag Manager). Use string compare to avoid the exception.
    private bool HasTag(string tagName)
    {
        if (string.IsNullOrEmpty(tagName) || gameObject == null) return false;
        try
        {
            // Compare tag strings case-insensitively to accept "Bomb" and "bomb"
            return string.Equals(gameObject.tag, tagName, System.StringComparison.OrdinalIgnoreCase);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Tag check failed for '{tagName}': {ex.Message}");
            return false;
        }
    }

    // Overload for GameObject to check tags safely
    private bool HasTag(GameObject go, string tagName)
    {
        if (go == null || string.IsNullOrEmpty(tagName)) return false;
        try
        {
            return string.Equals(go.tag, tagName, System.StringComparison.OrdinalIgnoreCase);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Tag check failed for '{tagName}' on GameObject '{go.name}': {ex.Message}");
            return false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // defensive: ensure required components exist
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (scrreenBoundriesScript == null) scrreenBoundriesScript = FindFirstObjectByType<ScreenBoundriesScript>();
        if (image == null) image = GetComponent<Image>();

        if (rectTransform == null || scrreenBoundriesScript == null) return;

        float waveOffset = Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;
        rectTransform.anchoredPosition += new Vector2(-speed * Time.deltaTime, waveOffset * Time.deltaTime);

        if (speed > 0 && transform.position.x < (scrreenBoundriesScript.minX + 80) && !isFadingOut)
        {
            StartCoroutine(FadeOutAndDestroy());
            isFadingOut = true;
        }

        if (speed < 0 && transform.position.x > (scrreenBoundriesScript.maxX - 80) && !isFadingOut)
        {
            StartCoroutine(FadeOutAndDestroy());
            isFadingOut = true;
        }

        // Click-to-explode for bombs (left mouse button)
        // NOTE: some places in the project use tag "bomb" (lowercase). Accept both.
        // Also pick the correct camera parameter for RectangleContainsScreenPoint: pass null for ScreenSpaceOverlay canvas.
        Canvas parentCanvas = rectTransform.GetComponentInParent<Canvas>();
        Camera hitCamera = (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : Camera.main;

        if ((HasTag("Bomb") || HasTag("bomb")) && !isExploading &&
            Input.GetMouseButtonDown(0) &&
            RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, hitCamera))
        {
            // Avoid accessing the 'tag' property directly in a way that would throw; we already used safe compare.
            string safeTag;
            try { safeTag = tag; } catch { safeTag = "<undefined>"; }

            Debug.Log($"Bomb clicked: {name} (tag={safeTag})");
            TriggerExplosion();
        }

        // Caurskatīt no šejienes
        if (ObjectScript.drag && !isFadingOut &&
            RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, Camera.main))
        {
            Debug.Log("The cursor collided with a flying object!");

            if (ObjectScript.lastDragged != null)
            {
                StartCoroutine(ShrinkAndDestroy(ObjectScript.lastDragged, 0.5f));
                ObjectScript.lastDragged = null;
                ObjectScript.drag = false;
            }

            // uniform behavior for bombs and others
            StartToDestroy(Color.cyan);
        }
    }

    public void TriggerExplosion()
    {
        isExploading = true;
        if (objectScript != null && objectScript.effects != null)
            objectScript.effects.PlayOneShot(objectScript.audioCli[6], 5f);

        if (TryGetComponent<Animator>(out Animator animator))
        {
            animator.SetBool("explode", true);
        }

        if (image != null)
        {
            image.color = Color.red;
            StartCoroutine(RecoverColor(0.4f));
        }

        StartCoroutine(Vibrate());
        StartCoroutine(WaitBeforeExpload());
    }

    IEnumerator WaitBeforeExpload()
    {
        float radius = 0f;
        if (TryGetComponent<CircleCollider2D>(out CircleCollider2D circleCollider))
        {
            radius = circleCollider.radius * transform.lossyScale.x;
        }
        ExploadAndDestroy(radius);
        yield return new WaitForSeconds(1f);
        ExploadAndDestroy(radius);
        Destroy(gameObject);
    }

    void ExploadAndDestroy(float radius)
    {
        // Find all colliders in radius and destroy only clouds (do not affect vehicles)
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider == null || hitCollider.gameObject == gameObject) continue;

            GameObject target = hitCollider.gameObject;

            // Only affect objects explicitly tagged as "Cloud"
            if (!HasTag(target, "Cloud")) continue;

            FlyingObjectsControllerScript obj =
                target.GetComponent<FlyingObjectsControllerScript>();

            if (obj != null && !obj.isExploading)
            {
                obj.StartToDestroy(Color.cyan); // Pass the required Color argument
            }
        }
    }

    public void StartToDestroy(Color cyan)
    {
        if (!isFadingOut)
        {
            StartCoroutine(FadeOutAndDestroy());
            isFadingOut = true;

            if (image != null)
            {
                image.color = Color.cyan;
                StartCoroutine(RecoverColor(0.5f));
            }

            if (objectScript != null && objectScript.effects != null)
                objectScript.effects.PlayOneShot(objectScript.audioCli[5]);

            StartCoroutine(Vibrate());
        }
    }

    IEnumerator Vibrate()
    {
        Vector2 originalPosition = rectTransform.anchoredPosition;
        float duration = 0.3f;
        float elpased = 0f;
        float intensity = 5f;

        while (elpased < duration)
        {
            rectTransform.anchoredPosition =
                originalPosition + Random.insideUnitCircle * intensity;
            elpased += Time.deltaTime;
            yield return null;
        }
        rectTransform.anchoredPosition = originalPosition;
    }

    IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    IEnumerator FadeOutAndDestroy()
    {
        float t = 0f;
        float startAlpha = canvasGroup.alpha;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        Destroy(gameObject);
    }

    IEnumerator ShrinkAndDestroy(GameObject target, float duration)
    {
        Vector3 originalScale = target.transform.localScale;
        Quaternion originalRotation = target.transform.rotation;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            target.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t / duration);
            float angle = Mathf.Lerp(0f, 360f, t / duration);
            target.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        // Ja iznīcinātais ir transportlīdzeklis
        if (HasTag(target, "Garbage") || HasTag(target, "Ambulance") || HasTag(target, "Fire") ||
            HasTag(target, "School") || HasTag(target, "b2") || HasTag(target, "cement") ||
            HasTag(target, "e46") || HasTag(target, "e61") || HasTag(target, "WorkCar") ||
            HasTag(target, "Police") || HasTag(target, "Tractor") || HasTag(target, "Tractor2"))
        {
            var gm = FindObjectOfType<GameManager>();
            if (gm != null) gm.OnVehicleDestroyed(target);
        }

        Destroy(target);
    }


    IEnumerator RecoverColor(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (image != null)
            image.color = originalColor;
    }


}