using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script used to control and save player settings.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    /// <summary>
    /// The global instance of the Settings Manager.
    /// </summary>
    public static SettingsManager Instance { get { return _instance; } }
    private static SettingsManager _instance;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        DontDestroyOnLoad(gameObject);

        CloseMenu();
    }

    // * Defaults ======================================

    [Header("Default Settings")]
    [SerializeField, Range(0f, 1f)] private float defaultVolume = 1f;

    const float MIN_SENSE = 50f;
    const float MAX_SENSE = 800f;
    const float BASE_SENSE = 300f;
    [SerializeField, Range(MIN_SENSE, MAX_SENSE)] private float defaultSensitivity = BASE_SENSE;

    // * ===============================================

    // * Getters & Setters =============================

    /// <summary>
    /// Music and SFX volume. Setting is actually applied in SoundManager.
    /// </summary>
    public static float Volume {
        get {
            if (_volume == -1f)
            {
                _volume = PlayerPrefs.GetFloat("Volume", -1f);
                if (_volume == -1f)
                {
                    _volume = Instance?.defaultVolume ?? 1f;
                    PlayerPrefs.SetFloat("Volume", _volume);
                }
            }

            return _volume;
        }
        set {
            float newVolume = Mathf.Clamp(value, 0f, 1f);
            PlayerPrefs.SetFloat("Volume", newVolume);
            _volume = newVolume;
        }
    }
    private static float _volume = -1f;

    /// <summary>
    /// Mouse sensitivity for the local player. Applies value to local player.
    /// </summary>
    public static float Sensitivity {
        get {
            if (_sensitivity == 0f)
            {
                _sensitivity = PlayerPrefs.GetFloat("Sensitivity", 0f);
                if (_sensitivity == 0f)
                {
                    _sensitivity = Instance?.defaultSensitivity ?? BASE_SENSE;
                    PlayerPrefs.SetFloat("Sensitivity", _sensitivity);
                }
            }
            return _sensitivity;
        }
        set {
            float newSensitivity = Mathf.Clamp(value, MIN_SENSE, MAX_SENSE);
            PlayerPrefs.SetFloat("Sensitivity", newSensitivity);
            _sensitivity = newSensitivity;
            if (NetworkPlayer.Local)
            {
                NetworkPlayer.Local.Sensitivity = _sensitivity;
            }
        }
    }
    private static float _sensitivity = 0f;

    /// <summary>
    /// Sensitivity based on a normalized 0 to 1 scale.
    /// </summary>
    public static float SenseNormalized {
        get { return (Sensitivity - MIN_SENSE) / (MAX_SENSE - MIN_SENSE); }
        set {
            float newSense = Mathf.Clamp(value, 0f, 1f);
            Sensitivity = (newSense * (MAX_SENSE - MIN_SENSE)) + MIN_SENSE;
        }
    }

    // * ===============================================

    // * Menu ==========================================

    [Space]
    [Header("Settings Menu")]
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sensitivitySlider;

    // Save settings if player chooses to cancel changes
    private float prevVolume;
    private float prevSense;

    // Menu events
    public event Notify OnMenuOpened;
    public event Notify OnMenuClosed;

    public bool MenuOpen { get { return _menuOpen;} }
    private bool _menuOpen = false;

    public void OpenMenu()
    {
        // Init values
        volumeSlider.value = Volume;
        sensitivitySlider.value = SenseNormalized;

        // Save original values
        prevVolume = Volume;
        prevSense = SenseNormalized;

        // Open menu
        menuCanvas.enabled = true;
        _menuOpen = true;

        OnMenuOpened?.Invoke();
    }

    public void CancelChanges()
    {
        Volume = prevVolume;
        SenseNormalized = prevSense;
        CloseMenu();
    }

    public void CloseMenu()
    {
        menuCanvas.enabled = false;
        _menuOpen = false;
        OnMenuClosed?.Invoke();
    }

    public void UpdateVolume()
    {
        Volume = volumeSlider.value;
    }

    public void UpdateSense()
    {
        SenseNormalized = sensitivitySlider.value;
    }

    // * ===============================================
}
