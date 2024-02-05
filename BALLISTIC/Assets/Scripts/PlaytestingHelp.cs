using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlaytestingHelp : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // Check if the "R" key is pressed
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Reload the current scene
            ReloadScene();
        }
    }

    void ReloadScene()
    {
        // Get the current active scene
        Scene currentScene = SceneManager.GetActiveScene();

        // Reload the current scene by its name
        SceneManager.LoadScene(currentScene.name);
    }
}
