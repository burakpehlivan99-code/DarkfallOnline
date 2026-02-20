using UnityEngine;

/// <summary>
/// Entry point of the game. Lives only in the Bootstrap scene, which is
/// always scene index 0 in Build Settings.
///
/// Bug fixes applied:
///   - SceneLoader.Instance is null-checked before use; a descriptive error
///     is logged instead of a cryptic NullReferenceException.
///   - firstScene is validated (non-null, non-whitespace) before the call.
///   - The target scene name is serialized so it can be changed from the
///     Inspector without touching code.
/// </summary>
public class Bootstrap : MonoBehaviour
{
    [Tooltip("Name of the scene to transition to immediately after Bootstrap " +
             "initializes. Must match a scene name in Build Settings exactly.")]
    [SerializeField] private string firstScene = "MainMenu";

    void Start()
    {
        if (SceneLoader.Instance == null)
        {
            Debug.LogError(
                "[Bootstrap] SceneLoader.Instance is null. " +
                "Ensure a SceneLoader component exists in the Bootstrap scene " +
                "and that its script executes before Bootstrap " +
                "(check Script Execution Order in Project Settings).");
            return;
        }

        if (string.IsNullOrWhiteSpace(firstScene))
        {
            Debug.LogError(
                "[Bootstrap] 'firstScene' is empty. " +
                "Assign a valid scene name in the Bootstrap GameObject's Inspector.");
            return;
        }

        SceneLoader.Instance.LoadScene(firstScene);
    }
}
