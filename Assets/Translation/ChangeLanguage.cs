using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using System.Collections;

public class ChangeLanguage : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown languageDropdown;

    private IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;

        // Первый запуск игры
        if (!PlayerPrefs.HasKey("Language"))
        {
            PlayerPrefs.SetString("Language", "en");
            PlayerPrefs.Save();
        }

        string lang = PlayerPrefs.GetString("Language");

        LocalizationHelper.SetLanguage(lang);

        switch (lang)
        {
            case "en":
                languageDropdown.SetValueWithoutNotify(0);
                break;

            case "de":
                languageDropdown.SetValueWithoutNotify(2);
                break;

            case "ru":
                languageDropdown.SetValueWithoutNotify(1);
                break;
        }

        languageDropdown.RefreshShownValue();
        languageDropdown.onValueChanged.AddListener(ChangeLanguageFromDropdown);
    }

    private void ChangeLanguageFromDropdown(int index)
    {
        string lang = index switch
        {
            0 => "en",
            1 => "ru",
            2 => "de",
            _ => "en"
        };

        LocalizationHelper.SetLanguage(lang);

        PlayerPrefs.SetString("Language", lang);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (languageDropdown != null)
            languageDropdown.onValueChanged.RemoveListener(ChangeLanguageFromDropdown);
    }
}