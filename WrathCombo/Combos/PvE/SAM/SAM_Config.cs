using ECommons.ImGuiMethods;
using System.Numerics;
using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Extensions;
using WrathCombo.Resources.Localization.JobConfigs;
using static WrathCombo.Window.Functions.UserConfig;
using static WrathCombo.Window.Text;
namespace WrathCombo.Combos.PvE;

internal partial class SAM
{
    internal static class Config
    {
        public static UserInt
            SAM_Balance_Content = new("SAM_Balance_Content", 1),
            SAM_Opener_IncludeGyoten = new("SAM_Opener_IncludeGyoten");

        public static UserBool
            SAM_Opener_Potion = new("SAM_Opener_Potion");
        internal static void Draw(Preset preset)
        {
            switch (preset)
            {
                case Preset.SAM_ST_Opener:
                    DrawBossOnlyChoice(SAM_Balance_Content);
                    DrawOpenerPotionChoice(SAM_Opener_Potion);

                    ImGui.TextWrapped(SAM_Config.SecondsDelayFromFirstStep);
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip(FormatAndCache(SAM_Config.DelaySavageBlade, All.SavageBlade.ActionName()));

                    ImGuiEx.Spacing(new Vector2(0, 10));
                    ImGuiEx.TextUnderlined($"{Gyoten.ActionName()} Settings");
                    ImGui.Spacing();
                    DrawRadioButton(SAM_Opener_IncludeGyoten,
                        FormatAndCache(SAM_Config.Include2x0, Gyoten.ActionName()),
                        FormatAndCache(SAM_Config.IncludeBoth0, Gyoten.ActionName()), 0, descriptionAsTooltip: true);
                    DrawRadioButton(SAM_Opener_IncludeGyoten,
                        SAM_Config.SkipBoth,
                        FormatAndCache(SAM_Config.SkipBothUsageOf0, Gyoten.ActionName()), 1, descriptionAsTooltip: true);
                    DrawRadioButton(SAM_Opener_IncludeGyoten,
                        SAM_Config.SkipFirst,
                        FormatAndCache(SAM_Config.SkipFirstUseOf0, Gyoten.ActionName()), 2, descriptionAsTooltip: true);
                    DrawRadioButton(SAM_Opener_IncludeGyoten,
                        SAM_Config.SkipSecond,
                        FormatAndCache(SAM_Config.SkipSecondUseOf0, Gyoten.ActionName()), 3, descriptionAsTooltip: true);
                    break;
            }
        }
    }
}
