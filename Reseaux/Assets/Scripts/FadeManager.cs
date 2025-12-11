using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    
    
    [Header("Assign your fade image prefab here")]
    public GameObject fadeImagePrefab;

    private CanvasGroup fadeCanvasGroup;
    private GameObject fadeInstance;

    private void Awake()
    {
        fadeInstance = Instantiate(fadeImagePrefab, transform);
        fadeCanvasGroup = fadeInstance.GetComponent<CanvasGroup>();
        if (fadeCanvasGroup == null)
            fadeCanvasGroup = fadeInstance.AddComponent<CanvasGroup>();

        fadeCanvasGroup.alpha = 0f; 
    }
    
    public void FadeIn(float duration = 1f)
    {
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(1f, duration));
    }
    public void FadeOut(float duration = 1f)
    {
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(0f, duration));
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        float startAlpha = fadeCanvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }
}