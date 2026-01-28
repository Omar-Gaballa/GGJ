using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [SerializeField] private GameObject pauseMenuPanel;   // Panel (child of Canvas)
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private int mainMenuSceneIndex = 0;

    private CanvasGroup panelGroup;
    private Coroutine fadeCoroutine;
    private bool isPaused;

    void Awake()
    {
        if (!pauseMenuPanel)
        {
            Debug.LogError("PauseMenu: pauseMenuPanel is not assigned!");
            enabled = false;
            return;
        }

        panelGroup = pauseMenuPanel.GetComponent<CanvasGroup>();
        if (!panelGroup) panelGroup = pauseMenuPanel.AddComponent<CanvasGroup>();

        // Initialize hidden & non-interactive
        ApplyState(alpha: 0f, interactive: false, active: false);
        isPaused = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        if (isPaused) return;
        isPaused = true;

        Time.timeScale = 0f;

        // Enable before fade so it can render & receive input
        ApplyState(alpha: panelGroup.alpha, interactive: true, active: true);
        StartFade(1f);
    }

    public void Resume()
    {
        if (!isPaused) return;
        isPaused = false;

        // Stop interactions immediately, fade out visually
        ApplyState(alpha: panelGroup.alpha, interactive: false, active: true);

        Time.timeScale = 1f;

        StartFade(0f, () => ApplyState(alpha: 0f, interactive: false, active: false));
    }

    public void LoadMainMenu()
    {
        // Always unpause before scene load
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneIndex);
    }

    private void StartFade(float targetAlpha, Action onComplete = null)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha, onComplete));
    }

    private IEnumerator FadeRoutine(float targetAlpha, Action onComplete)
    {
        float startAlpha = panelGroup.alpha;

        // Micro-optimization + avoids divide by zero
        if (fadeDuration <= 0.0001f)
        {
            panelGroup.alpha = targetAlpha;
            fadeCoroutine = null;
            onComplete?.Invoke();
            yield break;
        }

        float invDuration = 1f / fadeDuration;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * invDuration; // works while paused
            panelGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        panelGroup.alpha = targetAlpha;
        fadeCoroutine = null;
        onComplete?.Invoke();
    }

    private void ApplyState(float alpha, bool interactive, bool active)
    {
        pauseMenuPanel.SetActive(active);
        panelGroup.alpha = alpha;
        panelGroup.interactable = interactive;
        panelGroup.blocksRaycasts = interactive;
    }
}
