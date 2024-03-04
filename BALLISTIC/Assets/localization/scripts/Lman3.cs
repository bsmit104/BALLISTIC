using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;

public class Lman3 : MonoBehaviour
{
    private bool active = false;
    public TMP_Dropdown languageDropdown;

    private void Start()
    {
        // Subscribe to the dropdown value changed event
        languageDropdown.onValueChanged.AddListener(ChangeLocale);
    }

    public void ChangeLocale(int localeID)
    {
        if (active)
            return;

        StartCoroutine(SetLocale(localeID));
    }

    IEnumerator SetLocale(int _localID)
    {
        active = true;
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[_localID];
        active = false;
    }
}