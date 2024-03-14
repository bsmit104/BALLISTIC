using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Instantiated to show players the game is currently loading something.
/// Only instantiate this if loading consistently takes a long time, and it will result in a scene change.
/// </summary>
public class LoadingPopup : MonoBehaviour
{
    [Tooltip("Displays the animated loading text.")]
    [SerializeField] private TextMeshProUGUI text;
    [Tooltip("The duration between dots '.' being added to text.")]
    [SerializeField] private float dotDuration;
    [Tooltip("The max number of dots '.' that will be added before looping back to 0.")]
    [SerializeField] private int dotCount;

    private float timer = 0;
    private float dotCounter = 0;

    void Awake()
    {
        text.text = "Loading";
    }

    void Update()
    {
        // Add the dots "." to the loading string to create an animated loading effect
        if (timer <= 0)
        {
            text.text += " .";
            timer = dotDuration;
            dotCounter++;
            if (dotCounter == dotCount + 1)
            {
                text.text = "Loading";
                dotCounter = 0;
            }
        }
        timer -= Time.deltaTime;
    }
}
