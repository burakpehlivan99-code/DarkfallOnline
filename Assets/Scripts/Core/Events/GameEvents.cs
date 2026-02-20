using UnityEngine;

namespace DarkfallOnline.Events
{
    // ═══════════════════════════════════════════════════════════════════════
    //  GAME STATE EVENTS
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Raised by GameManager every time the game state changes.
    /// Carries both the previous and the new state so handlers can
    /// react to specific transitions (e.g. InGame → Paused).
    /// </summary>
    public struct GameStateChangedEvent : IEvent
    {
        public GameManager.GameState PreviousState;
        public GameManager.GameState NewState;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  SCENE LOADING EVENTS
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Raised the instant a scene load request is accepted by SceneLoader.
    /// Use this to show a loading screen or disable gameplay input.
    /// </summary>
    public struct SceneLoadStartedEvent : IEvent
    {
        public string SceneName;
    }

    /// <summary>
    /// Raised each frame while an async scene load is in progress.
    /// Progress is normalised to 0–1 (Unity's raw 0–0.9 range is remapped).
    /// </summary>
    public struct SceneLoadProgressEvent : IEvent
    {
        public string SceneName;
        /// <summary>Normalised progress 0 (just started) → 1 (load complete).</summary>
        public float Progress;
    }

    /// <summary>
    /// Raised after the loaded scene is active and the post-load game state
    /// has been applied. Use this to hide the loading screen.
    /// </summary>
    public struct SceneLoadCompletedEvent : IEvent
    {
        public string SceneName;
    }

    /// <summary>
    /// Raised when SceneLoader cannot start or complete a load.
    /// Reason contains a human-readable explanation (e.g., scene not in Build Settings).
    /// </summary>
    public struct SceneLoadFailedEvent : IEvent
    {
        public string SceneName;
        public string Reason;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  UI EVENTS
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>Raised by UIManager when a panel becomes visible.</summary>
    public struct UIPanelShownEvent : IEvent
    {
        public GameObject Panel;
    }

    /// <summary>Raised by UIManager when a panel is hidden.</summary>
    public struct UIPanelHiddenEvent : IEvent
    {
        public GameObject Panel;
    }

    /// <summary>Raised when the user clicks Play on the main menu.</summary>
    public struct PlayRequestedEvent : IEvent { }

    /// <summary>Raised when the user opens the Settings screen.</summary>
    public struct SettingsOpenedEvent : IEvent { }

    /// <summary>Raised when the user closes the Settings screen.</summary>
    public struct SettingsClosedEvent : IEvent { }

    /// <summary>Raised when the user clicks Quit.</summary>
    public struct QuitRequestedEvent : IEvent { }

    // ═══════════════════════════════════════════════════════════════════════
    //  PLAYER EVENTS  (stubs – full implementation in a later sprint)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>Raised when a player character's HP reaches zero.</summary>
    public struct PlayerDiedEvent : IEvent
    {
        public string PlayerName;
        public int    PlayerId;
    }

    /// <summary>Raised when a downed player is revived by an ally.</summary>
    public struct PlayerRevivedEvent : IEvent
    {
        public string PlayerName;
        public int    PlayerId;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  COMBAT EVENTS  (stubs – full implementation in a later sprint)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>Raised whenever any entity takes damage.</summary>
    public struct DamageTakenEvent : IEvent
    {
        public GameObject Target;
        public float      Amount;
        public bool       IsCritical;
    }

    /// <summary>Raised when an enemy is defeated (HP ≤ 0).</summary>
    public struct EnemyDefeatedEvent : IEvent
    {
        public GameObject Enemy;
        public Vector3    Position;
    }
}
