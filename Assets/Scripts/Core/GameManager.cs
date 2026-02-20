using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { MainMenu, Loading, InGame, Paused, GameOver }
    public GameState CurrentState { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"[GameManager] State: {newState}");
    }
}