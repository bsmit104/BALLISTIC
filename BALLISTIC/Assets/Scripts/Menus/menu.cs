using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] private GameObject hostJoinCanvas;
    [SerializeField] private GameObject lobbyCodeCanvas;

    public void Play() {
        SceneManager.LoadScene("PlayMenu");
    }
    public void Setting() {
        SceneManager.LoadScene("SettingMenu");
        SettingsManager.Instance.OpenMenu();
        SettingsManager.Instance.OnMenuClosed -= Back;
        SettingsManager.Instance.OnMenuClosed += Back;
    }
    public void Tutorial() {
        SceneManager.LoadScene("Tutorial");
    }
    public void Back() {
        SceneManager.LoadScene("MainMenu");
    }

    public void EnterLobbyCode() 
    {
        hostJoinCanvas.SetActive(false);
        lobbyCodeCanvas.SetActive(true);
    }

    public void ExitLobbyCode() 
    {
        hostJoinCanvas.SetActive(true);
        lobbyCodeCanvas.SetActive(false);
    }
}
