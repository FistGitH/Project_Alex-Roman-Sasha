using UnityEngine.Localization.Settings;
using System.Linq;

public static class LocalizationHelper
{
    public static void SetLanguage(string code)
    {
        var locale = LocalizationSettings.AvailableLocales.Locales
            .FirstOrDefault(x => x.Identifier.Code == code);

        if (locale != null)
            LocalizationSettings.SelectedLocale = locale;
    }
}