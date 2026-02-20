using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;

    void Start()
    {
        playButton.onClick.AddListener(OnPlayClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    void OnPlayClicked()
    {
        SceneLoader.Instance.LoadScene("GameScene");
    }

    void OnSettingsClicked()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    void OnQuitClicked()
    {
        Application.Quit();
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }
}