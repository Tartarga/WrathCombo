using ECommons.DalamudServices;
using ECommons.ExcelServices;
using Lumina.Excel.Sheets;
using System.Collections.Frozen;
using System.Globalization;
using System.Linq;
using WrathCombo.Combos;
using WrathCombo.Combos.PvE;
using WrathCombo.Core;

namespace WrathCombo.CustomComboNS.Functions;

internal abstract partial class CustomComboFunctions
{
    /// <summary> Checks if the given preset is enabled. </summary>
    public static bool IsEnabled(CustomComboPreset preset)
    {
        if ((int)preset < 100)
            return true;

        try
        {
            (string controllers, bool enabled, bool autoMode)? checkControlled = P.UIHelper.PresetControlled(preset);
            bool controlled = checkControlled is not null;
            bool? controlledState = checkControlled?.enabled;

            return controlled
                ? (bool)controlledState!
                : PresetStorage.IsEnabled(preset);
        }
        // IPC is not loaded yet
        catch
        {
            return PresetStorage.IsEnabled(preset);
        }
    }

    /// <summary> Checks if the given preset is not enabled. </summary>
    public static bool IsNotEnabled(CustomComboPreset preset) => !IsEnabled(preset);

    public class JobIDs
    {
        private static TextInfo? _cachedTextInfo;

        public static readonly FrozenDictionary<uint, ClassJob> ClassJobs = Svc.Data.GetExcelSheet<ClassJob>()!.ToFrozenDictionary(i => i.RowId);

        public static int JobIDToRole(uint jobId) => ClassJobs.TryGetValue(jobId, out var classJob) ? classJob.Role : 0;

        public static uint JobIDToClassJobCategory(uint jobId) => ClassJobs.TryGetValue(jobId, out var classJob) ? classJob.ClassJobCategory.RowId : 0;

        public static string JobIDToShorthand(uint jobId) => jobId != 0 && ClassJobs.TryGetValue(jobId, out var classJob) ? classJob.Abbreviation.ToString() : string.Empty;

        public static string JobIDToName(uint jobId)
        {
            // Special Cases
            switch (jobId)
            {
                case 0:     return "General/Multiple Jobs";
                case 99:    return "Global";
                case 100:   return OccultCrescent.ContentName;
            }

            // Override DoH/DoL
            jobId = jobId switch
            {
                DOH.JobID   => 8, // Carpenter
                DOL.JobID   => 16, // Miner
                _           => jobId
            };

            // Invalid Job
            if (!ClassJobs.TryGetValue(jobId, out var classJob))
                return "Unknown";

            // Combat Jobs: Use Job Name
            // DoH/DoL Jobs: Use Category Name
            string jobName = (jobId is 8 or 16)
                ? classJob.ClassJobCategory.Value.Name.ToString()
                : classJob.Name.ToString();

            return GetTextInfo().ToTitleCase(jobName);
        }

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
                    Dalamud.Game.ClientLanguage.French      => "fr-FR",
                    Dalamud.Game.ClientLanguage.Japanese    => "ja-JP",
                    Dalamud.Game.ClientLanguage.German      => "de-DE",
                    _                                       => "en-US",
                };

                _cachedTextInfo = new CultureInfo(cultureId, useUserOverride: false).TextInfo;
            }

            return _cachedTextInfo;
        }

        public static readonly FrozenSet<uint> Tank =
            Svc.Data.GetExcelSheet<ClassJob>()!
            .Where(cj => cj.Role == 1)
            .Select(cj => cj.RowId)
            .ToFrozenSet();

        public static readonly FrozenSet<uint> Healer =
            Svc.Data.GetExcelSheet<ClassJob>()!
                .Where(cj => cj.Role == 4)
                .Select(cj => cj.RowId)
                .ToFrozenSet();

        public static readonly FrozenSet<uint> Melee =
            Svc.Data.GetExcelSheet<ClassJob>()!
                .Where(cj => cj.Role == 2)
                .Select(cj => cj.RowId)
                .ToFrozenSet();

        public static readonly FrozenSet<uint> Ranged =
            Svc.Data.GetExcelSheet<ClassJob>()!
                .Where(cj => cj.Role == 3)
                .Select(cj => cj.RowId)
                .ToFrozenSet();
    }
}