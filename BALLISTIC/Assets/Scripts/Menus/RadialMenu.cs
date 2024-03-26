using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Use to procedurally create a radial menu interface.
/// </summary>
public class RadialMenu : MonoBehaviour
{
    [Tooltip("The prefab instantiated for each menu item.")]
    [SerializeField] private TextMeshProUGUI menuElement;
    [SerializeField] private float onHoverScaleIncrease;

    /// <summary>
    /// Returns true if the menu is currently open.
    /// </summary>
    public bool IsOpen { get { return isOpen; } }
    private bool isOpen = false;

    /// <summary>
    /// Opens the radial menu.
    /// </summary>
    public void OpenMenu()
    {
        gameObject.SetActive(true);
        isOpen = true;
    }

    /// <summary>
    /// Closes the radial menu.
    /// </summary>
    public void CloseMenu()
    {
        gameObject.SetActive(false);
        isOpen = false;
        PublishSelection();

        foreach (var option in options)
        {
            option.transform.localScale = Vector3.one;
        }
    }

    private List<TextMeshProUGUI> options = new List<TextMeshProUGUI>();

    /// <summary>
    /// Adds a new option to the menu.
    /// </summary>
    public void AddOption(string option)
    {
        // Add new option element
        var newOption = Instantiate(menuElement, transform);
        newOption.text = option;
        options.Add(newOption);
        int ind = options.Count - 1;
        UpdateOptionPositions();

        Vector3 originalScale = newOption.transform.localScale;

        // Set event listeners
        var trigger = newOption.GetComponent<EventTrigger>();

        // On hover
        EventTrigger.Entry onHover = new EventTrigger.Entry();
        onHover.eventID = EventTriggerType.PointerEnter;
        onHover.callback.AddListener( (data) => { 
            SetCurrentOption(ind);
            newOption.transform.localScale = originalScale + (Vector3.one * onHoverScaleIncrease);
        } );
        trigger.triggers.Add(onHover);

        // On hover exit
        EventTrigger.Entry onExit = new EventTrigger.Entry();
        onExit.eventID = EventTriggerType.PointerExit;
        onExit.callback.AddListener( (data) => { 
            if (currentOption == ind) SetCurrentOption(-1); 
            newOption.transform.localScale = originalScale;
        } );
        trigger.triggers.Add(onExit);
    }

    private void UpdateOptionPositions()
    {
        float delta = 360f / options.Count;
        for (int i = 0; i < options.Count; i++) 
        {
            options[i].GetComponent<RectTransform>().anchoredPosition = Rotate(new Vector2(0, 250f), delta * i);
        }
    }

    // Helper function for cursed code above
    Vector2 Rotate(Vector2 v, float angle)
    {
        return new Vector2(
            v.x * Mathf.Cos(angle * Mathf.Deg2Rad) - v.y * Mathf.Sin(angle * Mathf.Deg2Rad),
            v.x * Mathf.Sin(angle * Mathf.Deg2Rad) + v.y * Mathf.Cos(angle * Mathf.Deg2Rad)
        );
    }

    /// <summary>
    /// Returns the current option the player is hovering over. 
    /// If the player isn't hovering over anything, returns an empty string.
    /// </summary>
    public string CurrentOption 
    { 
        get 
        { 
            if (currentOption == -1) return "";
            return options[currentOption].text; 
        } 
    }
    private int currentOption = -1;

    private void SetCurrentOption(int optionInd)
    {
        currentOption = optionInd;
    }

    public delegate void OptionSelection(string option);

    /// <summary>
    /// Event triggered when the menu is closed with an option selected.
    /// </summary>
    public event OptionSelection OnOptionSelected;

    private void PublishSelection()
    {
        if (currentOption != -1)
        {
            OnOptionSelected?.Invoke(CurrentOption);
        }
        currentOption = -1;
    }
}
