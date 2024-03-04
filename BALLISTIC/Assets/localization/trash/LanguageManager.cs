// LanguageManager.cs
using UnityEngine;
using TMPro;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    // Add the language dropdown controller
    public LanguageDropdownController languageController;

    private void Awake()
    {
        // Ensure there is only one instance of the GameManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Ensure the language controller is available
        if (languageController != null)
        {
            // Set the dropdown value to the saved language index
            languageController.dropdown.value = GetCurrentLanguageIndex();  // <-- Change 'languageController.languageDropdown' to 'languageController.dropdown'
        }
    }

    // Add the GetCurrentLanguageIndex method
    public int GetCurrentLanguageIndex()
    {
        // Get the saved language from PlayerPrefs
        string savedLanguage = PlayerPrefs.GetString("SelectedLanguage", "English");

        // Get the index of the saved language in the dropdown
        for (int i = 0; i < languageController.dropdown.options.Count; i++)
        {
            if (languageController.dropdown.options[i].text == savedLanguage)
            {
                return i;
            }
        }

        return 0; // Default to the first language if not found
    }

    // Add the ChangeLanguage method
    public void ChangeLanguage(string newLanguage)
    {
        // Implement logic to change the language here
        // For example, you might want to update the PlayerPrefs to store the selected language
        PlayerPrefs.SetString("SelectedLanguage", newLanguage);
        PlayerPrefs.Save();

        // You can also update the UI or perform other actions related to changing the language
        // For example, you might use Unity's localization system to switch between different language files
        // Example: LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(newLanguage);
    }
}