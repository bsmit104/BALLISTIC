using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Used for buttons that have multiple images that should change color with 
/// hover and click events.
/// </summary>
public class ComplexButton : MonoBehaviour
{
    [Tooltip("The color listeners will be multiplied with on hover.")]
    public Color onHoverColor;
    [Tooltip("The color listeners will be multiplied with on click.")]
    public Color onClickColor;

    [Tooltip("The UI elements that should be colored.")]
    [SerializeField] private Image[] listeners;
    // Cache the original colors for each listener for exit events
    private Color[] listenerOriginalColors;

    private bool isHovering = false;
    private bool isClicking = false;

    void Awake()
    {
        // Cache original colors
        listenerOriginalColors = new Color[listeners.Length];
        for (int i = 0; i < listeners.Length; i++)
        {
            listenerOriginalColors[i] = listeners[i].color;
        }
    }

    public void OnHoverEnter()
    {
        isHovering = true;
        foreach (var listener in listeners)
        {
            listener.color *= onHoverColor;
        }
    }

    public void OnHoverExit()
    {
        isHovering = false;
        if (isClicking) return;
        for (int i = 0; i < listeners.Length; i++)
        {
            listeners[i].color = listenerOriginalColors[i];
        }
    }

    public void OnClickDown()
    {
        isClicking = true;
        foreach (var listener in listeners)
        {
            listener.color *= onClickColor;
        }
    }

    public void OnClickUp()
    {
        isClicking = false;
        if (isHovering)
        {
            // Set colors to on hover color if mouse is still on the button
            for (int i = 0; i < listeners.Length; i++)
            {
                listeners[i].color = listenerOriginalColors[i] * onHoverColor;
            }
        }
        else
        {
            for (int i = 0; i < listeners.Length; i++)
            {
                listeners[i].color = listenerOriginalColors[i];
            }
        }
    }
}
