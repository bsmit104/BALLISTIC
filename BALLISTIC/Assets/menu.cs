using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class menu : MonoBehaviour
{
    public void Play() {
        SceneManager.LoadScene("PlayMenu");
    }
    public void Setting() {
        SceneManager.LoadScene("SettingMenu");
    }
    public void Tutorial() {
        SceneManager.LoadScene("Tutorial");
    }
    public void Back() {
        SceneManager.LoadScene("MainMenu");
    }
}
