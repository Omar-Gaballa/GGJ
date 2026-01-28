using System.Collections;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [Header("Tutorial Panel")]
    [SerializeField] private GameObject tutorialPanel;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.3f;

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        // Get or add CanvasGroup automatically
        canvasGroup = tutorialPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = tutorialPanel.AddComponent<CanvasGroup>();
    }

    void Start()
    {
        tutorialPanel.SetActive(true);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        FadeIn();
    }

    public void CloseTutorial()
    {
        FadeOut();
    }

    private void FadeIn()
    {
        StartFade(1f);
    }

    private void FadeOut()
    {
        StartFade(0f, () =>
        {
            tutorialPanel.SetActive(false);
        });
    }

    private void StartFade(float targetAlpha, System.Action onComplete = null)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha, onComplete));
    }

    private IEnumerator FadeRoutine(float targetAlpha, System.Action onComplete)
    {
        float startAlpha = canvasGroup.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        onComplete?.Invoke();
        fadeCoroutine = null;
    }
}