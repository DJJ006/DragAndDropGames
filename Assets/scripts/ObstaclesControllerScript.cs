using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObstaclesControllerScript : MonoBehaviour
{
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

        if(objectScript.drag && !isFadingOut && RectTransformUtility.RectangleContainsScreenPoint(
            rectTransform, Input.mousePosition, Camera.main))
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

    IEnumerator RecoverColor()
    {
        yield return new WaitForSeconds(0.5f);
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

}
