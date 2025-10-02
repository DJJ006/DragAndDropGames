using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObstaclesControllerScript : MonoBehaviour
{

    [Tooltip("List every vehicle tag you want the cloud to catch")]
    [SerializeField]
    private string[] vehicleTags = {
        "Garbage",
        "Ambulance",
        "Fire",
        "School",
        "b2",
        "cement",
        "e46",
        "e61",
        "WorkCar",
        "Police",
        "Tractor",
        "Tractor2"
    };

    [HideInInspector]
    public float speed = 1f;
    public float waveAmplitude = 25f;
    public float waveFrequency = 1f;
    public float fadeDuration = 1.5f;
    private ObjectScript objectScript;
    private ScreenBoundriesScript screenBoundriesScript;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private bool isFadingOut = false;
    private bool isExploding = false;
    private Image image;
    private Color orginalColor;
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        rectTransform = GetComponent<RectTransform>();

        image = GetComponent<Image>();
        orginalColor = image.color;

        objectScript = FindObjectOfType<ObjectScript>();
        screenBoundriesScript = FindObjectOfType<ScreenBoundriesScript>();
        StartCoroutine(FadeIn());
    }

    // Update is called once per frame
    void Update()
    {
        float waveOffset = Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;
        rectTransform.anchoredPosition += new Vector2(-speed * Time.deltaTime, waveOffset * Time.deltaTime);

        //Iznīcinas ja lido pa kreisi
        if(speed > 0 && transform.position.x < (screenBoundriesScript.minX + 80) && !isFadingOut){
            isFadingOut = true;
            StartCoroutine(FadeOutAndDestroy());
        }

        //Iznīcinas ja lido pa labi
        if (speed < 0 && transform.position.x < (screenBoundriesScript.minX - 80) && !isFadingOut)
        {
            isFadingOut = true;
            StartCoroutine(FadeOutAndDestroy());
        }

        //Ja neko nevelk un kursors pieskaras bumbai
        if(CompareTag("bomb") && !isExploding && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, 
            Input.mousePosition, Camera.main))

        if(objectScript.drag && !isFadingOut && RectTransformUtility.RectangleContainsScreenPoint(
            rectTransform, Input.mousePosition, Camera.main))
            {
                Debug.Log("Obstacle hit by cursor (without draging)");
                TriggerExplosion();
            }

        {
            Debug.Log("Obstacle hit by drag");
            //.............
        }

    }

    IEnumerator FadeIn()
    {
        float a = 0f;
        while(a < fadeDuration)
        {
            a += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, a / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
        }

    IEnumerator FadeOutAndDestroy()
    {
        float a = 0f;
        float startAlpha = canvasGroup.alpha;
        while (a < fadeDuration)
        {
            a += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, a / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0;
        Destroy(gameObject);
    }

    IEnumerator ShrinkAndDestroy(GameObject target, float duration)
    {
        Vector3 orginalScale = target.transform.localScale;
        Quaternion orginalRotation = target.transform.rotation;
        float t = 0f;

        while(t < duration)
        {
            t += Time.deltaTime;
            target.transform.localScale = Vector3.Lerp(orginalScale, Vector3.zero, t / duration);
            float angle = Mathf.Lerp(0, 360, t / duration);
            target.transform.rotation = Quaternion.Euler(0, 0, angle);

            yield return null;
        }
        //Ko darīt ar mašīnu tālāk?
        //Nav obligāti jāiznīcina, varbūt jāatrgiež sākuma pozīcijā?
        Destroy(target);
    }

    IEnumerator RecoverColor(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        image.color = orginalColor;
    }

    IEnumerator Vibrate()
    {
        Vector2 orginalPosition = rectTransform.anchoredPosition;
        float duration = 0.3f;
        float elapsed = 0f;
        float intensity = 5f;

        while(elapsed < duration)
        {
            rectTransform.anchoredPosition = orginalPosition + UnityEngine.Random.insideUnitCircle * intensity;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collided object uses one of your vehicle tags
        foreach (var t in vehicleTags)
        {
            if (other.CompareTag(t))
            {
                StartCoroutine(ShrinkAndDestroy(other.gameObject, 0.5f));
                return;
            }
        }
    }

    public void TriggerExplosion()
    {
        isExploding = true;
        objectScript.effects.PlayOneShot(objectScript.audioCli[14], 5f);

        if(TryGetComponent<Animator>(out Animator animator))
        {
            animator.SetBool("explode", true);
        }

        image.color = Color.red;
        StartCoroutine(RecoverColor(0.3f));
        StartCoroutine(Vibrate());
        StartCoroutine(WaitBeforeExplosion());
    }

    void WaitBeforeExplosion()
    {
        float radius = 0;
        if(TryGetComponent<CircleCollider2D>(out CircleCollider2D circleCollider))
        {
            radius = circleCollider.radius * transform.lossyScale.x;
            ExploadAndDestroyNearbyObjects(radius);
            yield return new WaitForSeconds(1f);
            ExploadAndDestroyNearbyObjects(radius);
            Destroy(gameObject);
        }
    }

    void ExploadAndDestroyNearbyObjects(float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D hit in hits)
        {
            if(hit != null && hit.gameObject != gameObject)
            {
                ObstaclesControllerScript obj = hit.GetComponent<ObstaclesControllerScript>();
                if(obj != null && !obj.isExploding)
                {
                    obj.StartToDestroy();
                }
            }
        }
    }

    public void StartToDestroy()
    {
        if (!isFadingOut)
        {
            StartCoroutine(FadeOutAndDestroy());
            isFadingOut = true;

            image.color = Color.cyan;
            StartCoroutine(RecoverColor(0.5f));
            StartCoroutine(Vibrate());
            objectScript.effects.PlayOneShot(objectScript.audioCli[0]);
        }
    }

}
