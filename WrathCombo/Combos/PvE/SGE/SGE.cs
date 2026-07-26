using System.Linq;
using WrathCombo.Core;
using WrathCombo.CustomComboNS;
using WrathCombo.Native;
using static WrathCombo.Combos.PvE.SGE.Config;
using EZ = ECommons.Throttlers.EzThrottler;
using TS = System.TimeSpan;
namespace WrathCombo.Combos.PvE;

internal partial class SGE : Healer
{
    #region Simple DPS Mode

    internal class SGE_ST_Simple_DPS : CustomCombo
    {
        private static uint[] DosisActions => [.. DosisList.Keys];

        protected internal override Preset Preset => Preset.SGE_ST_Simple_DPS;

        protected override uint Invoke(uint actionID)
        {
            if (!CustomActionHelper.OneButtonRotationChecker(actionID, CustomActionType.SingleTargetDPS, DosisActions))
                return actionID;

            if (LevelChecked(Kardia) &&
                !HasStatusEffect(Buffs.Kardia) &&
                Target is not null)
                return Kardia.Retarget(actionID, Target);

            if (ContentSpecificActions.TryGet(out uint contentAction))
                return contentAction;

            if (CanDPSWeave(simpleMode: true, onAoE: false, [actionID], out uint weave))
                return weave;

            if (CanEDosis(simpleMode: true, [actionID], out uint edosis))
                return edosis;

            if (HasBattleTarget() && !HasStatusEffect(Buffs.Eukrasia) && InCombat())
            {
                if (CanPhlegma(simpleMode: true, out uint phlegma))
                    return phlegma;

                if (CanMovementGCD(simpleMode: true, out uint movement))
                    return movement;
            }

            return OriginalHook(Dosis);
        }
    }

    internal class SGE_AoE_Simple_DPS : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_AoE_Simple_DPS;

        protected override uint Invoke(uint actionID)
        {
            if (!CustomActionHelper.OneButtonRotationChecker(actionID, CustomActionType.AoEDPS, DyskrasiaList.ToArray()))
                return actionID;

            if (ContentSpecificActions.TryGet(out uint contentAction))
                return contentAction;

            if (CanDPSWeave(simpleMode: true, onAoE: true, [actionID], out uint weave))
                return weave;

            if (CanAoEDPSGCD(simpleMode: true, out uint gcd))
                return gcd;

            return OriginalHook(Dyskrasia);
        }
    }

    #endregion

    #region Advanced DPS Mode

    internal class SGE_ST_Advanced_DPS : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_ST_Advanced_DPS;

        protected override uint Invoke(uint actionID)
        {
            uint[] dosisActions = (int)SGE_ST_DPS_Advanced switch
            {
                1 => [Dosis2],
                var _ => DosisList.Keys.ToArray()
            };

            if (!CustomActionHelper.OneButtonRotationChecker(actionID, CustomActionType.SingleTargetDPS, dosisActions))
                return actionID;

            if (CustomActionHelper.CustomActionEnabled(CustomActionType.SingleTargetDPS))
                dosisActions = [All.SingleTargetDPS];

            if (IsEnabled(Preset.SGE_ST_DPS_Kardia) &&
                LevelChecked(Kardia) &&
                !HasStatusEffect(Buffs.Kardia) &&
                Target is not null)
                return Kardia.Retarget(actionID, Target);

            if (IsEnabled(Preset.SGE_ST_DPS_Opener) &&
                Opener().FullOpener(ref actionID))
                return actionID;

            if (ContentSpecificActions.TryGet(out uint contentAction))
                return contentAction;

            if (CanRaidwide(out uint raidwide))
                return raidwide;

            if (CanDPSWeave(simpleMode: false, onAoE: false, dosisActions, out uint weave))
                return weave;

            if (CanEDosis(simpleMode: false, dosisActions, out uint edosis))
                return edosis;

            if (HasBattleTarget() && !HasStatusEffect(Buffs.Eukrasia) && InCombat())
            {
                if (CanPhlegma(simpleMode: false, out uint phlegma))
                    return phlegma;

                if (CanMovementGCD(simpleMode: false, out uint movement))
                    return movement;
            }

            return OriginalHook(Dosis);
        }
    }

    internal class SGE_AoE_Advanced_DPS : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_AoE_Advanced_DPS;

        protected override uint Invoke(uint actionID)
        {
            if (!CustomActionHelper.OneButtonRotationChecker(actionID, CustomActionType.AoEDPS, DyskrasiaList.ToArray()))
                return actionID;

            if (ContentSpecificActions.TryGet(out uint contentAction))
                return contentAction;

            if (CanRaidwide(out uint raidwide))
                return raidwide;

            if (CanDPSWeave(simpleMode: false, onAoE: true, [actionID], out uint weave))
                return weave;

            if (CanAoEDPSGCD(simpleMode: false, out uint gcd))
                return gcd;

            return OriginalHook(Dyskrasia);
        }
    }

    #endregion

    #region Simple Heal Mode

    internal class SGE_ST_Simple_Heal : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_ST_Simple_Heal;

        protected override uint Invoke(uint actionID)
        {
            if (!CustomActionHelper.OneButtonRotationChecker(actionID, CustomActionType.SingleTargetHeals, Diagnosis))
                return actionID;

            return DoSTSimpleHeal(actionID);
        }
    }

    internal class SGE_AoE_Simple_Heal : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_AoE_Simple_Heal;

        protected override uint Invoke(uint actionID)
        {
            if (!CustomActionHelper.OneButtonRotationChecker(actionID, CustomActionType.AoEHeals, Prognosis))
                return actionID;

            return DoAoESimpleHeal();
        }
    }

    #endregion

    #region Advanced Heal Mode

    internal class SGE_ST_Advanced_Heal : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_ST_Advanced_Heal;

        protected override uint Invoke(uint actionID)
        {
            if (!CustomActionHelper.OneButtonRotationChecker(actionID, CustomActionType.SingleTargetHeals, Diagnosis))
                return actionID;

            return DoSTAdvancedHeal(actionID);
        }
    }

    internal class SGE_AoE_Advanced_Heal : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_AoE_Advanced_Heal;

        protected override uint Invoke(uint actionID)
        {
            if (!CustomActionHelper.OneButtonRotationChecker(actionID, CustomActionType.AoEHeals, Prognosis))
                return actionID;

            return DoAoEAdvancedHeal();
        }
    }

    #endregion

    #region Standalones

    internal class SGE_OverProtect : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_OverProtect;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Kerachole or Panhaima or Philosophia))
                return actionID;

            switch (actionID)
            {
                case Kerachole when IsEnabled(Preset.SGE_OverProtect_Kerachole) &&
                                    ActionReady(Kerachole) &&
                                    (HasStatusEffect(Buffs.Kerachole, anyOwner: true) ||
                                     IsEnabled(Preset.SGE_OverProtect_SacredSoil) && HasStatusEffect(SCH.Buffs.SacredSoil, anyOwner: true)):
                case Panhaima when IsEnabled(Preset.SGE_OverProtect_Panhaima) &&
                                   ActionReady(Panhaima) && HasStatusEffect(Buffs.Panhaima, anyOwner: true):
                    return SCH.SacredSoil;
                case Philosophia when IsEnabled(Preset.SGE_OverProtect_Philosophia) &&
                                      ActionReady(Philosophia) && HasStatusEffect(Buffs.Eudaimonia, anyOwner: true):
                    return SCH.Consolation;
                default:
                    return actionID;
            }
        }
    }

    internal class SGE_Raise : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_Raise;

        protected override uint Invoke(uint actionID)
        {
            if (actionID != Role.Swiftcast)
                return actionID;

            return IsOnCooldown(Role.Swiftcast)
                ? IsEnabled(Preset.SGE_Raise_Retarget)
                    ? Egeiro.Retarget(Role.Swiftcast,
                        SimpleTarget.Stack.AllyToRaise)
                    : Egeiro
                : actionID;
        }
    }

    internal class SGE_ZoePneuma : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_ZoePneuma;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Pneuma)
                return actionID;

            return ActionReady(Pneuma) && IsOffCooldown(Zoe)
                ? Zoe
                : actionID;
        }
    }

    internal class SGE_Rhizo : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_Rhizo;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Kerachole or Taurochole or Druochole or Ixochole))
                return actionID;

            return AddersgallList.Contains(actionID) &&
                   ActionReady(Rhizomata) && !HasAddersgall && IsOffCooldown(actionID)
                ? Rhizomata
                : actionID;
        }
    }

    internal class SGE_Eukrasia : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_Eukrasia;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Eukrasia || !HasStatusEffect(Buffs.Eukrasia))
                return actionID;

            return (int)SGE_Eukrasia_Mode switch
            {
                0 => OriginalHook(Dosis),
                1 => IsEnabled(Preset.SGE_Retarget_EukrasianDiagnosis)
                    ? EukrasianDiagnosis.Retarget(Eukrasia, HealStack)
                    : EukrasianDiagnosis,
                2 => OriginalHook(Prognosis),
                3 => OriginalHook(Dyskrasia),
                _ => actionID
            };
        }
    }

    internal class SGE_TauroDruo : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_TauroDruo;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Taurochole)
                return actionID;

            if (!LevelChecked(Taurochole) || IsOnCooldown(Taurochole))
                return IsEnabled(Preset.SGE_Retarget_Druochole)
                    ? Druochole.Retarget(Taurochole, HealStack)
                    : Druochole;

            return IsEnabled(Preset.SGE_Retarget_Taurochole)
                ? Taurochole.Retarget(HealStack)
                : Taurochole;
        }
    }

    internal class SGE_Kardia : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_Kardia;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Soteria)
                return actionID;

            if (!HasStatusEffect(Buffs.Kardia) || IsOnCooldown(Soteria))
                return IsEnabled(Preset.SGE_Retarget_Kardia)
                    ? Kardia.Retarget(actionID, HealStack)
                    : Kardia;

            return actionID;
        }
    }

    internal class SGE_Mit_ST : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_Mit_ST;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Krasis)
                return actionID;

            if (ActionReady(Krasis))
                return IsEnabled(Preset.SGE_Retarget_Krasis)
                    ? Krasis.Retarget(HealStack)
                    : actionID;

            if (!HasStatusEffect(Buffs.EukrasianDiagnosis, HealStack))
            {
                if (!HasStatusEffect(Buffs.Eukrasia))
                    return Eukrasia;

                return IsEnabled(Preset.SGE_Retarget_EukrasianDiagnosis)
                    ? EukrasianDiagnosis.Retarget(Krasis, HealStack)
                    : EukrasianDiagnosis;
            }

            if (SGE_Mit_ST_Options[0] && !ActionReady(Krasis) &&
                ActionReady(Haima))
                return IsEnabled(Preset.SGE_Retarget_Haima)
                    ? Haima.Retarget(Krasis, HealStack)
                    : Haima;

            if (SGE_Mit_ST_Options[1] && !ActionReady(Krasis) &&
                ActionReady(Taurochole) &&
                GetTargetHPPercent(HealStack) <= SGE_Mit_ST_TaurocholeThreshold)
                return IsEnabled(Preset.SGE_Retarget_Taurochole)
                    ? Taurochole.Retarget(Krasis, HealStack)
                    : Taurochole;

            return actionID;
        }
    }

    internal class SGE_Mit_AoE : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_Mit_AoE;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Holos)
                return actionID;

            if (SGE_Mit_AoE_Options[1] &&
                ActionReady(Kerachole) &&
                !HasStatusEffect(Buffs.Kerachole, anyOwner: true) &&
                !HasStatusEffect(SCH.Buffs.SacredSoil, anyOwner: true))
                return Kerachole;

            if (SGE_Mit_AoE_Options[0] &&
                ActionReady(Philosophia))
                return Philosophia;

            if (GetPartyBuffPercent(Buffs.EukrasianPrognosis) < SGE_Mit_AoE_PrognosisOption)
                return HasStatusEffect(Buffs.Eukrasia)
                    ? OriginalHook(Prognosis)
                    : Eukrasia;

            if (ActionReady(Holos) &&
                !HasStatusEffect(Buffs.Holosakos, anyOwner: true))
                return Holos;

            if (SGE_Mit_AoE_Options[2] &&
                ActionReady(Panhaima) &&
                !HasStatusEffect(Buffs.Panhaima, anyOwner: true))
                return Panhaima;

            return actionID;
        }
    }

    internal class SGE_Retarget : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_Retarget;

        protected override uint Invoke(uint actionID)
        {
            if (!EZ.Throttle("SGERetargetingFeature", TS.FromSeconds(.1)))
                return actionID;

            if (IsEnabled(Preset.SGE_Retarget_Diagnosis))
                OriginalHook(Diagnosis).Retarget(HealStack);

            if (IsEnabled(Preset.SGE_Retarget_EukrasianDiagnosis))
                EukrasianDiagnosis.Retarget(Diagnosis, HealStack);

            if (IsEnabled(Preset.SGE_Retarget_Haima))
                Haima.Retarget(HealStack);

            if (IsEnabled(Preset.SGE_Retarget_Druochole))
                Druochole.Retarget(HealStack);

            if (IsEnabled(Preset.SGE_Retarget_Taurochole))
                Taurochole.Retarget(HealStack);

            if (IsEnabled(Preset.SGE_Retarget_Krasis))
                Krasis.Retarget(HealStack);

            if (IsEnabled(Preset.SGE_Retarget_Kardia))
                Kardia.Retarget(HealStack);

            if (IsEnabled(Preset.SGE_Retarget_Icarus))
                Icarus.Retarget(SimpleTarget.Stack.MouseOver ?? SimpleTarget.HardTarget);

            return actionID;
        }
    }

    #endregion
}
