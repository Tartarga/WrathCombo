#region

using System.Text;
using ECommons.ExcelServices;
using WrathCombo.Core;

#endregion

namespace WrathCombo.Extensions;

internal static partial class PresetExtensions
{
    public static PresetStorage.PresetData? Attributes(this Preset preset)
    {
        if (PresetStorage.AllPresets.TryGetValue(preset, out var atts))
            return atts;

        return null;
    }

    public static string Name(this Preset? preset) =>
        preset is null ? "" : preset.Value.Name();

    extension(Preset preset)
    {
        public string Name()
        {
            var attributes = preset.Attributes();

            if (attributes?.CustomComboInfo is null)
                return preset.ToString();

            return attributes.CustomComboInfo.Name;
        }

        public string NameWithFullLineage
            (Job? currentJob = null)
        {
            var pdata = preset.Attributes();

            if (pdata?.CustomComboInfo is null)
                return preset.ToString();

            var name = new StringBuilder(preset.Name());

            Preset? inspectingPreset = preset;
            while (inspectingPreset is not null)
            {
                var parent = PresetStorage.AllPresets[inspectingPreset.Value].Parent;
                if (parent is not null)
                {
                    name.Insert(0, $"{parent.Name()} > ");
                }
                // Insert the job name at the beginning
                else
                {
                    string lastPresetJob;
                    var lastPresetData = inspectingPreset.Value.Attributes();

                    if (currentJob is not null && lastPresetData.CustomComboInfo.Job == currentJob)
                        break;

                    if (lastPresetData.CustomComboInfo.Job == Job.ADV)
                    {
                        lastPresetJob = "[Roles And Content] ";
                        if (lastPresetData.IsVariant)
                            lastPresetJob += "Variant > ";
                        if (PresetStorage.IsOccultCrescent(inspectingPreset.Value))
                            lastPresetJob += "Occult Crescent > ";
                    }
                    else
                        lastPresetJob = $"[{lastPresetData.CustomComboInfo.JobShorthand}] ";

                    name.Insert(0, lastPresetJob);
                }

                inspectingPreset = parent;
            }

            return name.ToString();
        }

        public bool Enabled() =>
            PresetStorage.IsEnabled(preset);

        public bool FullLineageEnabled()
        {
            Preset? inspectingPreset = preset;
            while (inspectingPreset is not null)
            {
                if (!PresetStorage.IsEnabled(inspectingPreset.Value))
                    return false;

                var parent = PresetStorage.AllPresets[inspectingPreset.Value].Parent;
                inspectingPreset = parent;
            }

            return true;
        }
    }
}