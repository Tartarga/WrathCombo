using ECommons.ImGuiMethods;
using System.Numerics;
using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Extensions;
using WrathCombo.Resources.Localization.JobConfigs;
using WrathCombo.Window.Functions;
using static WrathCombo.Window.Functions.UserConfig;
using static WrathCombo.Window.Text;
namespace WrathCombo.Combos.PvE;

internal partial class SAM
{
    internal static class Config
    {
        public static UserInt
            SAM_Balance_Content = new("SAM_Balance_Content", 1),
            SAM_Opener_IncludeGyoten = new("SAM_Opener_IncludeGyoten"),
            SAM_ST_HiganbanaBossHPOption = new("SAM_ST_HiganbanaBossHPOption"),
            SAM_ST_HiganbanaBossAddsHPOption = new("SAM_ST_HiganbanaBossAddsHPOption", 25),
            SAM_ST_HiganbanaTrashHPOption = new("SAM_ST_HiganbanaTrashHPOption", 100),
            SAM_ST_HiganbanaRefresh = new("SAM_ST_HiganbanaRefresh", 15),
            SAM_ST_KenkiOvercapAmount = new("SAM_ST_KenkiOvercapAmount", 65),
            SAM_ST_ExecuteThreshold = new("SAM_ST_ExecuteThreshold", 5),
            SAM_ST_MeikyoExecuteThreshold = new("SAM_ST_MeikyoExecuteThreshold", 5),
            SAM_ST_ManualTN = new("SAM_ST_ManualTN"),
            SAM_ST_SecondWindHPThreshold = new("SAM_ST_SecondWindHPThreshold", 40),
            SAM_ST_BloodbathHPThreshold = new("SAM_ST_BloodbathHPThreshold", 30),
            SAM_AoE_KenkiOvercapAmount = new("SAM_AoE_KenkiOvercapAmount", 50),
            SAM_AoE_SecondWindHPThreshold = new("SAM_AoE_SecondWindHPThreshold", 40),
            SAM_AoE_BloodbathHPThreshold = new("SAM_AoE_BloodbathHPThreshold", 30);

        public static UserBool
            SAM_Opener_Potion = new("SAM_Opener_Potion"),
            SAM_ST_CDs_Guren = new("SAM_ST_CDs_Guren"),
            SAM_ST_CDs_OgiNamikiri_Movement = new("SAM_ST_CDs_OgiNamikiri_Movement");

        public static UserFloat
            SAM_ST_MeditateTimeStill = new("SAM_ST_MeditateTimeStill", 2.5f);

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

                case Preset.SAM_ST_CDs_UseHiganbana:
                    DrawSliderInt(0, 100, SAM_ST_HiganbanaBossHPOption,
                        Generics.BossOnlyHpPercent);

                    DrawSliderInt(0, 100, SAM_ST_HiganbanaBossAddsHPOption,
                        Generics.BossEncounterNonBossHpPercent);

                    DrawSliderInt(0, 100, SAM_ST_HiganbanaTrashHPOption,
                        Generics.NonBossHpPercent);

                    ImGui.Indent();
                    DrawSliderInt(0, 15, SAM_ST_HiganbanaRefresh,
                        FormatAndCache(Generics.DoTSecondsRemainingZeroDisable, Higanbana.ActionName()));
                    ImGui.Unindent();
                    break;

                case Preset.SAM_ST_CDs_Senei:
                    DrawAdditionalBoolChoice(SAM_ST_CDs_Guren,
                        FormatAndCache(Generics._0Option, Guren.ActionName()),
                        FormatAndCache(SAM_Config.Add0IfSeneiNotUnlocked, Guren.ActionName(), Senei.ActionName()));
                    break;

                case Preset.SAM_ST_CDs_OgiNamikiri:
                    DrawAdditionalBoolChoice(SAM_ST_CDs_OgiNamikiri_Movement,
                        Generics.MovementOption,
                        FormatAndCache(SAM_Config.Add0And1WhenNotMoving, OgiNamikiri.ActionName(), KaeshiNamikiri.ActionName()));
                    break;

                case Preset.SAM_ST_Shinten:
                    DrawSliderInt(50, 85, SAM_ST_KenkiOvercapAmount,
                        SAM_Config.KenkiOvercapAmount);

                    DrawSliderInt(0, 100, SAM_ST_ExecuteThreshold,
                        SAM_Config.HPPercentKenki);
                    break;

                case Preset.SAM_ST_CDs_MeikyoShisui:
                    DrawSliderInt(0, 100, SAM_ST_MeikyoExecuteThreshold,
                        FormatAndCache(SAM_Config.HPPercentMeikyo, MeikyoShisui.ActionName()));
                    break;

                case Preset.SAM_ST_TrueNorth:
                    DrawSliderInt(0, 1, SAM_ST_ManualTN,
                        Generics.ChargePool);
                    break;

                case Preset.SAM_ST_Meditate:
                    ImGui.SetCursorPosX(48f.Scale());
                    DrawSliderFloat(0, 3, SAM_ST_MeditateTimeStill,
                        Generics.StationaryDelayCheck, decimals: 1);
                    break;

                case Preset.SAM_ST_ComboHeals:
                    DrawSliderInt(0, 100, SAM_ST_SecondWindHPThreshold,
                        FormatAndCache(Generics.HPPercentageThreshold, Role.SecondWind.ActionName()));

                    DrawSliderInt(0, 100, SAM_ST_BloodbathHPThreshold,
                        FormatAndCache(Generics.HPPercentageThreshold, Role.Bloodbath.ActionName()));
                    break;

                case Preset.SAM_AoE_Kyuten:
                    DrawSliderInt(25, 85, SAM_AoE_KenkiOvercapAmount,
                        SAM_Config.KenkiOvercapAmount);
                    break;

                case Preset.SAM_AoE_ComboHeals:
                    DrawSliderInt(0, 100, SAM_AoE_SecondWindHPThreshold,
                        FormatAndCache(Generics.HPPercentageThreshold, Role.SecondWind.ActionName()));

                    DrawSliderInt(0, 100, SAM_AoE_BloodbathHPThreshold,
                        FormatAndCache(Generics.HPPercentageThreshold, Role.Bloodbath.ActionName()));
                    break;
            }
        }
    }
}
