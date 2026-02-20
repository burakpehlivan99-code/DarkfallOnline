using UnityEngine;
using UnityEngine.UI;
using DarkfallOnline.Events;

/// <summary>
/// Drives the Main Menu screen.
///
/// Bug fixes applied:
///   - All [SerializeField] references are validated in a dedicated
///     ValidateReferences() method at Start(); missing references produce
///     descriptive errors and abort initialization safely instead of causing
///     NullReferenceExceptions scattered across click handlers.
///   - SceneLoader.Instance null-checked in OnPlayClicked() before use.
///   - Panel transitions (show/hide) route through UIManager when available
///     so that UIPanelShown/HiddenEvents are raised correctly.
///
/// EventBus events raised:
///   PlayRequestedEvent    – when the Play button is clicked
///   SettingsOpenedEvent   – when the Settings button is clicked
///   SettingsClosedEvent   – when settings are closed (Back button)
///   QuitRequestedEvent    – when the Quit button is clicked
///
/// EventBus subscriptions: none (this class only raises events).
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;

    // ── Unity lifecycle ───────────────────────────────────────────────────

    void Start()
    {
        if (!ValidateReferences()) return;

        playButton.onClick.AddListener(OnPlayClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        // Ensure correct initial panel state
        SetPanel(mainPanel,    visible: true);
        SetPanel(settingsPanel, visible: false);
    }

    void OnDestroy()
    {
        // Defensive cleanup so listeners are not invoked on destroyed UI
        if (playButton != null)     playButton.onClick.RemoveListener(OnPlayClicked);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(OnSettingsClicked);
        if (quitButton != null)     quitButton.onClick.RemoveListener(OnQuitClicked);
    }

    // ── Button handlers ───────────────────────────────────────────────────

    void OnPlayClicked()
    {
        EventBus<PlayRequestedEvent>.Raise(new PlayRequestedEvent());

        if (SceneLoader.Instance == null)
        {
            Debug.LogError(
                "[MainMenuUI] SceneLoader.Instance is null. " +
                "Cannot transition to GameScene.");
            return;
        }

        SceneLoader.Instance.LoadScene("GameScene");
    }

    void OnSettingsClicked()
    {
        EventBus<SettingsOpenedEvent>.Raise(new SettingsOpenedEvent());

        SetPanel(mainPanel,    visible: false);
        SetPanel(settingsPanel, visible: true);
    }

    void OnQuitClicked()
    {
        EventBus<QuitRequestedEvent>.Raise(new QuitRequestedEvent());

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>
    /// Called by the "Back" button inside the settings panel to return to
    /// the main panel. May also be called from other scripts.
    /// </summary>
    public void CloseSettings()
    {
        EventBus<SettingsClosedEvent>.Raise(new SettingsClosedEvent());

        SetPanel(settingsPanel, visible: false);
        SetPanel(mainPanel,     visible: true);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Shows or hides <paramref name="panel"/> through UIManager when available
    /// (so that UI events are raised), otherwise falls back to SetActive directly.
    /// </summary>
    void SetPanel(GameObject panel, bool visible)
    {
        if (UIManager.Instance != null)
        {
            if (visible) UIManager.Instance.ShowPanel(panel);
            else         UIManager.Instance.HidePanel(panel);
        }
        else
        {
            panel.SetActive(visible);
        }
    }

    /// <summary>
    /// Validates all required Inspector references.
    /// Returns false (and logs errors) if any are missing.
    /// </summary>
    bool ValidateReferences()
    {
        bool valid = true;

        if (playButton == null)
        {
            Debug.LogError("[MainMenuUI] 'playButton' is not assigned in the Inspector.");
            valid = false;
        }
        if (settingsButton == null)
        {
            Debug.LogError("[MainMenuUI] 'settingsButton' is not assigned in the Inspector.");
            valid = false;
        }
        if (quitButton == null)
        {
            Debug.LogError("[MainMenuUI] 'quitButton' is not assigned in the Inspector.");
            valid = false;
        }
        if (mainPanel == null)
        {
            Debug.LogError("[MainMenuUI] 'mainPanel' is not assigned in the Inspector.");
            valid = false;
        }
        if (settingsPanel == null)
        {
            Debug.LogError("[MainMenuUI] 'settingsPanel' is not assigned in the Inspector.");
            valid = false;
        }

        return valid;
    }
}
