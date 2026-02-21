using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DarkfallOnline.Events;

/// <summary>
/// Full-screen loading overlay that reacts to SceneLoader events.
/// Persists across all scenes via DontDestroyOnLoad.
///
/// ── Unity setup (Bootstrap scene) ────────────────────────────────────────
///
///   1. Create a Canvas (Screen Space – Overlay, Sort Order 99) in Bootstrap.
///   2. Attach this component to the Canvas root.
///   3. Add a child GameObject "LoadingRoot" and wire Inspector fields:
///
///      loadingRoot     – the "LoadingRoot" child (starts hidden)
///      canvasGroup     – CanvasGroup on LoadingRoot (controls fade alpha)
///      progressBar     – Slider inside LoadingRoot (Min 0, Max 1)
///      progressText    – Text label next to the bar  [optional]
///      backgroundImage – RawImage filling LoadingRoot (pixel-art RPG art)
///                        Set its texture to tile (Wrap Mode: Repeat) for
///                        the UV-scroll parallax effect.
///      fadeDuration    – Seconds for fade-in / fade-out  (default 0.35 s)
///      scrollSpeed     – UV scroll rate of background   (default 0.02)
///
/// ── Architecture note ────────────────────────────────────────────────────
///
///   EventBusUtil clears every event bus on scene unload to prevent stale
///   listener leaks. Because LoadingScreenUI is a persistent DontDestroyOnLoad
///   object it re-registers its bindings in OnSceneLoaded, which fires after
///   the new scene is active and the buses have been cleared.
///
///   Timing guarantee:
///     sceneUnloaded  → EventBusUtil clears buses
///     sceneLoaded    → LoadingScreenUI re-registers         ← before…
///     SceneLoadCompletedEvent raised by SceneLoader         ← …this
///   So the hide signal is always received correctly.
///
/// ── EventBus events consumed ─────────────────────────────────────────────
///   SceneLoadStartedEvent   – fade-in; resets progress bar
///   SceneLoadProgressEvent  – updates bar value + label each frame
///   SceneLoadCompletedEvent – fade-out; deactivates loading root
/// </summary>
public class LoadingScreenUI : MonoBehaviour
{
    public static LoadingScreenUI Instance { get; private set; }

    // ── Inspector fields ──────────────────────────────────────────────────

    [Header("Root")]
    [Tooltip("Child GameObject containing all loading-screen visuals. " +
             "Activated on load start, deactivated after fade-out.")]
    [SerializeField] private GameObject loadingRoot;

    [Header("Animation")]
    [Tooltip("CanvasGroup on loadingRoot used to fade alpha 0 ↔ 1.")]
    [SerializeField] private CanvasGroup canvasGroup;
    [Tooltip("Duration in seconds of the fade-in and fade-out transitions.")]
    [SerializeField] private float fadeDuration = 0.35f;

    [Header("Progress")]
    [Tooltip("Slider (Min 0, Max 1) that mirrors SceneLoadProgressEvent.Progress.")]
    [SerializeField] private Slider progressBar;
    [Tooltip("Optional Text label that shows '0%' → '100%'.")]
    [SerializeField] private Text progressText;

    [Header("Background Scroll")]
    [Tooltip("RawImage showing the pixel-art RPG environment. " +
             "Texture Wrap Mode must be set to Repeat for UV scrolling.")]
    [SerializeField] private RawImage backgroundImage;
    [Tooltip("Horizontal UV scroll speed of the background image.")]
    [SerializeField] [Range(0f, 0.1f)] private float scrollSpeed = 0.02f;

    // ── Private state ─────────────────────────────────────────────────────

    private EventBinding<SceneLoadStartedEvent>   _onLoadStarted;
    private EventBinding<SceneLoadProgressEvent>  _onLoadProgress;
    private EventBinding<SceneLoadCompletedEvent> _onLoadCompleted;

    private Coroutine _fadeCoroutine;
    private Coroutine _scrollCoroutine;

    // ── Unity lifecycle ───────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Start fully hidden
        if (loadingRoot != null)
            loadingRoot.SetActive(false);
    }

    private void OnEnable()
    {
        RegisterBindings();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        DeregisterBindings();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ── Scene lifecycle ───────────────────────────────────────────────────

    /// <summary>
    /// Re-creates and re-registers event bindings after each scene load.
    /// EventBusUtil clears all buses on scene unload, so persistent objects
    /// must re-subscribe once the new scene is active.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        DeregisterBindings();
        RegisterBindings();
    }

    // ── Event registration ────────────────────────────────────────────────

    private void RegisterBindings()
    {
        _onLoadStarted   = new EventBinding<SceneLoadStartedEvent>(HandleLoadStarted);
        _onLoadProgress  = new EventBinding<SceneLoadProgressEvent>(HandleLoadProgress);
        _onLoadCompleted = new EventBinding<SceneLoadCompletedEvent>(HandleLoadCompleted);

        EventBus<SceneLoadStartedEvent>.Register(_onLoadStarted);
        EventBus<SceneLoadProgressEvent>.Register(_onLoadProgress);
        EventBus<SceneLoadCompletedEvent>.Register(_onLoadCompleted);
    }

    private void DeregisterBindings()
    {
        EventBus<SceneLoadStartedEvent>.Deregister(_onLoadStarted);
        EventBus<SceneLoadProgressEvent>.Deregister(_onLoadProgress);
        EventBus<SceneLoadCompletedEvent>.Deregister(_onLoadCompleted);
    }

    // ── Event handlers ────────────────────────────────────────────────────

    private void HandleLoadStarted(SceneLoadStartedEvent e)
    {
        ResetProgress();
        Show();
    }

    private void HandleLoadProgress(SceneLoadProgressEvent e)
    {
        if (progressBar != null)
            progressBar.value = e.Progress;

        if (progressText != null)
            progressText.text = $"{Mathf.RoundToInt(e.Progress * 100)}%";
    }

    private void HandleLoadCompleted(SceneLoadCompletedEvent e)
    {
        Hide();
    }

    // ── Show / Hide ───────────────────────────────────────────────────────

    private void Show()
    {
        if (loadingRoot != null)
            loadingRoot.SetActive(true);

        RestartCoroutine(ref _scrollCoroutine, ScrollBackground());
        RestartCoroutine(ref _fadeCoroutine,   FadeTo(1f));
    }

    private void Hide()
    {
        RestartCoroutine(ref _fadeCoroutine, FadeOutThenDeactivate());
    }

    // ── Coroutines ────────────────────────────────────────────────────────

    private IEnumerator FadeTo(float target)
    {
        if (canvasGroup == null) yield break;

        float start   = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed          += Time.unscaledDeltaTime;   // unscaled: works when time is paused
            canvasGroup.alpha = Mathf.Lerp(start, target, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = target;
    }

    private IEnumerator FadeOutThenDeactivate()
    {
        // Run fade-out to completion before deactivating
        yield return FadeTo(0f);

        // Stop background scroll once invisible
        if (_scrollCoroutine != null)
        {
            StopCoroutine(_scrollCoroutine);
            _scrollCoroutine = null;
        }

        if (loadingRoot != null)
            loadingRoot.SetActive(false);
    }

    /// <summary>
    /// Slowly scrolls the background RawImage horizontally via UV offset,
    /// creating a low-cost parallax effect without moving any transforms.
    /// </summary>
    private IEnumerator ScrollBackground()
    {
        if (backgroundImage == null) yield break;

        while (true)
        {
            Rect r = backgroundImage.uvRect;
            r.x   += scrollSpeed * Time.unscaledDeltaTime;
            backgroundImage.uvRect = r;
            yield return null;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private void ResetProgress()
    {
        if (progressBar != null)
            progressBar.value = 0f;

        if (progressText != null)
            progressText.text = "0%";
    }

    /// <summary>
    /// Stops any running coroutine in <paramref name="slot"/>, starts
    /// <paramref name="routine"/>, and updates the slot reference.
    /// Using a ref slot prevents dangling Coroutine handles.
    /// </summary>
    private void RestartCoroutine(ref Coroutine slot, IEnumerator routine)
    {
        if (slot != null)
            StopCoroutine(slot);

        slot = StartCoroutine(routine);
    }
}
