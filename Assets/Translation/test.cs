using UnityEngine;
using UnityEngine.Localization.Settings;
using TMPro;
using System.Collections;

public class Test : MonoBehaviour
{
    public TMP_Text text;

    IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;

        LocalizationSettings.SelectedLocale =
            LocalizationSettings.AvailableLocales.GetLocale("ru");

        string value = LocalizationSettings.StringDatabase.GetLocalizedString(
            "UI Text",
            "Menu.Description");

        Debug.Log(value);

        text.text = value;
    }
}