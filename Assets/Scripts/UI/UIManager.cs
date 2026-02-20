using UnityEngine;
using DarkfallOnline.Events;

/// <summary>
/// Centralized UI panel manager. Persists across scenes via DontDestroyOnLoad.
///
/// Bug fixes applied:
///   - Both ShowPanel and HidePanel null-check the incoming GameObject before
///     calling SetActive, replacing the original NullReferenceException with
///     a descriptive error log.
///   - Duplicate-instance guard uses Instance != this to avoid self-destruction
///     when the same DontDestroyOnLoad object re-enters Awake on scene reload.
///
/// EventBus integration:
///   - ShowPanel raises UIPanelShownEvent.
///   - HidePanel raises UIPanelHiddenEvent.
///   Listeners (audio, analytics, tutorial systems, etc.) can react to these
///   without coupling to UIManager.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

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
    /// Makes <paramref name="panel"/> visible and raises
    /// <see cref="UIPanelShownEvent"/>.
    /// </summary>
    public void ShowPanel(GameObject panel)
    {
        if (panel == null)
        {
            Debug.LogError("[UIManager] ShowPanel called with a null panel reference.");
            return;
        }

        panel.SetActive(true);

        EventBus<UIPanelShownEvent>.Raise(new UIPanelShownEvent { Panel = panel });
    }

    /// <summary>
    /// Hides <paramref name="panel"/> and raises
    /// <see cref="UIPanelHiddenEvent"/>.
    /// </summary>
    public void HidePanel(GameObject panel)
    {
        if (panel == null)
        {
            Debug.LogError("[UIManager] HidePanel called with a null panel reference.");
            return;
        }

        panel.SetActive(false);

        EventBus<UIPanelHiddenEvent>.Raise(new UIPanelHiddenEvent { Panel = panel });
    }
}
