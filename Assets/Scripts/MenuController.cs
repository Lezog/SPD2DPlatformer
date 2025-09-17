using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject SettingsMenu;
    [SerializeField] private GameObject MainMenu;
    public void StartGame()
    {
        SceneFade.Instance.FadeToScene("StartLevel");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Settings()
    {
        SettingsMenu.SetActive(true);
        MainMenu.SetActive(false);
    }

    public void Back()
    {
        SettingsMenu.SetActive(false);
        MainMenu.SetActive(true);
    }
}
