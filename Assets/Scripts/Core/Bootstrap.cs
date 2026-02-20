using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    void Start()
    {
        SceneLoader.Instance.LoadScene("MainMenu");
    }
}