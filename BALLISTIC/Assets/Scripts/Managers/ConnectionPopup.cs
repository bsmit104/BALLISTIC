using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class ConnectionPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI connectionStatusText;

    private bool closedPopup = false;

    public void SetText(string text)
    {
        connectionStatusText.text = text;
    }

    void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();

        if (eventSystems.Length > 1)
        {
            Destroy(transform.GetChild(1).gameObject);
        }
    }

    void Update()
    {
        if (closedPopup)
        {
            Debug.Log("returning to menu");
            SceneManager.LoadScene("PlayMenu");
            Destroy(gameObject);
        }
    }

    public void ClosePopup()
    {
        closedPopup = true;
        Debug.Log("closed popup");
    }
}
