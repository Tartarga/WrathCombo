using ECommons.DalamudServices;
using System.Globalization;
using System.Resources;
using WrathCombo.Resources.Localization.Presets;
using WrathCombo.Resources.Localization.UI.Settings;

namespace WrathCombo.Window
{
    internal class Text
    {
        // Pre-allocated cultures
        private static readonly CultureInfo Ja = new("ja");
        private static readonly CultureInfo En = new("en");
        private static readonly CultureInfo De = new("de");
        private static readonly CultureInfo Fr = new("fr");
        private static readonly CultureInfo ZhHans = new("zh-Hans"); // Simplified
        private static readonly CultureInfo ZhHant = new("zh-Hant"); // Traditional
        private static readonly CultureInfo ZhTW = new("zh-TW"); // Traditional
        private static readonly CultureInfo Ko = new("ko-KR");

        // Cache the game culture
        private static readonly CultureInfo GameCulture = GetGameCulture();

        // Expose TextInfo for formatting purposes (Job Names)
        public static TextInfo TextFormatting => GameCulture.TextInfo;

        private static CultureInfo GetGameCulture()
        {
            return (int)Svc.ClientState.ClientLanguage switch
            {
                // Global Client
                // https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Game/ClientLanguage.cs
                0 => Ja,
                1 => En,
                2 => De,
                3 => Fr,

                // Forked Client (AtmoOmen's Dalamud)
                // https://github.com/AtmoOmen/Dalamud/blob/master/Dalamud/Game/ClientLanguage.cs
                4 => ZhHans, // ChineseSimplified
                5 => ZhHant, // ChineseTraditional (CHT), unknown if correct
                6 => Ko,     // Korean
                7 => ZhTW, // TraditionalChinese (TC), unknown if correct

                // Fallback to current UI culture if we somehow get an unexpected value
                _ => CultureInfo.CurrentUICulture
            };
        }

        /// <summary>
        /// Core localized string resolver.
        /// Lets ResourceManager handle fallback chain.
        /// </summary>
        private static string GetLocalizedString(string key, ResourceManager rm)
        {
            var value = rm.GetString(key, GameCulture);

            // If missing entirely, return key (debug-friendly)
            return value ?? key;
        }

        /// <summary>
        /// Preset localization
        /// </summary>
        public static string GetPresetString(string key) => GetLocalizedString(key, CustomComboPresets.ResourceManager);

        /// <summary>
        /// Settings UI localization
        /// </summary>
        public static string GetSettingsUIString(string key) => GetLocalizedString(key, Settings.ResourceManager);
    }
}
