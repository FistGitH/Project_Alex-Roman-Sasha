using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string tableName = "UI Text";
    [SerializeField] private string entryKey;

    private TMP_Text text;

    private IEnumerator Start()
    {
        text = GetComponent<TMP_Text>();

        yield return LocalizationSettings.InitializationOperation;

        UpdateText();

        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale)
    {
        UpdateText();
    }

    private void UpdateText()
    {
        text.text = LocalizationSettings.StringDatabase.GetLocalizedString(
            tableName,
            entryKey);
    }

    private void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }
}