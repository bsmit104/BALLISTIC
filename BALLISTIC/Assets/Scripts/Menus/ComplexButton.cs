using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComplexButton : MonoBehaviour
{
    public Color onHoverColor;
    public Color onClickColor;

    [SerializeField] private Image[] listeners;
    private Color[] listenerOriginalColors;

    private bool isHovering = false;
    private bool isClicking = false;

    void Awake()
    {
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
