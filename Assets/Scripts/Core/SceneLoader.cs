using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadAsync(sceneName));
    }

    IEnumerator LoadAsync(string sceneName)
    {
        GameManager.Instance.SetState(GameManager.GameState.Loading);
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;
    }
}