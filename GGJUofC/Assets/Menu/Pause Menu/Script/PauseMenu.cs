using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [SerializeField] private Canvas pauseCanvas;          // Drag your Canvas here
    [SerializeField] private GameObject pauseMenuPanel;   // Drag PauseMenu Panel here (child of Canvas)

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.25f;

    [Header("Scene Index")]
    [SerializeField] private int mainMenuSceneIndex = 0;

    private CanvasGroup panelGroup;
    private bool isPaused;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        // Safety checks
        if (pauseCanvas == null)
            Debug.LogError("PauseMenu: pauseCanvas is not assigned!");

        if (pauseMenuPanel == null)
            Debug.LogError("PauseMenu: pauseMenuPanel is not assigned!");

        // Grab (or add) CanvasGroup on the panel
        if (pauseMenuPanel != null)
        {
            panelGroup = pauseMenuPanel.GetComponent<CanvasGroup>();
            if (panelGroup == null)
                panelGroup = pauseMenuPanel.AddComponent<CanvasGroup>();
        }
    }

    void Start()
    {
        // Start hidden but active (so it can fade)
        SetPanel(0f, false);
        isPaused = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        if (isPaused || panelGroup == null) return;
        isPaused = true;

        Time.timeScale = 0f;

        pauseMenuPanel.SetActive(true);
        panelGroup.interactable = true;
        panelGroup.blocksRaycasts = true;

        StartFade(1f);
    }

    public void Resume()
    {
        if (!isPaused || panelGroup == null) return;
        isPaused = false;

        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;

        Time.timeScale = 1f;

        StartFade(0f, () => pauseMenuPanel.SetActive(false));
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneIndex);
    }

    private void StartFade(float targetAlpha, System.Action onComplete = null)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha, onComplete));
    }

    private IEnumerator FadeRoutine(float targetAlpha, System.Action onComplete)
    {
        float startAlpha = panelGroup.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime; // works while paused
            float lerp = Mathf.Clamp01(t / fadeDuration);
            panelGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, lerp);
            yield return null;
        }

        panelGroup.alpha = targetAlpha;
        onComplete?.Invoke();
        fadeCoroutine = null;
    }

    private void SetPanel(float alpha, bool interactive)
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        if (panelGroup != null)
        {
            panelGroup.alpha = alpha;
            panelGroup.interactable = interactive;
            panelGroup.blocksRaycasts = interactive;
        }

        if (pauseMenuPanel != null && alpha == 0f)
            pauseMenuPanel.SetActive(false);
    }
}
