using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameFunctions;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using WrathCombo.Combos.PvE.ALL;
using WrathCombo.Core;
using WrathCombo.CustomComboNS;
using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Extensions;
using static WrathCombo.Combos.PvE.SGE.Config;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;
namespace WrathCombo.Combos.PvE;

internal partial class SGE
{
    private static bool IsPhlegmaCapped =>
        GetRemainingCharges(OriginalHook(Phlegma)) == GetMaxCharges(OriginalHook(Phlegma));

    private static IGameObject? Target =>
        SimpleTarget.UIMouseOverTarget.IfCanUseOn(Kardia).IfWithinRange(30) ??
        SimpleTarget.HardTarget.IfCanUseOn(Kardia).IfWithinRange(30) ??
        SimpleTarget.AnyTank;

    private static IGameObject? HealStack =>
        SimpleTarget.Stack.AllyToHeal;

    private static bool HasAddersgall() => Addersgall > 0;

    private static bool HasAddersgallAboveHold() => Addersgall > SGE_Heal_HoldAddersgall;

    private static bool HasAddersting() =>
        Addersting > 0;

    #region Lists

    internal static readonly FrozenDictionary<uint, ushort> EukrasianDosisList = new Dictionary<uint, ushort>
    {
        { Dosis, Debuffs.EukrasianDosis },
        { Dosis2, Debuffs.EukrasianDosis2 },
        { Dosis3, Debuffs.EukrasianDosis3 }
    }.ToFrozenDictionary();

    private static readonly List<uint>
        AddersgallList = [Taurochole, Druochole, Ixochole, Kerachole],
        DyskrasiaList = [Dyskrasia, Dyskrasia2];

    private static readonly FrozenDictionary<uint, (ushort Debuff, uint Eukrasian)> DosisList = new Dictionary<uint, (ushort D, uint E)>
    {
        { Dosis, (D: Debuffs.EukrasianDosis, E: EukrasianDosis) },
        { Dosis2, (D: Debuffs.EukrasianDosis2, E: EukrasianDosis2) },
        { Dosis3, (D: Debuffs.EukrasianDosis3, E: EukrasianDosis3) },
        //For bad latency/fps where OriginalHook(Dosis) might return an Eukrasian,
        { EukrasianDosis, (D: Debuffs.EukrasianDosis, E: EukrasianDosis) },
        { EukrasianDosis2, (D: Debuffs.EukrasianDosis2, E: EukrasianDosis2) },
        { EukrasianDosis3, (D: Debuffs.EukrasianDosis3, E: EukrasianDosis3) }
    }.ToFrozenDictionary();

    #endregion

    #region Gauge

    private static SGEGauge Gauge => GetJobGauge<SGEGauge>();

    private static byte Addersgall => Gauge.Addersgall;

    private static byte Addersting => Gauge.Addersting;

    #endregion

    #region Dot Checker

    internal static bool ShouldRefreshEDosis()
    {
        uint dotAction = OriginalHook(Dosis);
        int hpThreshold = IsNotEnabled(Preset.SGE_ST_Simple_DPS) ? EDosisHpThreshold(CurrentTarget) : 0;
        EukrasianDosisList.TryGetValue(dotAction, out ushort dotDebuffID);
        double dotRefresh = IsNotEnabled(Preset.SGE_ST_Simple_DPS) ? SGE_ST_DPS_EukrasianDosisUptime_Threshold : 2.5;
        float dotRemaining = GetStatusEffectRemainingTime(dotDebuffID, CurrentTarget);

        return ActionReady(Eukrasia) &&
               CanApplyStatus(CurrentTarget, dotDebuffID) &&
               HasBattleTarget() &&
               GetTargetHPPercent() > hpThreshold &&
               dotRemaining <= dotRefresh;
    }

    internal static int EDosisHpThreshold(IGameObject? x)
    {
        if (x is null)
            return 0;

        if (InBossEncounter())
            return x.IsBoss() ? SGE_ST_DPS_EukrasianDosisBossOption : SGE_ST_DPS_EukrasianDosisBossAddsOption;

        return SGE_ST_DPS_EukrasianDosisTrashOption;
    }

    #endregion

    #region Healing

    #region Raidwides

    private static bool RaidwideKerachole() =>
        IsEnabled(Preset.SGE_Raidwide_Kerachole) &&
        ActionReady(Kerachole) && HasAddersgallAboveHold() &&
        CanWeave() && GroupDamageIncoming();

    private static bool RaidwideHolos() =>
        IsEnabled(Preset.SGE_Raidwide_Holos) &&
        ActionReady(Holos) && CanWeave() && GroupDamageIncoming() &&
        GetPartyAvgHPPercent() <= SGE_Raidwide_HolosOption;

    private static bool RaidwideEprognosis()
    {
        bool shieldCheck = GetPartyBuffPercent(Buffs.EukrasianPrognosis) <= SGE_AoE_Heal_EPrognosisOption &&
                           GetPartyBuffPercent(SCH.Buffs.Galvanize) <= SGE_AoE_Heal_EPrognosisOption;

        return IsEnabled(Preset.SGE_Raidwide_EPrognosis) && shieldCheck && GroupDamageIncoming() && LevelChecked(Eukrasia);
    }

    #endregion

    #region ST

    private static int GetMatchingConfigST(int i, IGameObject? target, out uint action, out bool enabled)
    {
        IGameObject? healTarget = target ?? SimpleTarget.Stack.AllyToHeal;

        bool shieldCheck = !SGE_ST_Heal_EDiagnosisOpts[0] ||
                           !HasStatusEffect(Buffs.EukrasianDiagnosis, healTarget, true) &&
                           !HasStatusEffect(Buffs.EukrasianPrognosis, healTarget, true);

        bool scholarShieldCheck = !SGE_ST_Heal_EDiagnosisOpts[1] ||
                                  !HasStatusEffect(SCH.Buffs.Galvanize);
        bool tankCheck = healTarget.IsInParty() && healTarget.Role is CombatRole.Tank;

        switch (i)
        {
            case 0:
                action = Soteria;
                enabled = IsEnabled(Preset.SGE_ST_Heal_Soteria);
                return SGE_ST_Heal_Soteria;

            case 1:
                action = Zoe;
                enabled = IsEnabled(Preset.SGE_ST_Heal_Zoe);
                return SGE_ST_Heal_Zoe;

            case 2:
                action = Pepsis;

                enabled = IsEnabled(Preset.SGE_ST_Heal_Pepsis) &&
                          HasStatusEffect(Buffs.EukrasianDiagnosis, healTarget);
                return SGE_ST_Heal_Pepsis;

            case 3:
                action = Taurochole;
                enabled = IsEnabled(Preset.SGE_ST_Heal_Taurochole) && HasAddersgallAboveHold() &&
                          (tankCheck || !IsInParty() || !SGE_ST_Heal_Taurochole_TankOnly);
                return SGE_ST_Heal_Taurochole;

            case 4:
                action = Haima;
                enabled = IsEnabled(Preset.SGE_ST_Heal_Haima) &&
                          (!SGE_ST_Heal_HaimaBossOption || !InBossEncounter()) &&
                          (tankCheck || !IsInParty() || !SGE_ST_Heal_Haima_TankOnly);
                return SGE_ST_Heal_Haima;

            case 5:
                action = Krasis;
                enabled = IsEnabled(Preset.SGE_ST_Heal_Krasis) &&
                          (!SGE_ST_Heal_KrasisBossOption || !InBossEncounter()) &&
                          (tankCheck || !IsInParty() || !SGE_ST_Heal_Krasis_TankOnly);
                return SGE_ST_Heal_Krasis;

            case 6:
                action = Druochole;
                enabled = IsEnabled(Preset.SGE_ST_Heal_Druochole) && HasAddersgallAboveHold();
                return SGE_ST_Heal_Druochole;

            case 7:
                action = Eukrasia;
                enabled = IsEnabled(Preset.SGE_ST_Heal_EDiagnosis) &&
                          GetTargetHPPercent(healTarget, SGE_ST_Heal_IncludeShields) <= SGE_ST_Heal_EDiagnosisHP &&
                          shieldCheck && scholarShieldCheck;
                return SGE_ST_Heal_EDiagnosisHP;

            case 8:
                action = Kerachole;
                enabled = IsEnabled(Preset.SGE_ST_Heal_Kerachole) && HasAddersgallAboveHold() &&
                          (!SGE_ST_Heal_KeracholeBossOption || !InBossEncounter());
                return SGE_ST_Heal_KeracholeHP;

            case 9:
                action = OriginalHook(Physis);
                enabled = IsEnabled(Preset.SGE_ST_Heal_Physis) &&
                          (!SGE_ST_Heal_PhysisBossOption || !InBossEncounter());
                return SGE_ST_Heal_PhysisHP;

            case 10:
                action = Panhaima;
                enabled = IsEnabled(Preset.SGE_ST_Heal_Panhaima) &&
                          (!SGE_ST_Heal_PanhaimaBossOption || !InBossEncounter());
                return SGE_ST_Heal_PanhaimaHP;

            case 11:
                action = Holos;
                enabled = IsEnabled(Preset.SGE_ST_Heal_Holos) &&
                          (!SGE_ST_Heal_HolosBossOption || !InBossEncounter());
                return SGE_ST_Heal_HolosHP;
        }

        enabled = false;
        action = 0;

        return 0;
    }

    #endregion

    #region AoE

    private static int GetMatchingConfigAoE(int i, out uint action, out bool enabled)
    {
        bool shieldCheck = GetPartyBuffPercent(Buffs.EukrasianPrognosis) <= SGE_AoE_Heal_EPrognosisOption &&
                           GetPartyBuffPercent(SCH.Buffs.Galvanize) <= SGE_AoE_Heal_EPrognosisOption;

        bool anyPanhaima = !SGE_ST_Heal_PanhaimaOpts[0] ||
                           !HasStatusEffect(Buffs.Panhaima, null, true);
        switch (i)
        {
            case 0:
                action = Kerachole;
                enabled = IsEnabled(Preset.SGE_AoE_Heal_Kerachole) &&
                          (!SGE_AoE_Heal_KeracholeTrait ||
                           SGE_AoE_Heal_KeracholeTrait && TraitLevelChecked(Traits.EnhancedKerachole)) &&
                          HasAddersgallAboveHold();
                return SGE_AoE_Heal_KeracholeOption;

            case 1:
                action = Ixochole;
                enabled = IsEnabled(Preset.SGE_AoE_Heal_Ixochole) &&
                          HasAddersgallAboveHold();
                return SGE_AoE_Heal_IxocholeOption;

            case 2:
                action = OriginalHook(Physis);
                enabled = IsEnabled(Preset.SGE_AoE_Heal_Physis);
                return SGE_AoE_Heal_PhysisOption;

            case 3:
                action = Holos;
                enabled = IsEnabled(Preset.SGE_AoE_Heal_Holos);
                return SGE_AoE_Heal_HolosOption;

            case 4:
                action = Panhaima;
                enabled = IsEnabled(Preset.SGE_AoE_Heal_Panhaima) && anyPanhaima;
                return SGE_AoE_Heal_PanhaimaOption;

            case 5:
                action = Pepsis;
                enabled = IsEnabled(Preset.SGE_AoE_Heal_Pepsis) &&
                          HasStatusEffect(Buffs.EukrasianPrognosis);
                return SGE_AoE_Heal_PepsisOption;

            case 6:
                action = Philosophia;
                enabled = IsEnabled(Preset.SGE_AoE_Heal_Philosophia);
                return SGE_AoE_Heal_PhilosophiaOption;

            case 7:
                action = Zoe;
                enabled = IsEnabled(Preset.SGE_AoE_Heal_Zoe);
                return SGE_AoE_Heal_ZoeOption;

            case 8:
                action = Eukrasia;
                enabled = IsEnabled(Preset.SGE_AoE_Heal_EPrognosis)
                          && shieldCheck;
                return 100; //Don't HP Check
        }

        enabled = false;
        action = 0;
        return 0;
    }

    #endregion

    #endregion

    #region DPS

    private static bool CanRaidwide(out uint action)
    {
        if (RaidwideKerachole())
        {
            action = Kerachole;
            return true;
        }

        if (RaidwideHolos())
        {
            action = Holos;
            return true;
        }

        if (RaidwideEprognosis())
        {
            action = HasStatusEffect(Buffs.Eukrasia)
                ? OriginalHook(Prognosis)
                : Eukrasia;
            return true;
        }

        action = 0;
        return false;
    }

    private static bool CanDpsWeave(bool simpleMode, bool onAoE, uint[] retargetIds, out uint action)
    {
        action = 0;

        if (!CanWeave())
            return false;

        if (!onAoE && HasStatusEffect(Buffs.Eukrasia))
            return false;

        int lucidMp = simpleMode
            ? 7500
            : onAoE
                ? SGE_AoE_DPS_Lucid
                : SGE_ST_DPS_Lucid;
        Preset lucidPreset = onAoE ? Preset.SGE_AoE_DPS_Lucid : Preset.SGE_ST_DPS_Lucid;
        if ((simpleMode || IsEnabled(lucidPreset)) &&
            Role.CanLucidDream(lucidMp))
        {
            action = Role.LucidDreaming;
            return true;
        }

        int addersgallProtect = simpleMode
            ? 3
            : onAoE
                ? SGE_AoE_DPS_AddersgallProtect
                : SGE_ST_DPS_AddersgallProtect;
        Preset protectPreset = onAoE
            ? Preset.SGE_AoE_DPS_AddersgallProtect
            : Preset.SGE_ST_DPS_AddersgallProtect;
        if ((simpleMode || IsEnabled(protectPreset)) &&
            ActionReady(Druochole) && Addersgall >= addersgallProtect)
        {
            action = Druochole.RetargetIfEnabled(retargetIds);
            return true;
        }

        Preset psychePreset = onAoE ? Preset.SGE_AoE_DPS_Psyche : Preset.SGE_ST_DPS_Psyche;
        if ((simpleMode || IsEnabled(psychePreset)) &&
            ActionReady(Psyche) &&
            (onAoE
                ? HasBattleTarget() && InActionRange(Psyche)
                : InCombat()))
        {
            action = Psyche;
            return true;
        }

        int rhizoThreshold = simpleMode
            ? 1
            : onAoE
                ? SGE_AoE_DPS_Rhizo
                : SGE_ST_DPS_Rhizo;
        Preset rhizoPreset = onAoE ? Preset.SGE_AoE_DPS_Rhizo : Preset.SGE_ST_DPS_Rhizo;
        if ((simpleMode || IsEnabled(rhizoPreset)) &&
            ActionReady(Rhizomata) && Addersgall < rhizoThreshold)
        {
            action = Rhizomata;
            return true;
        }

        Preset soteriaPreset = onAoE ? Preset.SGE_AoE_DPS_Soteria : Preset.SGE_ST_DPS_Soteria;
        if ((simpleMode || IsEnabled(soteriaPreset)) &&
            ActionReady(Soteria) && HasStatusEffect(Buffs.Kardia))
        {
            action = Soteria;
            return true;
        }

        return false;
    }

    private static bool CanPhlegma(bool simpleMode, out uint action)
    {
        action = 0;

        if ((!simpleMode && !IsEnabled(Preset.SGE_ST_DPS_Phlegma)) ||
            !InActionRange(OriginalHook(Phlegma)) ||
            !ActionReady(OriginalHook(Phlegma)))
            return false;

        bool burst = simpleMode || SGE_ST_DPS_Phlegma_Burst;
        int chargePool = simpleMode ? 1 : SGE_ST_DPS_Phlegma;

        if ((!burst || !LevelChecked(Psyche)) &&
            GetRemainingCharges(OriginalHook(Phlegma)) > chargePool)
        {
            action = OriginalHook(Phlegma);
            return true;
        }

        if (burst &&
            (GetCooldownRemainingTime(Psyche) > 40 && IsPhlegmaCapped ||
             IsOffCooldown(Psyche) ||
             JustUsed(Psyche, 5f)))
        {
            action = OriginalHook(Phlegma);
            return true;
        }

        return false;
    }

    private static bool CanMovementGCD(bool simpleMode, out uint action)
    {
        action = 0;

        if (!IsMoving())
            return false;

        if (simpleMode)
        {
            if (ActionReady(OriginalHook(Toxikon)) && HasAddersting())
            {
                action = OriginalHook(Toxikon);
                return true;
            }

            if (ActionReady(Dyskrasia) && InActionRange(Dyskrasia))
            {
                action = OriginalHook(Dyskrasia);
                return true;
            }

            return false;
        }

        if (!IsEnabled(Preset.SGE_ST_DPS_Movement))
            return false;

        foreach(int priority in SGE_ST_DPS_Movement_Priority.OrderBy(x => x))
        {
            int index = SGE_ST_DPS_Movement_Priority.IndexOf(priority);
            if (CanMovementOption(index, out action))
                return true;
        }

        return false;
    }

    private static bool CanEDosis(bool simpleMode, uint[] retargetIds, out uint action)
    {
        action = 0;
        uint dotAction = OriginalHook(Dosis);
        DosisList.TryGetValue(dotAction, out (ushort Debuff, uint Eukrasian) debuff);

        if (simpleMode)
        {
            IGameObject? target = SimpleTarget.DottableEnemy(debuff.Eukrasian, debuff.Debuff, 0, 3, 99);
            if (target is not null && CanApplyStatus(target, debuff.Debuff) &&
                !JustUsedOn(debuff.Eukrasian, target) && LevelChecked(Eukrasia))
            {
                action = HasStatusEffect(Buffs.Eukrasia)
                    ? dotAction.Retarget(retargetIds, target)
                    : Eukrasia;
                return true;
            }

            return false;
        }

        if (!IsEnabled(Preset.SGE_ST_DPS_EDosis) || !PartyInCombat())
            return false;

        if (ShouldRefreshEDosis())
        {
            action = HasStatusEffect(Buffs.Eukrasia) ? dotAction : Eukrasia;
            return true;
        }

        IGameObject? multiTarget = SimpleTarget.DottableEnemy(
            debuff.Eukrasian, debuff.Debuff, EDosisHpThreshold,
            SGE_ST_DPS_EukrasianDosisUptime_Threshold, 2);

        if (multiTarget is not null && CanApplyStatus(multiTarget, debuff.Debuff) &&
            !JustUsedOn(debuff.Eukrasian, multiTarget) &&
            SGE_ST_DPS_EDosis_TwoTarget && LevelChecked(Eukrasia))
        {
            action = HasStatusEffect(Buffs.Eukrasia)
                ? dotAction.Retarget(retargetIds, multiTarget)
                : Eukrasia;
            return true;
        }

        return false;
    }

    private static bool HasEDyskrasiaTargets() =>
        EnemiesInRange(EukrasianDyskrasia).Count(x =>
            (GetPossessedStatusRemainingTime(Debuffs.EukrasianDyskrasia, x) is <= 4 or float.NaN &&
             GetPossessedStatusRemainingTime(DosisList[OriginalHook(Dosis)].Debuff, x) is <= 4 or float.NaN) &&
            GetTargetHPPercent(x) > 25) >= 4;

    private static bool CanEDyskrasia(bool simpleMode, out uint action)
    {
        action = 0;

        if ((!simpleMode && !IsEnabled(Preset.SGE_AoE_DPS_EDyskrasia)) ||
            !HasEDyskrasiaTargets() ||
            JustUsed(EukrasianDyskrasia) ||
            !TraitLevelChecked(Traits.OffensiveMagicMasteryII) ||
            !ActionReady(Eukrasia))
            return false;

        action = Eukrasia;
        return true;
    }

    private static bool CanAoEDpsGCD(bool simpleMode, out uint action)
    {
        action = 0;

        if (CanEDyskrasia(simpleMode, out action))
            return true;

        if ((simpleMode || IsEnabled(Preset.SGE_AoE_DPS_Phlegma)) &&
            ActionReady(OriginalHook(Phlegma)) &&
            HasBattleTarget() &&
            InActionRange(OriginalHook(Phlegma)))
        {
            action = OriginalHook(Phlegma);
            return true;
        }

        if ((simpleMode || IsEnabled(Preset.SGE_AoE_DPS_Toxikon)) &&
            ActionReady(OriginalHook(Toxikon)) &&
            HasBattleTarget() && HasAddersting() &&
            InActionRange(OriginalHook(Toxikon)))
        {
            action = OriginalHook(Toxikon);
            return true;
        }

        if ((simpleMode || IsEnabled(Preset.SGE_AoE_DPS_Pneuma)) &&
            (simpleMode || SGE_AoE_DPS_PneumaBossOption == 0 || TargetIsBoss()) &&
            ActionReady(Pneuma) && HasBattleTarget() &&
            InActionRange(Pneuma))
        {
            action = Pneuma;
            return true;
        }

        return false;
    }

    #endregion

    #region Movement Prio

    private static (uint Action, Preset Preset, System.Func<bool> Logic)[]
        PrioritizedMovement =>
    [
        (OriginalHook(Toxikon), Preset.SGE_ST_DPS_Movement,
            () => SGE_ST_DPS_Movement[0] &&
                  ActionReady(OriginalHook(Toxikon)) &&
                  HasAddersting()),
        (OriginalHook(Dyskrasia), Preset.SGE_ST_DPS_Movement,
            () => SGE_ST_DPS_Movement[1] &&
                  ActionReady(OriginalHook(Dyskrasia)) &&
                  InActionRange(OriginalHook(Dyskrasia))),
        (Eukrasia, Preset.SGE_ST_DPS_Movement,
            () => SGE_ST_DPS_Movement[2] &&
                  ActionReady(Eukrasia) &&
                  !HasStatusEffect(Buffs.Eukrasia))
    ];

    private static bool CanMovementOption
        (int index, out uint action)
    {
        action = PrioritizedMovement[index].Action;
        return ActionReady(action) && LevelChecked(action) &&
               PrioritizedMovement[index].Logic() &&
               IsEnabled(PrioritizedMovement[index].Preset);
    }

    #endregion

    #region Openers

    internal static WrathOpener Opener()
    {
        if (ToxikonOpener.LevelChecked &&
            SGE_SelectedOpener == 0)
            return ToxikonOpener;

        if (PneumaOpener.LevelChecked &&
            SGE_SelectedOpener == 1)
            return PneumaOpener;

        return WrathOpener.Dummy;
    }

    internal static SGEToxikonOpener ToxikonOpener = new();
    internal static SGEPneumaOpener PneumaOpener = new();

    internal abstract class SGEOpenerBase : WrathOpener
    {
        public override int MinOpenerLevel => 92;
        public override int MaxOpenerLevel => 109;

        public override Preset Preset => Preset.SGE_ST_DPS_Opener;

        internal override UserData ContentCheckConfig => SGE_Balance_Content;
        internal override bool IncludePot => SGE_Opener_Potion;

        public override List<(int[] Steps, Func<bool> Condition)> SkipSteps { get; set; } =
        [
            ([1], () => HasStatusEffect(Buffs.Eukrasia))
        ];

        public override List<(int[] Steps, Func<float> HoldDelay)> PrepullDelays { get; set; } =
        [
            ([1], () => CountdownRemaining - 5),
            ([2], () => CountdownRemaining - 2),
            ([3], () => CountdownRemaining - 1)
        ];

        protected static bool SharedOpenerCooldowns() =>
            GetRemainingCharges(Phlegma3) is 2 &&
            IsOffCooldown(Psyche);
    }

    internal class SGEToxikonOpener : SGEOpenerBase
    {
        public override List<uint> OpenerActions { get; set; } =
        [
            Eukrasia,
            Items.UseItem(Items.GetStrongestPotionRow(Items.PotionType.Mind)),
            Toxikon2,
            EukrasianDosis3,
            Dosis3,
            Dosis3,
            Dosis3,
            Phlegma3,
            Psyche,
            Phlegma3,
            Dosis3,
            Dosis3,
            Dosis3,
            Dosis3,
            Eukrasia,
            EukrasianDosis3,
            Dosis3,
            Dosis3,
            Dosis3
        ];

        public override bool HasCooldowns() =>
            SharedOpenerCooldowns() &&
            HasAddersting();
    }

    internal class SGEPneumaOpener : SGEOpenerBase
    {
        public override List<uint> OpenerActions { get; set; } =
        [
            Eukrasia,
            Items.UseItem(Items.GetStrongestPotionRow(Items.PotionType.Mind)),
            Pneuma,
            EukrasianDosis3,
            Dosis3,
            Dosis3,
            Dosis3,
            Phlegma3,
            Psyche,
            Phlegma3,
            Dosis3,
            Dosis3,
            Dosis3,
            Dosis3,
            Eukrasia,
            EukrasianDosis3,
            Dosis3,
            Dosis3,
            Dosis3
        ];

        public override bool HasCooldowns() =>
            SharedOpenerCooldowns() &&
            IsOffCooldown(Pneuma);
    }

    #endregion

    #region ID's

    // Actions
    internal const uint

        // Heals and Shields
        Diagnosis = 24284,
        Prognosis = 24286,
        Physis = 24288,
        Druochole = 24296,
        Kerachole = 24298,
        Ixochole = 24299,
        Pepsis = 24301,
        Physis2 = 24302,
        Taurochole = 24303,
        Haima = 24305,
        Panhaima = 24311,
        Holos = 24310,
        EukrasianDiagnosis = 24291,
        EukrasianPrognosis = 24292,
        EukrasianPrognosis2 = 37034,
        Egeiro = 24287,

        // DPS
        Dosis = 24283,
        Dosis2 = 24306,
        Dosis3 = 24312,
        EukrasianDosis = 24293,
        EukrasianDosis2 = 24308,
        EukrasianDosis3 = 24314,
        Phlegma = 24289,
        Phlegma2 = 24307,
        Phlegma3 = 24313,
        Dyskrasia = 24297,
        Dyskrasia2 = 24315,
        Toxikon = 24304,
        Toxikon2 = 24316,
        Pneuma = 24318,
        EukrasianDyskrasia = 37032,
        Psyche = 37033,

        //Movement
        Icarus = 24295,

        // Buffs
        Soteria = 24294,
        Zoe = 24300,
        Krasis = 24317,
        Philosophia = 37035,

        // Other
        Kardia = 24285,
        Eukrasia = 24290,
        Rhizomata = 24309;

    // Action Groups


    // Debuff Pairs of Actions and Debuff


    // Action Buffs
    internal static class Buffs
    {
        internal const ushort
            Kardia = 2604,
            Kardion = 2605,
            Eukrasia = 2606,
            EukrasianDiagnosis = 2607,
            EukrasianPrognosis = 2609,
            Haima = 2612,
            Panhaima = 2613,
            Kerachole = 2618,
            Zoe = 2611,
            Holosakos = 3365,
            Eudaimonia = 3899;
    }

    internal static class Debuffs
    {
        internal const ushort
            EukrasianDosis = 2614,
            EukrasianDosis2 = 2615,
            EukrasianDosis3 = 2616,
            EukrasianDyskrasia = 3897;
    }

    internal static class Traits
    {
        internal const ushort
            Addersgall = 370,
            Addersting = 373,
            EnhancedKerachole = 375,
            OffensiveMagicMasteryII = 376;
    }

    #endregion
}
