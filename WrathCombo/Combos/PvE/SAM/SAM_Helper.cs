using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using System;
using System.Collections.Generic;
using WrathCombo.Combos.PvE.ALL;
using WrathCombo.CustomComboNS;
using WrathCombo.CustomComboNS.Functions;
using static FFXIVClientStructs.FFXIV.Client.Game.ActionManager;
using static WrathCombo.Combos.PvE.SAM.Config;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;
using ActionType = FFXIVClientStructs.FFXIV.Client.Game.ActionType;
namespace WrathCombo.Combos.PvE;

internal partial class SAM
{
    #region Combo

    private static uint WithTrueNorth(uint action, bool onPositional) =>
        !onPositional &&
        ActionReady(Role.TrueNorth) &&
        !HasStatusEffect(Role.Buffs.TrueNorth) &&
        TargetNeedsPositionals()
            ? Role.TrueNorth
            : action;

    private static uint DoMeikyoCombo(uint actionID, bool onAoE)
    {
        if (onAoE)
        {
            float fugetsuRemaining = GetStatusEffectRemainingTime(Buffs.Fugetsu);
            float fukaRemaining = GetStatusEffectRemainingTime(Buffs.Fuka);
            bool refreshFugetsu = fugetsuRemaining <= fukaRemaining;
            bool refreshFuka = fukaRemaining <= fugetsuRemaining;

            if ((!HasKa || !HasStatusEffect(Buffs.Fuka) ||
                 SenCount is 3 && refreshFuka) &&
                LevelChecked(Oka))
                return Oka;

            if (LevelChecked(Mangetsu) &&
                (!HasGetsu || !HasStatusEffect(Buffs.Fugetsu) || !LevelChecked(Oka) ||
                 SenCount is 3 && refreshFugetsu))
                return Mangetsu;

            return actionID;
        }

        if (LevelChecked(Yukikaze) && !HasSetsu && HasKa && HasGetsu)
            return Yukikaze;

        if (LevelChecked(Gekko) &&
            (!LevelChecked(Kasha) ||
             !HasStatusEffect(Buffs.Fugetsu) ||
             (OnTargetsRear() || OnTargetsFront()) && !HasGetsu ||
             OnTargetsFlank() && HasKa))
            return WithTrueNorth(Gekko, OnTargetsRear());

        if (LevelChecked(Kasha) &&
            (!HasStatusEffect(Buffs.Fuka) ||
             (OnTargetsFlank() || OnTargetsFront()) && !HasKa ||
             OnTargetsRear() && HasGetsu))
            return WithTrueNorth(Kasha, OnTargetsFlank());

        return actionID;
    }

    private static bool UseTsubame(bool onAoE)
    {
        if (!ActionReady(OriginalHook(TsubameGaeshi)) ||
            !InActionRange(OriginalHook(TsubameGaeshi)))
            return false;

        if (onAoE &&
            (HasStatusEffect(Buffs.TsubameReady) ||
             HasStatusEffect(Buffs.KaeshiGokenReady) ||
             HasStatusEffect(Buffs.TendoKaeshiGokenReady)))
            return true;

        if (HasStatusEffect(Buffs.TsubameReady) ||
            HasStatusEffect(Buffs.TendoKaeshiSetsugekkaReady))
            return true;

        return false;
    }

    private static bool UseIaiJutsu(bool onAoE)
    {
        if (IsMoving() ||
            !ActionReady(OriginalHook(Iaijutsu)) ||
            !InActionRange(OriginalHook(Iaijutsu)) ||
            !HasStatusEffect(Buffs.Fuka) ||
            !HasStatusEffect(Buffs.Fugetsu))
            return false;

        if (onAoE)
        {
            if (SenCount is 2 &&
                OriginalHook(Iaijutsu) is TenkaGoken or TendoGoken)
                return true;

            return false;
        }

        if (SenCount is 1 &&
            HasBattleTarget() &&
            CanApplyStatus(CurrentTarget, Debuffs.Higanbana) &&
            GetStatusEffectRemainingTime(Debuffs.Higanbana, CurrentTarget) <= 15)
            return true;

        if (SenCount is 3 ||
            SenCount is 2 && !LevelChecked(MidareSetsugekka))
            return true;

        return false;
    }

    private static bool UsePrepullMeikyo() =>
        !InCombat() && HasBattleTarget() &&
        ActionReady(MeikyoShisui) &&
        !HasStatusEffect(Buffs.MeikyoShisui);

    private static bool UseMeikyo(bool onAoE)
    {
        if (!ActionReady(MeikyoShisui) ||
            HasStatusEffect(Buffs.MeikyoShisui) ||
            HasStatusEffect(Buffs.Tendo) ||
            JustUsed(MeikyoShisui))
            return false;

        // Spend after a finisher. Do not gate on Senei CD alone — that left Meikyo
        // stuck at 2 charges (Enhanced Meikyo @76+, common on lvl 80 sync).
        if (onAoE)
            return ComboTimer is 0;

        return JustUsed(Yukikaze, 2f) || JustUsed(Gekko, 2f) || JustUsed(Kasha, 2f) ||
               JustUsed(KaeshiSetsugekka, 2f) || JustUsed(KaeshiNamikiri, 2f);
    }

    private static bool UseIkishoten() =>
        ActionReady(Ikishoten) &&
        !HasStatusEffect(Buffs.ZanshinReady) &&
        Kenki <= 50;

    private static bool UseZanshin() =>
        ActionReady(Zanshin) &&
        InActionRange(Zanshin) &&
        HasStatusEffect(Buffs.ZanshinReady);

    private static bool UseShoha() =>
        ActionReady(Shoha) && MeditationStacks is 3;

    private static bool UseHagakure() =>
        ActionReady(Hagakure) &&
        OriginalHook(Iaijutsu) is MidareSetsugekka or TendoSetsugekka;

    private static bool UseOgiNamikiri() =>
        ActionReady(OriginalHook(OgiNamikiri)) &&
        InActionRange(OriginalHook(OgiNamikiri)) &&
        (IsNamikiriReady ||
         HasStatusEffect(Buffs.OgiNamikiriReady) && !IsMoving());

    private static bool CanDumpKenki() =>
        Kenki >= 95 ||
        !LevelChecked(Guren) ||
        GetCooldownRemainingTime(Guren) > GCD * 6 ||
        Kenki >= 50;

    // Pre-100: on CD. At 100: under Tendo / right after Tendo Midare or Kaeshi.
    private static bool UseSenei() =>
        ActionReady(Senei) &&
        (!LevelChecked(TendoSetsugekka) ||
         HasStatusEffect(Buffs.Tendo) && SenCount >= 2 ||
         JustUsed(TendoSetsugekka, 15f) ||
         JustUsed(TendoKaeshiSetsugekka, 15f));

    private static bool UseKenki(ref uint actionID, bool onAoE)
    {
        if (onAoE)
        {
            if (ActionReady(Guren) && InActionRange(Guren))
            {
                actionID = Guren;
                return true;
            }

            if (ActionReady(Kyuten) && InActionRange(Kyuten) && CanDumpKenki())
            {
                actionID = Kyuten;
                return true;
            }

            return false;
        }

        if (UseSenei())
        {
            actionID = Senei;
            return true;
        }

        if (!LevelChecked(Senei) && ActionReady(Guren) && InActionRange(Guren))
        {
            actionID = Guren;
            return true;
        }

        if (ActionReady(Shinten) && CanDumpKenki())
        {
            actionID = Shinten;
            return true;
        }

        return false;
    }

    private static uint DoBasicCombo(bool onAoE)
    {
        if (onAoE)
        {
            if (ComboTimer > 0 && ComboAction is Fuko or Fuga)
            {
                float fugetsuRemaining = GetStatusEffectRemainingTime(Buffs.Fugetsu);
                float fukaRemaining = GetStatusEffectRemainingTime(Buffs.Fuka);
                bool refreshFugetsu = fugetsuRemaining <= fukaRemaining;
                bool refreshFuka = fukaRemaining <= fugetsuRemaining;

                if ((!HasKa || !HasStatusEffect(Buffs.Fuka) ||
                     SenCount is 3 && refreshFuka) &&
                    LevelChecked(Oka))
                    return Oka;

                if (LevelChecked(Mangetsu) &&
                    HasStatusEffect(Buffs.Fuka) &&
                    (!HasGetsu || !HasStatusEffect(Buffs.Fugetsu) || !LevelChecked(Oka) ||
                     SenCount is 3 && refreshFugetsu))
                    return Mangetsu;
            }

            return OriginalHook(Fuga);
        }

        if (ComboTimer > 0)
        {
            if (ComboAction is Hakaze or Gyofu)
            {
                float fugetsuRemaining = GetStatusEffectRemainingTime(Buffs.Fugetsu);
                float fukaRemaining = GetStatusEffectRemainingTime(Buffs.Fuka);
                bool refreshFugetsu = fugetsuRemaining <= fukaRemaining;
                bool refreshFuka = fukaRemaining <= fugetsuRemaining;

                if (!LevelChecked(Gekko))
                {
                    if (LevelChecked(Shifu) &&
                        (!HasStatusEffect(Buffs.Fuka) ||
                         HasStatusEffect(Buffs.Fugetsu) && refreshFuka))
                        return Shifu;

                    if (LevelChecked(Jinpu))
                        return Jinpu;

                    if (LevelChecked(Shifu))
                        return Shifu;
                }

                if (LevelChecked(Yukikaze) && !HasSetsu &&
                    fugetsuRemaining > 7 && fukaRemaining > 7)
                    return Yukikaze;

                if (LevelChecked(Shifu) &&
                    ((OnTargetsFlank() || OnTargetsFront()) && !HasKa && LevelChecked(Kasha) ||
                     OnTargetsRear() && HasGetsu && LevelChecked(Kasha) ||
                     !HasStatusEffect(Buffs.Fuka) ||
                     SenCount is 3 && refreshFuka ||
                     !LevelChecked(Kasha) && LevelChecked(Gekko)))
                    return Shifu;

                if (LevelChecked(Jinpu) &&
                    (!LevelChecked(Kasha) && LevelChecked(Gekko) ||
                     (OnTargetsRear() || OnTargetsFront()) && !HasGetsu && LevelChecked(Gekko) ||
                     OnTargetsFlank() && HasKa && LevelChecked(Gekko) ||
                     !HasStatusEffect(Buffs.Fugetsu) ||
                     SenCount is 3 && refreshFugetsu))
                    return Jinpu;
            }

            if (ComboAction is Jinpu && LevelChecked(Gekko))
                return WithTrueNorth(Gekko, OnTargetsRear());

            if (ComboAction is Shifu && LevelChecked(Kasha))
                return WithTrueNorth(Kasha, OnTargetsFlank());
        }

        return OriginalHook(Hakaze);
    }

    #endregion

    #region Openers

    internal static WrathOpener Opener()
    {
        if (Lvl70.LevelChecked)
            return Lvl70;

        if (Lvl80.LevelChecked)
            return Lvl80;

        if (Lvl90.LevelChecked)
            return Lvl90;

        if (Lvl100.LevelChecked)
            return Lvl100;

        return WrathOpener.Dummy;
    }

    internal static SAMLvl70Opener Lvl70 = new();
    internal static SAMLvl80Opener Lvl80 = new();
    internal static SAMLvl90Opener Lvl90 = new();
    internal static SAMLvl100Opener Lvl100 = new();

    internal abstract class SAMOpenerBase : WrathOpener
    {
        public override Preset Preset => Preset.SAM_ST_Opener;

        internal override UserData ContentCheckConfig => SAM_Balance_Content;
        internal override bool IncludePot => SAM_Opener_Potion;

        public override List<(int[] Steps, Func<float> HoldDelay)> PrepullDelays { get; set; } =
        [
            ([1], () => CountdownRemaining - 13),
            ([2], () => CountdownRemaining - 5)
        ];

        public override List<(int[] Steps, Func<bool> Condition)> SkipSteps { get; set; } =
        [
            ([2], () => !TargetNeedsPositionals())
        ];

        protected static bool SharedOpenerCooldowns() =>
            GetRemainingCharges(Role.TrueNorth) >= 1 &&
            IsOffCooldown(Ikishoten) &&
            SenCount is 0;
    }

    internal class SAMLvl70Opener : SAMOpenerBase
    {
        public override int MinOpenerLevel => 70;
        public override int MaxOpenerLevel => 70;

        public override List<uint> OpenerActions { get; set; } =
        [
            MeikyoShisui,
            Role.TrueNorth,
            Gekko,
            Items.UseItem(Items.GetStrongestPotionRow(Items.PotionType.Strength)),
            Kasha,
            Ikishoten,
            Yukikaze,
            Shinten,
            MidareSetsugekka,
            Shinten,
            Hakaze,
            Guren,
            Yukikaze,
            Shinten,
            Higanbana
        ];

        public override bool HasCooldowns() =>
            IsOffCooldown(MeikyoShisui) &&
            IsOffCooldown(Guren) &&
            SharedOpenerCooldowns();
    }

    internal class SAMLvl80Opener : SAMOpenerBase
    {
        public override int MinOpenerLevel => 80;
        public override int MaxOpenerLevel => 80;

        public override List<uint> OpenerActions { get; set; } =
        [
            MeikyoShisui,
            Role.TrueNorth,
            Gekko,
            Items.UseItem(Items.GetStrongestPotionRow(Items.PotionType.Strength)),
            Ikishoten,
            Kasha,
            Yukikaze,
            MidareSetsugekka,
            Senei,
            KaeshiSetsugekka,
            MeikyoShisui,
            Gekko,
            Higanbana,
            Gekko,
            Kasha,
            Hakaze,
            Yukikaze,
            MidareSetsugekka,
            Shoha,
            KaeshiSetsugekka
        ];

        public override bool HasCooldowns() =>
            GetRemainingCharges(MeikyoShisui) is 2 &&
            IsOffCooldown(Senei) &&
            SharedOpenerCooldowns();
    }

    internal class SAMLvl90Opener : SAMOpenerBase
    {
        public override int MinOpenerLevel => 90;
        public override int MaxOpenerLevel => 90;

        public override List<uint> OpenerActions { get; set; } =
        [
            MeikyoShisui,
            Role.TrueNorth,
            Gekko,
            Items.UseItem(Items.GetStrongestPotionRow(Items.PotionType.Strength)),
            Ikishoten,
            Kasha,
            Yukikaze,
            MidareSetsugekka,
            Senei,
            KaeshiSetsugekka,
            MeikyoShisui,
            Gekko,
            Higanbana,
            OgiNamikiri,
            Shoha,
            KaeshiNamikiri,
            Kasha,
            Gekko,
            Hakaze,
            Yukikaze,
            MidareSetsugekka,
            KaeshiSetsugekka
        ];

        public override bool HasCooldowns() =>
            GetRemainingCharges(MeikyoShisui) is 2 &&
            IsOffCooldown(Senei) &&
            SharedOpenerCooldowns();
    }

    internal class SAMLvl100Opener : SAMOpenerBase
    {
        public override int MinOpenerLevel => 100;
        public override int MaxOpenerLevel => 100;

        public override List<uint> OpenerActions { get; set; } =
        [
            MeikyoShisui,
            Role.TrueNorth,
            Gekko,
            Items.UseItem(Items.GetStrongestPotionRow(Items.PotionType.Strength)),
            Kasha,
            Ikishoten,
            Yukikaze,
            TendoSetsugekka,
            Senei,
            TendoKaeshiSetsugekka,
            MeikyoShisui,
            Gekko,
            Zanshin,
            Higanbana,
            OgiNamikiri,
            Shoha,
            KaeshiNamikiri,
            Kasha,
            Shinten,
            Gekko,
            Gyoten,
            Gyofu,
            Yukikaze,
            Shinten,
            TendoSetsugekka,
            Gyoten,
            TendoKaeshiSetsugekka
        ];

        public override List<(int[] Steps, Func<bool> Condition)> SkipSteps { get; set; } =
        [
            ([2], () => !TargetNeedsPositionals()),
            ([19, 24], () => !ActionReady(Shinten)),
            ([21], () => !ActionReady(Gyoten) || (int)SAM_Opener_IncludeGyoten is 1 or 2),
            ([26], () => !ActionReady(Gyoten) || (int)SAM_Opener_IncludeGyoten is 1 or 3),
            ([8, 25], () => SenCount is not 3 && !(SenCount is 2 && JustUsed(Yukikaze))),
            ([10, 27], () => !HasStatusEffect(Buffs.TsubameReady) && !JustUsed(TendoSetsugekka)),
            ([14], () => SenCount is not 1 && !(SenCount is 2 && JustUsed(Gekko)))
        ];

        public override bool HasCooldowns() =>
            GetRemainingCharges(MeikyoShisui) is 2 &&
            IsOffCooldown(Senei) &&
            SharedOpenerCooldowns();
    }

    #endregion

    #region Gauge

    private static float GCD =>
        GetAdjustedRecastTime(ActionType.Action, Hakaze) / 1000f;

    private static SAMGauge Gauge => GetJobGauge<SAMGauge>();

    private static bool HasGetsu => Gauge.HasGetsu;

    private static bool HasSetsu => Gauge.HasSetsu;

    private static bool HasKa => Gauge.HasKa;

    private static byte Kenki => Gauge.Kenki;

    private static byte MeditationStacks => Gauge.MeditationStacks;

    private static Kaeshi Kaeshi => Gauge.Kaeshi;

    private static bool IsNamikiriReady => Kaeshi is Kaeshi.Namikiri;

    private static int SenCount =>
        (HasGetsu ? 1 : 0) + (HasSetsu ? 1 : 0) + (HasKa ? 1 : 0);

    #endregion

    #region ID's

    public const uint
        Hakaze = 7477,
        Yukikaze = 7480,
        Gekko = 7481,
        Enpi = 7486,
        Jinpu = 7478,
        Kasha = 7482,
        Shifu = 7479,
        Mangetsu = 7484,
        Fuga = 7483,
        Oka = 7485,
        Higanbana = 7489,
        TenkaGoken = 7488,
        MidareSetsugekka = 7487,
        Shinten = 7490,
        Kyuten = 7491,
        Hagakure = 7495,
        Guren = 7496,
        Meditate = 7497,
        Senei = 16481,
        MeikyoShisui = 7499,
        Seigan = 7501,
        ThirdEye = 7498,
        Iaijutsu = 7867,
        TsubameGaeshi = 16483,
        KaeshiHiganbana = 16484,
        Shoha = 16487,
        Ikishoten = 16482,
        Fuko = 25780,
        OgiNamikiri = 25781,
        KaeshiNamikiri = 25782,
        Yaten = 7493,
        Gyoten = 7492,
        KaeshiSetsugekka = 16486,
        TendoGoken = 36965,
        TendoKaeshiSetsugekka = 36968,
        Zanshin = 36964,
        TendoSetsugekka = 36966,
        Tengentsu = 7498,
        Gyofu = 36963;

    public static class Buffs
    {
        public const ushort
            MeikyoShisui = 1233,
            EnhancedEnpi = 1236,
            EyesOpen = 1252,
            Meditate = 1231,
            OgiNamikiriReady = 2959,
            Fuka = 1299,
            Fugetsu = 1298,
            TsubameReady = 4216,
            TendoKaeshiSetsugekkaReady = 4218,
            KaeshiGokenReady = 3852,
            TendoKaeshiGokenReady = 4217,
            ZanshinReady = 3855,
            Tengentsu = 3853,
            Tendo = 3856;
    }

    public static class Debuffs
    {
        public const ushort
            Higanbana = 1228;
    }

    public static class Traits
    {
        public const ushort
            EnhancedHissatsu = 591,
            EnhancedMeikyoShishui = 443,
            EnhancedMeikyoShishui2 = 593;
    }

    #endregion
}
