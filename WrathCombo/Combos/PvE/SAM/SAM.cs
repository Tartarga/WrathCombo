using WrathCombo.CustomComboNS;
using WrathCombo.Native;
using static WrathCombo.Combos.PvE.SAM.Config;
namespace WrathCombo.Combos.PvE;

internal partial class SAM : Melee
{
    internal class SAM_ST_SimpleMode : CustomCombo
    {
        protected internal override Preset Preset => Preset.SAM_ST_SimpleMode;

        protected override uint Invoke(uint actionID)
        {
            if (!CustomActionHelper.OneButtonRotationChecker(actionID, CustomActionType.SingleTargetDPS, Hakaze, Gyofu))
                return actionID;

            if (UsePrepullMeikyo())
                return MeikyoShisui;

            if (CanWeave())
            {
                if (UseMeikyo(false))
                    return MeikyoShisui;

                if (UseIkishoten())
                    return Ikishoten;

                if (UseZanshin())
                    return Zanshin;

                if (UseShoha())
                    return Shoha;

                if (UseKenki(ref actionID, false))
                    return actionID;
            }

            if (UseTsubame(false))
                return OriginalHook(TsubameGaeshi);

            if (UseOgiNamikiri(false))
                return OriginalHook(OgiNamikiri);

            if (UseIaiJutsu(false))
                return OriginalHook(Iaijutsu);

            return HasStatusEffect(Buffs.MeikyoShisui)
                ? DoMeikyoCombo(actionID, false)
                : DoBasicCombo(false);
        }
    }

    internal class SAM_AoE_SimpleMode : CustomCombo
    {
        protected internal override Preset Preset => Preset.SAM_AoE_SimpleMode;

        protected override uint Invoke(uint actionID)
        {
            if (!CustomActionHelper.OneButtonRotationChecker(actionID, CustomActionType.AoEDPS, Fuga, Fuko))
                return actionID;

            if (UsePrepullMeikyo())
                return MeikyoShisui;

            if (CanWeave())
            {
                if (UseHagakure())
                    return Hagakure;

                if (UseMeikyo(true))
                    return MeikyoShisui;

                if (UseIkishoten())
                    return Ikishoten;

                if (UseZanshin())
                    return Zanshin;

                if (UseShoha())
                    return Shoha;

                if (UseKenki(ref actionID, true))
                    return actionID;
            }

            if (UseTsubame(true))
                return OriginalHook(TsubameGaeshi);

            if (UseOgiNamikiri(true))
                return OriginalHook(OgiNamikiri);

            if (UseIaiJutsu(true))
                return OriginalHook(Iaijutsu);

            return HasStatusEffect(Buffs.MeikyoShisui)
                ? DoMeikyoCombo(actionID, true)
                : DoBasicCombo(true);
        }
    }

    internal class SAM_ST_AdvancedMode : CustomCombo
    {
        protected internal override Preset Preset => Preset.SAM_ST_AdvancedMode;

        protected override uint Invoke(uint actionID)
        {
            if (!CustomActionHelper.OneButtonRotationChecker(actionID, CustomActionType.SingleTargetDPS, Hakaze, Gyofu))
                return actionID;

            if (IsEnabled(Preset.SAM_ST_Opener) &&
                Opener().FullOpener(ref actionID) &&
                HasBattleTarget())
                return actionID;

            if (IsEnabled(Preset.SAM_ST_CDs) &&
                IsEnabled(Preset.SAM_ST_CDs_MeikyoShisui) &&
                UsePrepullMeikyo(requireNotJustUsed: true))
                return MeikyoShisui;

            if (ContentSpecificActions.TryGet(out uint contentAction))
                return contentAction;

            if (CanWeave())
            {
                if (IsEnabled(Preset.SAM_ST_CDs))
                {
                    if (IsEnabled(Preset.SAM_ST_CDs_MeikyoShisui) &&
                        UseMeikyo(false, SAM_ST_MeikyoExecuteThreshold))
                        return MeikyoShisui;

                    if (IsEnabled(Preset.SAM_ST_CDs_Ikishoten) &&
                        UseIkishoten())
                        return Ikishoten;
                }

                if (IsEnabled(Preset.SAM_ST_Damage))
                {
                    if (IsEnabled(Preset.SAM_ST_CDs_Senei))
                    {
                        if (UseSenei())
                            return Senei;

                        if (SAM_ST_CDs_Guren &&
                            !LevelChecked(Senei) &&
                            UseGuren())
                            return Guren;
                    }

                    if (IsEnabled(Preset.SAM_ST_CDs_Zanshin) &&
                        UseZanshin())
                        return Zanshin;

                    if (IsEnabled(Preset.SAM_ST_CDs_Shoha) &&
                        UseShoha())
                        return Shoha;

                    if (IsEnabled(Preset.SAM_ST_Shinten) &&
                        UseShinten(SAM_ST_ExecuteThreshold, SAM_ST_KenkiOvercapAmount))
                        return Shinten;
                }

                if (IsEnabled(Preset.SAM_ST_Feint) &&
                    Role.CanFeint() &&
                    GroupDamageIncoming())
                    return Role.Feint;

                if (IsEnabled(Preset.SAM_ST_ThirdEye) &&
                    UseThirdEye())
                    return OriginalHook(ThirdEye);

                if (IsEnabled(Preset.SAM_ST_Meditate) &&
                    UseMeditate())
                    return Meditate;

                if (IsEnabled(Preset.SAM_ST_ComboHeals))
                {
                    if (Role.CanSecondWind(SAM_ST_SecondWindHPThreshold))
                        return Role.SecondWind;

                    if (Role.CanBloodBath(SAM_ST_BloodbathHPThreshold))
                        return Role.Bloodbath;
                }

                if (IsEnabled(Preset.SAM_ST_StunInterrupt) &&
                    RoleActions.Melee.CanLegSweep())
                    return Role.LegSweep;
            }

            if (IsEnabled(Preset.SAM_ST_Damage))
            {
                if (IsEnabled(Preset.SAM_ST_CDs_Iaijutsu) &&
                    IsEnabled(Preset.SAM_ST_CDs_UseTsubame) &&
                    UseTsubame(false))
                    return OriginalHook(TsubameGaeshi);

                if (IsEnabled(Preset.SAM_ST_CDs_OgiNamikiri) &&
                    UseOgiNamikiri(false, respectMovement: SAM_ST_CDs_OgiNamikiri_Movement))
                    return OriginalHook(OgiNamikiri);

                if (IsEnabled(Preset.SAM_ST_CDs_Iaijutsu) &&
                    UseIaiJutsu(
                        false,
                        useHiganbana: IsEnabled(Preset.SAM_ST_CDs_UseHiganbana),
                        useTenkaGoken: IsEnabled(Preset.SAM_ST_CDs_UseTenkaGoken),
                        useMidare: IsEnabled(Preset.SAM_ST_CDs_UseMidare),
                        onlyWhenStationary: IsEnabled(Preset.SAM_ST_CDs_Iaijutsu_Movement),
                        higanbanaHpThreshold: HiganbanaHPThreshold(),
                        higanbanaDotRefresh: SAM_ST_HiganbanaRefresh))
                    return OriginalHook(Iaijutsu);

                if (IsEnabled(Preset.SAM_ST_RangedUptime) &&
                    ActionReady(Enpi) && !InMeleeRange() && HasBattleTarget())
                    return Enpi;
            }

            return HasStatusEffect(Buffs.MeikyoShisui)
                ? DoMeikyoCombo(
                    actionID,
                    false,
                    useTrueNorth: IsEnabled(Preset.SAM_ST_TrueNorth),
                    useYukikaze: IsEnabled(Preset.SAM_ST_Yukikaze),
                    useKasha: IsEnabled(Preset.SAM_ST_Kasha),
                    useGekko: IsEnabled(Preset.SAM_ST_Gekko),
                    trueNorthCharges: SAM_ST_ManualTN)
                : DoBasicCombo(
                    false,
                    useTrueNorth: IsEnabled(Preset.SAM_ST_TrueNorth),
                    useYukikaze: IsEnabled(Preset.SAM_ST_Yukikaze),
                    useKasha: IsEnabled(Preset.SAM_ST_Kasha),
                    useGekko: IsEnabled(Preset.SAM_ST_Gekko),
                    trueNorthCharges: SAM_ST_ManualTN);
        }
    }

    internal class SAM_AoE_AdvancedMode : CustomCombo
    {
        protected internal override Preset Preset => Preset.SAM_AoE_AdvancedMode;

        protected override uint Invoke(uint actionID)
        {
            if (!CustomActionHelper.OneButtonRotationChecker(actionID, CustomActionType.AoEDPS, Fuga, Fuko))
                return actionID;

            if (IsEnabled(Preset.SAM_AoE_CDs) &&
                IsEnabled(Preset.SAM_AoE_MeikyoShisui) &&
                UsePrepullMeikyo(requireNotJustUsed: true))
                return MeikyoShisui;

            if (ContentSpecificActions.TryGet(out uint contentAction))
                return contentAction;

            if (CanWeave())
            {
                if (IsEnabled(Preset.SAM_AoE_Hagakure) &&
                    UseHagakure())
                    return Hagakure;

                if (IsEnabled(Preset.SAM_AoE_CDs))
                {
                    if (IsEnabled(Preset.SAM_AoE_MeikyoShisui) &&
                        UseMeikyo(true))
                        return MeikyoShisui;

                    if (IsEnabled(Preset.SAM_AoE_CDs_Ikishoten) &&
                        UseIkishoten())
                        return Ikishoten;
                }

                if (IsEnabled(Preset.SAM_AoE_Damage))
                {
                    if (IsEnabled(Preset.SAM_AoE_Zanshin) &&
                        UseZanshin())
                        return Zanshin;

                    if (IsEnabled(Preset.SAM_AoE_Guren) &&
                        UseGuren())
                        return Guren;

                    if (IsEnabled(Preset.SAM_AoE_Shoha) &&
                        UseShoha())
                        return Shoha;
                }

                if (IsEnabled(Preset.SAM_AoE_Kyuten) &&
                    UseKyuten(SAM_AoE_KenkiOvercapAmount))
                    return Kyuten;

                if (IsEnabled(Preset.SAM_AoE_ComboHeals))
                {
                    if (Role.CanSecondWind(SAM_AoE_SecondWindHPThreshold))
                        return Role.SecondWind;

                    if (Role.CanBloodBath(SAM_AoE_BloodbathHPThreshold))
                        return Role.Bloodbath;
                }

                if (IsEnabled(Preset.SAM_AoE_StunInterrupt) &&
                    RoleActions.Melee.CanLegSweep())
                    return Role.LegSweep;
            }

            if (IsEnabled(Preset.SAM_AoE_Damage))
            {
                if (IsEnabled(Preset.SAM_AoE_TenkaGoken) &&
                    UseTsubame(true))
                    return OriginalHook(TsubameGaeshi);

                if (IsEnabled(Preset.SAM_AoE_OgiNamikiri) &&
                    UseOgiNamikiri(true))
                    return OriginalHook(OgiNamikiri);

                if (IsEnabled(Preset.SAM_AoE_TenkaGoken) &&
                    UseIaiJutsu(true))
                    return OriginalHook(Iaijutsu);
            }

            return HasStatusEffect(Buffs.MeikyoShisui)
                ? DoMeikyoCombo(actionID, true, useOka: IsEnabled(Preset.SAM_AoE_Oka))
                : DoBasicCombo(true, useOka: IsEnabled(Preset.SAM_AoE_Oka));
        }
    }
}
