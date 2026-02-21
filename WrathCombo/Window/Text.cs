using ECommons.DalamudServices;
using System;
using System.Globalization;
using System.Resources;
using WrathCombo.Resources.Localization;

namespace WrathCombo.Window
{
    internal class Text
    {
        private static TextInfo? _cachedTextInfo;

        //Used to format job names based on region
        public static TextInfo GetTextInfo()
        {
            // Use cached TextInfo if available
            // Otherwise create new and cache for future use
            if (_cachedTextInfo is null)
            {
                // Job names are lowercase by default
                // This capitalizes based on regional rules
                var cultureId = Svc.ClientState.ClientLanguage switch
                {
                    Dalamud.Game.ClientLanguage.French => "fr-FR",
                    Dalamud.Game.ClientLanguage.Japanese => "ja-JP",
                    Dalamud.Game.ClientLanguage.German => "de-DE",
                    _ => "en-US",
                };

                _cachedTextInfo = new CultureInfo(cultureId, useUserOverride: false).TextInfo;
            }

            return _cachedTextInfo;
        }

        public static string GetPresetString(string key)
        {
            return GetLocalizedString(key, CustomComboPresets.ResourceManager);
        }

        public static string GetSettingsUIString(string key)
        {
            return GetLocalizedString(key, Menu_Settings.ResourceManager);
        }

        // Get the string (English fallback)
        private static string GetLocalizedString(string key, ResourceManager? rm)
        {
            int langValue = (int)Svc.ClientState.ClientLanguage;

            string langCode;

            // Only trust 0–3 as standard global values
            if (langValue >= 0 && langValue <= 3)
            {
                langCode = langValue switch
                {
                    0 => "ja",   // Japanese
                    1 => "en",   // English
                    2 => "de",   // German
                    3 => "fr",   // French
                    _ => "en"    // shouldn't hit, but safety net
                };
            }
            else
            {
                // Anything outside 0–3 → treat as non-standard (e.g. Chinese fork) → use OS culture
                var osCulture = CultureInfo.CurrentUICulture;

                // Use TwoLetterISOLanguageName for simplicity (ja, en, de, fr, zh, etc.)
                langCode = osCulture.TwoLetterISOLanguageName;
            }

            CultureInfo culture;
            try
            {
                culture = new CultureInfo(langCode);
            }
            catch (CultureNotFoundException)
            {
                // If OS gives something weird/unrecognized, fall back to English
                culture = new CultureInfo("en");
            }

            // Try requested culture first
            string? resolved = rm.GetString(key, culture);

            // Strong English fallback if not found
            if (resolved == null && langCode != "en")
            {
                resolved = rm.GetString(key, new CultureInfo("en"));
            }


            // Fallback: show the key itself (good for debugging missing translations)
            string final = resolved ?? key;

            // Normalize line endings — safe & recommended
            return final.Replace("\\n", Environment.NewLine);
        }
    }
}
