using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Analytics : MonoBehaviour
{
    [Tooltip("Press 'Q' to trigger analytics events manually.")]
    [SerializeField] bool allowCheatKeys = false;
    [Tooltip("Select to write out current analytics to 'analytics.txt' file.")]
    [SerializeField] bool writeToFile = false;
    [Space]
    [Space]
    [Header("Analytics")]
    [Tooltip("Time player spends in main menu before entering lobby.")]
    [SerializeField] float timeSpentInMenu;
    [Tooltip("Average play time per level. Should be about 2 mins.")]
    [SerializeField] float avgTimeSpentPerLevel;
    [Tooltip("Number of levels that have been played.")]
    [SerializeField] int levelsPlayed;

    float lastLevelChange = 0;
    float timeAtGameStart = 0;

    int prevSceneIndex;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        timeAtGameStart = Time.time;
    }

    void Update()
    {
        if (allowCheatKeys)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (timeSpentInMenu == 0)
                {
                    OnEnterLobby();
                }
                else
                {
                    OnLevelChanged();
                }
            }
        }

        if (writeToFile)
        {
            writeToFile = false;

            string output = ToString();

            File.WriteAllText(Application.dataPath + "/analytics.txt", output);
        }
    }

    public override string ToString()
    {
        string output = "Analytics at " + Time.time.ToString() + ":\n";
        output += "     Time Spent In Main Menu: " + timeSpentInMenu + " secs\n";
        output += "     Avg Time Spent Per Level: " + avgTimeSpentPerLevel + " secs\n";
        output += "     Levels Played: " + levelsPlayed + " levels\n";
        return output;
    }

    void FixedUpdate()
    {
        if (SceneManager.GetActiveScene().buildIndex != prevSceneIndex)
        {
            if (timeSpentInMenu == 0)
            {
                OnEnterLobby();
            }
            else
            {
                OnLevelChanged();
            }
        }
    }

    void OnEnterLobby()
    {
        timeSpentInMenu = Time.time - timeAtGameStart;
        prevSceneIndex = SceneManager.GetActiveScene().buildIndex;
        lastLevelChange = Time.time;
    }

    void OnLevelChanged()
    {
        levelsPlayed++;
        avgTimeSpentPerLevel =(avgTimeSpentPerLevel + (Time.time - lastLevelChange)) / levelsPlayed;
        prevSceneIndex = SceneManager.GetActiveScene().buildIndex;
        lastLevelChange = Time.time;
    }
}
