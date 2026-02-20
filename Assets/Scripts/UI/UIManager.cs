using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ShowPanel(GameObject panel) => panel.SetActive(true);
    public void HidePanel(GameObject panel) => panel.SetActive(false);
}