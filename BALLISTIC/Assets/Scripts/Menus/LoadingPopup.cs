using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float dotDuration;
    [SerializeField] private int dotCount;

    private float timer = 0;
    private float dotCounter = 0;

    void Awake()
    {
        text.text = "Loading";
    }

    void Update()
    {
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
