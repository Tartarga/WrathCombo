using ECommons.DalamudServices;
using System.Globalization;
using System.Resources;
using WrathCombo.Resources.Localization;

namespace WrathCombo.Window
{
    internal class Text
    {
        // Pre-allocated cultures
        private static readonly CultureInfo Ja = new("ja");
        private static readonly CultureInfo En = new("en");
        private static readonly CultureInfo De = new("de");
        private static readonly CultureInfo Fr = new("fr");

        // Cache the game culture
        private static readonly CultureInfo GameCulture = GetGameCulture();

        // Expose TextInfo for formatting purposes (Job Names)
        public static TextInfo TextFormatting => GameCulture.TextInfo;

        private static CultureInfo GetGameCulture()
        {
            return (int)Svc.ClientState.ClientLanguage switch
            {
                0 => Ja,
                1 => En,
                2 => De,
                3 => Fr,
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
        public static string GetSettingsUIString(string key) => GetLocalizedString(key, Menu_Settings.ResourceManager);
    }
}
