using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DarkfallOnline.Events;

/// <summary>
/// Handles asynchronous scene transitions. Persists across scenes via
/// DontDestroyOnLoad.
///
/// Bug fixes applied:
///   - isLoading / IsLoading guard: a second LoadScene call while a load is
///     already in progress is safely ignored with a warning instead of
///     starting a second coroutine that would corrupt state.
///   - Scene name validated (non-null, non-whitespace) before starting the
///     coroutine so the error is caught at the call site.
///   - GameManager.Instance null-checked before every SetState call; missing
///     GameManager produces a warning, not a NullReferenceException.
///   - AsyncOperation null-checked: SceneManager.LoadSceneAsync returns null
///     when the scene is not in Build Settings. Previously this caused an
///     immediate NullReferenceException. Now a SceneLoadFailedEvent is raised
///     and the loader resets cleanly.
///   - allowSceneActivation = false used to separate the 0→0.9 progress phase
///     from the activation step, enabling accurate per-frame progress events.
///   - Post-load game state determined by scene name and applied correctly so
///     the state is never stuck on "Loading" after a transition.
///
/// EventBus events raised:
///   SceneLoadStartedEvent   – immediately when load is accepted
///   SceneLoadProgressEvent  – each frame during load (Progress 0→1)
///   SceneLoadCompletedEvent – after scene is active and state restored
///   SceneLoadFailedEvent    – if the load cannot be started or completed
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    /// <summary>True while an async scene load is in progress.</summary>
    public bool IsLoading { get; private set; }

    // ── Unity lifecycle ───────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>
    /// Begins an async load of <paramref name="sceneName"/>.
    ///
    /// Silently (with a warning) ignores the request if a load is already in
    /// progress. Callers should check <see cref="IsLoading"/> before calling
    /// if they need more control.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (IsLoading)
        {
            Debug.LogWarning(
                $"[SceneLoader] Load of '{sceneName}' ignored – " +
                "a scene is already being loaded.");
            return;
        }

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[SceneLoader] LoadScene called with a null or empty scene name.");
            return;
        }

        StartCoroutine(LoadAsync(sceneName));
    }

    // ── Private coroutine ─────────────────────────────────────────────────

    IEnumerator LoadAsync(string sceneName)
    {
        IsLoading = true;

        // ── 1. Notify: load started ───────────────────────────────────────
        EventBus<SceneLoadStartedEvent>.Raise(new SceneLoadStartedEvent
        {
            SceneName = sceneName,
        });

        // Update game state to Loading (null-safe)
        if (GameManager.Instance != null)
            GameManager.Instance.SetState(GameManager.GameState.Loading);
        else
            Debug.LogWarning("[SceneLoader] GameManager.Instance is null; " +
                             "cannot apply Loading state.");

        // ── 2. Start async load ───────────────────────────────────────────
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        if (op == null)
        {
            // Scene is likely missing from Build Settings
            string reason =
                $"SceneManager returned null for '{sceneName}'. " +
                "Verify the scene is added to File > Build Settings.";

            Debug.LogError($"[SceneLoader] {reason}");

            EventBus<SceneLoadFailedEvent>.Raise(new SceneLoadFailedEvent
            {
                SceneName = sceneName,
                Reason    = reason,
            });

            // Restore previous state so the game is not stuck on Loading
            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameManager.Instance.PreviousState);

            IsLoading = false;
            yield break;
        }

        // Prevent Unity from activating the scene the moment it hits 0.9.
        // We want to fire a final progress event at 1.0 first.
        op.allowSceneActivation = false;

        // ── 3. Report progress (0 → ~0.9 in Unity's scale) ───────────────
        while (op.progress < 0.9f)
        {
            // Unity's AsyncOperation.progress goes 0→0.9 while loading,
            // then jumps to 1.0 only after activation. We remap to 0→1.
            float normalized = Mathf.Clamp01(op.progress / 0.9f);

            EventBus<SceneLoadProgressEvent>.Raise(new SceneLoadProgressEvent
            {
                SceneName = sceneName,
                Progress  = normalized,
            });

            yield return null;
        }

        // Fire a 100 % progress event before activating the scene
        EventBus<SceneLoadProgressEvent>.Raise(new SceneLoadProgressEvent
        {
            SceneName = sceneName,
            Progress  = 1f,
        });

        // ── 4. Activate the scene ─────────────────────────────────────────
        op.allowSceneActivation = true;

        while (!op.isDone)
            yield return null;

        // ── 5. Apply post-load game state ─────────────────────────────────
        // Determine the correct state based on which scene was loaded.
        // Extend this mapping as new scenes are added.
        var postLoadState = sceneName == "GameScene"
            ? GameManager.GameState.InGame
            : GameManager.GameState.MainMenu;

        if (GameManager.Instance != null)
            GameManager.Instance.SetState(postLoadState);

        IsLoading = false;

        // ── 6. Notify: load completed ─────────────────────────────────────
        EventBus<SceneLoadCompletedEvent>.Raise(new SceneLoadCompletedEvent
        {
            SceneName = sceneName,
        });

        Debug.Log($"[SceneLoader] '{sceneName}' loaded and active.");
    }
}
