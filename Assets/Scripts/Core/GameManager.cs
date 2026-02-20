using UnityEngine;
using DarkfallOnline.Events;

/// <summary>
/// Central game state machine. Persists across all scenes via DontDestroyOnLoad.
///
/// Bug fixes applied:
///   - Duplicate-instance check now compares Instance != this so a surviving
///     DontDestroyOnLoad instance is never self-destroyed on scene reload.
///   - SetState is a no-op when the requested state equals the current state,
///     preventing redundant log spam and unnecessary EventBus raises.
///   - PreviousState is tracked so any listener can react to the full transition
///     (e.g. InGame → Paused vs. MainMenu → Paused).
///
/// EventBus integration:
///   - Raises GameStateChangedEvent on every successful state transition.
///     Listeners subscribe via:
///       var b = new EventBinding&lt;GameStateChangedEvent&gt;(OnStateChanged);
///       EventBus&lt;GameStateChangedEvent&gt;.Register(b);
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        MainMenu,
        Loading,
        InGame,
        Paused,
        GameOver,
    }

    /// <summary>The current game state.</summary>
    public GameState CurrentState  { get; private set; } = GameState.MainMenu;

    /// <summary>
    /// The state that was active immediately before the most recent transition.
    /// Equals CurrentState on first run (no previous transition has occurred).
    /// </summary>
    public GameState PreviousState { get; private set; } = GameState.MainMenu;

    // ── Convenience properties ────────────────────────────────────────────

    /// <summary>True while the player is actively in a gameplay scene.</summary>
    public bool IsInGame  => CurrentState == GameState.InGame;

    /// <summary>True while the game is paused.</summary>
    public bool IsPaused  => CurrentState == GameState.Paused;

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
    /// Transitions to <paramref name="newState"/> and raises
    /// <see cref="GameStateChangedEvent"/> through the EventBus.
    ///
    /// Does nothing (and raises no event) if <paramref name="newState"/>
    /// already equals <see cref="CurrentState"/>.
    /// </summary>
    public void SetState(GameState newState)
    {
        if (CurrentState == newState) return;

        PreviousState = CurrentState;
        CurrentState  = newState;

        Debug.Log($"[GameManager] State: {PreviousState} → {CurrentState}");

        EventBus<GameStateChangedEvent>.Raise(new GameStateChangedEvent
        {
            PreviousState = PreviousState,
            NewState      = CurrentState,
        });
    }
}
