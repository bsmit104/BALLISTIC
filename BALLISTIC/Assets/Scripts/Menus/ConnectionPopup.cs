using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/// <summary>
/// Instantiated to display disconnections. Should only be created by NetworkRunnerCallbacks.
/// </summary>
public class ConnectionPopup : MonoBehaviour
{
    [Tooltip("The text used to display disconnection messages.")]
    [SerializeField] private TextMeshProUGUI connectionStatusText;

    private bool closedPopup = false;

    /// <summary>
    /// Sets the text the popup will display.
    /// </summary>
    public void SetText(string text)
    {
        connectionStatusText.text = text;
    }

    void Awake()
    {
        // Unlock the cursor so the player can click the X button
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // prevent double event systems from being active in the scene
        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();

        if (eventSystems.Length > 1)
        {
            transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Return to play menu once the popup is closed
        if (closedPopup)
        {
            Debug.Log("returning to menu");
            SceneManager.LoadScene("PlayMenu");
            Destroy(gameObject);
        }

        // Check if local event system should be reactivated
        if (!transform.GetChild(1).gameObject.activeInHierarchy)
        {
            EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();

            if (eventSystems.Length <= 1)
            {
                transform.GetChild(1).gameObject.SetActive(true);
            }
        }
    }

    public void ClosePopup()
    {
        closedPopup = true;
    }
}
