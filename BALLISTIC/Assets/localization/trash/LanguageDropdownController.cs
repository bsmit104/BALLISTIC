// LanguageDropdownController.cs
using UnityEngine;
using TMPro;

public class LanguageDropdownController : MonoBehaviour
{
    public TMP_Dropdown dropdown;  // Use TMP_Dropdown for TextMeshPro Dropdown

    private void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();

        // Subscribe to the dropdown's OnValueChanged event
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    private void OnDropdownValueChanged(int index)
    {
        // Get the selected language from the dropdown options
        string selectedLanguage = dropdown.options[index].text;

        // Call the ChangeLanguage method in the LanguageManager
        LanguageManager.Instance.ChangeLanguage(selectedLanguage);
    }
}

// using UnityEngine;
// using TMPro;
// using UnityEngine.Localization.Settings;

// public class Localization : MonoBehaviour
// {
//     public TMP_Dropdown languageDropdown;

//     private void Start()
//     {
//         // Initialize the dropdown with the available languages
//         UpdateLanguageDropdown();

//         // Set the current language based on the saved preference (PlayerPrefs)
//         string savedLanguage = PlayerPrefs.GetString("SelectedLanguage", "en"); // Default to English if not set
//         SetLanguage(savedLanguage);

//         // Listen for changes in the dropdown
//         languageDropdown.onValueChanged.AddListener(OnLanguageDropdownChanged);
//     }

//     private void UpdateLanguageDropdown()
//     {
//         // Clear existing options
//         languageDropdown.ClearOptions();

//         // Add options to the dropdown based on the available locales
//         foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
//         {
//             languageDropdown.options.Add(new TMP_Dropdown.OptionData(locale.name));
//         }

//         // Refresh the dropdown display
//         languageDropdown.RefreshShownValue();
//     }

//     private void OnLanguageDropdownChanged(int index)
//     {
//         // Get the selected language from the dropdown
//         string selectedLanguage = LocalizationSettings.AvailableLocales.Locales[index].LocaleName;

//         // Set the language and save the preference
//         SetLanguage(selectedLanguage);
//         PlayerPrefs.SetString("SelectedLanguage", selectedLanguage);
//         Debug.Log($"Language changed to: {selectedLanguage}");
//     }

//     private void SetLanguage(string language)
//     {
//         // Change the current locale to the selected language
//         LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(language);
//     }
// }



// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// using UnityEngine.Localization.Settings;

// public class localization : MonoBehaviour
// {
//     private bool active = false;
//     public void ChangeLocale(int localeID) {
//         if (active == true)
//             return;
//         StartCoroutine(SetLocale(localeID));
//     }

//     IEnumerator SetLocale(int _localID) {
//         active = true;
//         yield return LocalizationSettings.InitializationOperation;
//         LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[_localID];
//         active = false;
//     }
// }
