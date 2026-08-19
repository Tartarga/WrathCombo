using Dalamud.Game.ClientState.Conditions;
using WrathCombo.Core;
using WrathCombo.CustomComboNS;
using WrathCombo.Extensions;
using WrathCombo.Native;

namespace WrathCombo.Combos.PvE;

internal partial class BLU : Caster
{
    #region DPS

    internal class BLU_ST_DPS : CustomCombo
    {
        protected internal override Preset Preset => Preset.BLU_ST_DPS;

        protected override uint Invoke(uint actionID)
        {
            if (!CustomActionHelper.OneButtonRotationChecker(actionID, CustomActionType.SingleTargetDPS, SonicBoom))
                return actionID;

            if (CustomActionHelper.CustomActionEnabled(CustomActionType.SingleTargetDPS) &&
                IsEnabled(Preset.BLU_ST_Tank) &&
                HasTankMimicry)
                return actionID;

            return DoDPS(actionID, actionID, onAoE: false);
        }
    }

    internal class BLU_AoE_DPS : CustomCombo
    {
        protected internal override Preset Preset => Preset.BLU_AoE_DPS;

        protected override uint Invoke(uint actionID)
        {
            if (!CustomActionHelper.OneButtonRotationChecker(actionID, CustomActionType.AoEDPS, Electrogenesis))
                return actionID;

            if (CustomActionHelper.CustomActionEnabled(CustomActionType.AoEDPS) &&
                IsEnabled(Preset.BLU_AoE_Tank) &&
                HasTankMimicry)
                return actionID;

            return DoDPS(actionID, actionID, onAoE: true);
        }
    }

    #endregion

    #region Tank

    internal class BLU_ST_Tank : CustomCombo
    {
        protected internal override Preset Preset => Preset.BLU_ST_Tank;

        protected override uint Invoke(uint actionID)
        {
            if (!CustomActionHelper.OneButtonRotationChecker(actionID, CustomActionType.SingleTargetDPS, GoblinPunch))
                return actionID;

            if (CustomActionHelper.CustomActionEnabled(CustomActionType.SingleTargetDPS) &&
                IsEnabled(Preset.BLU_ST_DPS) &&
                !HasTankMimicry)
                return actionID;

            return DoTank(actionID, actionID, onAoE: false);
        }
    }

    internal class BLU_AoE_Tank : CustomCombo
    {
        protected internal override Preset Preset => Preset.BLU_AoE_Tank;

        protected override uint Invoke(uint actionID)
        {
            if (!CustomActionHelper.OneButtonRotationChecker(actionID, CustomActionType.AoEDPS, RightRound))
                return actionID;

            if (CustomActionHelper.CustomActionEnabled(CustomActionType.AoEDPS) &&
                IsEnabled(Preset.BLU_AoE_DPS) &&
                !HasTankMimicry)
                return actionID;

            return DoTank(actionID, actionID, onAoE: true);
        }
    }

    #endregion

    #region Healer

    internal class BLU_ST_Heal : CustomCombo
    {
        protected internal override Preset Preset => Preset.BLU_ST_Heal;

        protected override uint Invoke(uint actionID)
        {
            if (!CustomActionHelper.OneButtonRotationChecker(actionID, CustomActionType.SingleTargetHeals, PomCure))
                return actionID;

            return DoHeal(actionID, onAoE: false);
        }
    }

    internal class BLU_AoE_Heal : CustomCombo
    {
        protected internal override Preset Preset => Preset.BLU_AoE_Heal;

        protected override uint Invoke(uint actionID)
        {
            if (!CustomActionHelper.OneButtonRotationChecker(actionID, CustomActionType.AoEHeals, WhiteWind))
                return actionID;

            return DoHeal(actionID, onAoE: true);
        }
    }

    #endregion

    #region Miscellaneous

    internal class BLU_BuffedSoT : CustomCombo
    {
        protected internal override Preset Preset => Preset.BLU_BuffedSoT;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is SongOfTorment)
            {
                if (!HasStatusEffect(Buffs.Bristle) && IsSpellActive(Bristle))
                    return Bristle;
                if (IsSpellActive(SongOfTorment))
                    return SongOfTorment;
            }

            return actionID;
        }
    }

    internal class BLU_Opener : CustomCombo
    {
        protected internal override Preset Preset => Preset.BLU_Opener;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is MoonFlute)
            {
                //If Triple Trident is saved for Crit/Det builds
                if (GetCooldownRemainingTime(TripleTrident) <= 3 && IsSpellActive(TripleTrident))
                {
                    if (!HasStatusEffect(Buffs.Whistle) && IsSpellActive(Whistle) && !WasLastSpell(Whistle) && IsOffCooldown(JKick))
                        return Whistle;
                    if (!HasStatusEffect(Buffs.Tingle) && IsSpellActive(Tingle) && !WasLastSpell(Tingle) && IsOffCooldown(JKick))
                        return Tingle;
                    if (!HasStatusEffect(Buffs.MoonFlute) && !HasStatusEffect(Buffs.WaningNocturne) && IsSpellActive(MoonFlute) && !WasLastSpell(MoonFlute) && !JustUsed(MoonFlute))
                        return MoonFlute;
                    if (IsOffCooldown(JKick) && IsSpellActive(JKick))
                        return JKick;
                    if (IsOffCooldown(TripleTrident))
                        return TripleTrident;
                }

                //If Triple Trident is used on CD for Crit/Sps builds or Triple Trident isn't active
                if ((GetCooldownRemainingTime(TripleTrident) > 3 && IsSpellActive(TripleTrident)) || !IsSpellActive(TripleTrident))
                {
                    if (!HasStatusEffect(Buffs.Whistle) && IsOffCooldown(JKick) && !WasLastSpell(Whistle) && IsSpellActive(Whistle) && IsOffCooldown(JKick))
                        return Whistle;
                    if (!HasStatusEffect(Buffs.Tingle) && IsSpellActive(Tingle) && !WasLastSpell(Tingle) && IsOffCooldown(JKick))
                        return Tingle;
                    if (!HasStatusEffect(Buffs.MoonFlute) && !HasStatusEffect(Buffs.WaningNocturne) && IsSpellActive(MoonFlute) && !JustUsed(MoonFlute))
                        return MoonFlute;
                    if (IsOffCooldown(JKick) && IsSpellActive(JKick))
                        return JKick;
                }

                if (IsOffCooldown(Nightbloom) && IsSpellActive(Nightbloom))
                    return Nightbloom;
                if (IsOffCooldown(RoseOfDestruction) && IsSpellActive(RoseOfDestruction))
                    return RoseOfDestruction;
                if (IsOffCooldown(FeatherRain) && IsSpellActive(FeatherRain))
                    return FeatherRain.Retarget(MoonFlute,
                        SimpleTarget.HardTarget.IfHostile() ??
                        SimpleTarget.LastHostileHardTarget);
                if (IsOffCooldown(Eruption) && IsSpellActive(Eruption))
                    return Eruption;
                if (!HasStatusEffect(Buffs.Bristle) && IsOffCooldown(Role.Swiftcast) && IsSpellActive(Bristle))
                    return Bristle;
                if (IsOffCooldown(Role.Swiftcast) && LevelChecked(Role.Swiftcast))
                    return Role.Swiftcast;
                if (IsOffCooldown(GlassDance) && IsSpellActive(GlassDance))
                    return GlassDance;
                if (GetCooldownRemainingTime(Surpanakha) < 95 && IsSpellActive(Surpanakha))
                    return Surpanakha;
                if (IsOffCooldown(MatraMagic) && HasStatusEffect(Buffs.DPSMimicry) && IsSpellActive(MatraMagic))
                    return MatraMagic;
                if (IsOffCooldown(ShockStrike) && IsSpellActive(ShockStrike))
                    return ShockStrike;
                if ((IsOffCooldown(PhantomFlurry) && IsSpellActive(PhantomFlurry)) || HasStatusEffect(Buffs.PhantomFlurry))
                    return PhantomFlurry;
            }

            return actionID;
        }
    }

    internal class BLU_FinalSting : CustomCombo
    {
        protected internal override Preset Preset => Preset.BLU_FinalSting;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is FinalSting)
            {
                if (IsEnabled(Preset.BLU_SoloMode) && HasCondition(ConditionFlag.BoundByDuty) && !HasStatusEffect(Buffs.BasicInstinct) && GetPartyMembers().Count == 0 && IsSpellActive(BasicInstinct))
                    return BasicInstinct;
                if (!HasStatusEffect(Buffs.Whistle) && IsSpellActive(Whistle) && !WasLastAction(Whistle))
                    return Whistle;
                if (!HasStatusEffect(Buffs.Tingle) && IsSpellActive(Tingle) && !WasLastSpell(Tingle))
                    return Tingle;
                if (!HasStatusEffect(Buffs.MoonFlute) && !WasLastSpell(MoonFlute) && IsSpellActive(MoonFlute))
                    return MoonFlute;
                if (IsEnabled(Preset.BLU_Primals))
                {
                    if (IsOffCooldown(RoseOfDestruction) && IsSpellActive(RoseOfDestruction))
                        return RoseOfDestruction;
                    if (IsOffCooldown(FeatherRain) && IsSpellActive(FeatherRain))
                        return FeatherRain.Retarget(FinalSting,
                            SimpleTarget.HardTarget.IfHostile() ??
                            SimpleTarget.LastHostileHardTarget);
                    if (IsOffCooldown(Eruption) && IsSpellActive(Eruption))
                        return Eruption;
                    if (IsOffCooldown(MatraMagic) && IsSpellActive(MatraMagic))
                        return MatraMagic;
                    if (IsOffCooldown(GlassDance) && IsSpellActive(GlassDance))
                        return GlassDance;
                    if (IsOffCooldown(ShockStrike) && IsSpellActive(ShockStrike))
                        return ShockStrike;
                }

                if (IsOffCooldown(Role.Swiftcast) && LevelChecked(Role.Swiftcast))
                    return Role.Swiftcast;
                if (IsSpellActive(FinalSting))
                    return FinalSting;
            }

            return actionID;
        }
    }

    internal class BLU_Ultravibrate : CustomCombo
    {
        protected internal override Preset Preset => Preset.BLU_Ultravibrate;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is Ultravibration)
            {
                if (IsEnabled(Preset.BLU_HydroPull) && !InMeleeRange() && IsSpellActive(HydroPull))
                    return HydroPull;
                if (!HasStatusEffect(Debuffs.DeepFreeze, CurrentTarget, true) && IsOffCooldown(Ultravibration) && IsSpellActive(RamsVoice))
                    return RamsVoice;

                if (HasStatusEffect(Debuffs.DeepFreeze, CurrentTarget, true))
                {
                    if (IsOffCooldown(Role.Swiftcast))
                        return Role.Swiftcast;
                    if (IsSpellActive(Ultravibration) && IsOffCooldown(Ultravibration))
                        return Ultravibration;
                }
            }

            return actionID;
        }
    }

    internal class BLU_DebuffCombo : CustomCombo
    {
        protected internal override Preset Preset => Preset.BLU_DebuffCombo;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is Devour or Offguard or BadBreath)
            {
                if (!HasStatusEffect(Debuffs.Offguard, CurrentTarget, true) && IsOffCooldown(Offguard) && IsSpellActive(Offguard))
                    return Offguard;
                if (!HasStatusEffect(Debuffs.Malodorous, CurrentTarget, true) && HasStatusEffect(Buffs.TankMimicry) && IsSpellActive(BadBreath))
                    return BadBreath;
                if (IsOffCooldown(Devour) && HasStatusEffect(Buffs.TankMimicry) && IsSpellActive(Devour))
                    return Devour;
                if (Role.CanLucidDream(9000))
                    return Role.LucidDreaming;
            }

            return actionID;
        }
    }

    internal class BLU_Addle : CustomCombo
    {
        protected internal override Preset Preset => Preset.BLU_Addle;

        protected override uint Invoke(uint actionID) => (actionID is MagicHammer && IsOnCooldown(MagicHammer) && IsOffCooldown(Role.Addle) && !HasStatusEffect(Role.Debuffs.Addle, CurrentTarget) && !HasStatusEffect(Debuffs.Conked, CurrentTarget)) ? Role.Addle : actionID;
    }

    internal class BLU_PrimalCombo : CustomCombo
    {
        protected internal override Preset Preset => Preset.BLU_PrimalCombo;
        internal static bool surpanakhaReady = false;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is FeatherRain or Eruption)
            {
                if (HasStatusEffect(Buffs.PhantomFlurry))
                    return OriginalHook(PhantomFlurry);

                if (!HasStatusEffect(Buffs.PhantomFlurry))
                {
                    if (IsEnabled(Preset.BLU_PrimalCombo_WingedReprobation) && GetStatusEffect(Buffs.WingedReprobation)?.Param > 1 && IsOffCooldown(WingedReprobation))
                        return OriginalHook(WingedReprobation);

                    if (IsOffCooldown(FeatherRain) && IsSpellActive(FeatherRain) &&
                        (IsNotEnabled(Preset.BLU_PrimalCombo_Pool) || (IsEnabled(Preset.BLU_PrimalCombo_Pool) && (GetCooldownRemainingTime(Nightbloom) > 30 || IsOffCooldown(Nightbloom)))))
                        return FeatherRain.Retarget([FeatherRain, Eruption],
                            SimpleTarget.HardTarget.IfHostile() ??
                            SimpleTarget.LastHostileHardTarget);
                    if (IsOffCooldown(Eruption) && IsSpellActive(Eruption) &&
                        (IsNotEnabled(Preset.BLU_PrimalCombo_Pool) || (IsEnabled(Preset.BLU_PrimalCombo_Pool) && (GetCooldownRemainingTime(Nightbloom) > 30 || IsOffCooldown(Nightbloom)))))
                        return Eruption;
                    if (IsOffCooldown(ShockStrike) && IsSpellActive(ShockStrike) &&
                        (IsNotEnabled(Preset.BLU_PrimalCombo_Pool) || (IsEnabled(Preset.BLU_PrimalCombo_Pool) && (GetCooldownRemainingTime(Nightbloom) > 60 || IsOffCooldown(Nightbloom)))))
                        return ShockStrike;
                    if (IsOffCooldown(RoseOfDestruction) && IsSpellActive(RoseOfDestruction) &&
                        (IsNotEnabled(Preset.BLU_PrimalCombo_Pool) || (IsEnabled(Preset.BLU_PrimalCombo_Pool) && (GetCooldownRemainingTime(Nightbloom) > 30 || IsOffCooldown(Nightbloom)))))
                        return RoseOfDestruction;
                    if (IsOffCooldown(GlassDance) && IsSpellActive(GlassDance) &&
                        (IsNotEnabled(Preset.BLU_PrimalCombo_Pool) || (IsEnabled(Preset.BLU_PrimalCombo_Pool) && (GetCooldownRemainingTime(Nightbloom) > 90 || IsOffCooldown(Nightbloom)))))
                        return GlassDance;
                    if (IsEnabled(Preset.BLU_PrimalCombo_JKick) && IsOffCooldown(JKick) && IsSpellActive(JKick) &&
                        (IsNotEnabled(Preset.BLU_PrimalCombo_Pool) || (IsEnabled(Preset.BLU_PrimalCombo_Pool) && (GetCooldownRemainingTime(Nightbloom) > 60 || IsOffCooldown(Nightbloom)))))
                        return JKick;
                    if (IsEnabled(Preset.BLU_PrimalCombo_Nightbloom) && IsOffCooldown(Nightbloom) && IsSpellActive(Nightbloom))
                        return Nightbloom;
                    if (IsEnabled(Preset.BLU_PrimalCombo_Matra) && IsOffCooldown(MatraMagic) && IsSpellActive(MatraMagic))
                        return MatraMagic;
                    if (IsEnabled(Preset.BLU_PrimalCombo_Suparnakha) && IsSpellActive(Surpanakha))
                    {
                        if (GetRemainingCharges(Surpanakha) == 4)
                            surpanakhaReady = true;
                        if (surpanakhaReady && GetRemainingCharges(Surpanakha) > 0)
                            return Surpanakha;
                        if (GetRemainingCharges(Surpanakha) == 0)
                            surpanakhaReady = false;
                    }

                    if (IsEnabled(Preset.BLU_PrimalCombo_WingedReprobation) && IsSpellActive(WingedReprobation) && IsOffCooldown(WingedReprobation))
                        return OriginalHook(WingedReprobation);

                    if (IsEnabled(Preset.BLU_PrimalCombo_SeaShanty) && IsSpellActive(SeaShanty) && IsOffCooldown(SeaShanty))
                        return SeaShanty;

                    if (IsEnabled(Preset.BLU_PrimalCombo_PhantomFlurry) && IsOffCooldown(PhantomFlurry) && IsSpellActive(PhantomFlurry))
                        return PhantomFlurry;
                }
            }

            return actionID;
        }
    }

    internal class BLU_KnightCombo : CustomCombo
    {
        protected internal override Preset Preset => Preset.BLU_KnightCombo;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is WhiteKnightsTour or BlackKnightsTour)
            {
                if (HasStatusEffect(Debuffs.Slow, CurrentTarget) && IsSpellActive(BlackKnightsTour))
                    return BlackKnightsTour;
                if (HasStatusEffect(Debuffs.Bind, CurrentTarget) && IsSpellActive(WhiteKnightsTour))
                    return WhiteKnightsTour;
            }

            return actionID;
        }
    }

    internal class BLU_LightHeadedCombo : CustomCombo
    {
        protected internal override Preset Preset => Preset.BLU_LightHeadedCombo;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is PeripheralSynthesis)
            {
                if (!HasStatusEffect(Debuffs.Lightheaded, CurrentTarget) && IsSpellActive(PeripheralSynthesis))
                    return PeripheralSynthesis;
                if (HasStatusEffect(Debuffs.Lightheaded, CurrentTarget) && IsSpellActive(MustardBomb))
                    return MustardBomb;
            }

            return actionID;
        }
    }

    internal class BLU_PerpetualRayStunCombo : CustomCombo
    {
        protected internal override Preset Preset => Preset.BLU_PerpetualRayStunCombo;

        protected override uint Invoke(uint actionID) => (actionID is PerpetualRay && (HasStatusEffect(Debuffs.Stun, CurrentTarget, true) || WasLastAction(PerpetualRay)) && IsSpellActive(SharpenedKnife) && InMeleeRange()) ? SharpenedKnife : actionID;
    }

    internal class BLU_MeleeCombo : CustomCombo
    {
        protected internal override Preset Preset => Preset.BLU_MeleeCombo;

        protected override uint Invoke(uint actionID) => (actionID is SonicBoom && InActionRange(SharpenedKnife) && IsSpellActive(SharpenedKnife)) ? SharpenedKnife : actionID;
    }

    internal class BLU_PeatClean : CustomCombo
    {
        protected internal override Preset Preset => Preset.BLU_PeatClean;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is DeepClean)
            {
                if (IsSpellActive(PeatPelt) && !HasStatusEffect(Debuffs.Begrimed, CurrentTarget))
                    return PeatPelt;
            }

            return actionID;
        }
    }
    internal class BLU_NewMoonFluteOpener : CustomCombo
    {
        protected internal override Preset Preset => Preset.BLU_NewMoonFluteOpener;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is MoonFlute)
            {
                if (!HasStatusEffect(Buffs.MoonFlute))
                {
                    if (IsSpellActive(Whistle) && !HasStatusEffect(Buffs.Whistle) && !WasLastAction(Whistle))
                        return Whistle;

                    if (IsSpellActive(Tingle) && !HasStatusEffect(Buffs.Tingle))
                        return Tingle;

                    if (IsSpellActive(RoseOfDestruction) && GetCooldown(RoseOfDestruction).CooldownRemaining < 1f)
                        return RoseOfDestruction;

                    if (IsSpellActive(MoonFlute) && !JustUsed(MoonFlute))
                        return MoonFlute;
                }

                if (IsSpellActive(JKick) && IsOffCooldown(JKick))
                    return JKick;

                if (IsSpellActive(TripleTrident) && IsOffCooldown(TripleTrident))
                    return TripleTrident;

                if (IsSpellActive(Nightbloom) && IsOffCooldown(Nightbloom))
                    return Nightbloom;

                if (IsEnabled(Preset.BLU_NewMoonFluteOpener_DoTOpener))
                {
                    if ((!HasStatusEffect(Debuffs.BreathOfMagic, CurrentTarget, true) && IsSpellActive(BreathOfMagic)) || (!HasStatusEffect(Debuffs.MortalFlame, CurrentTarget, true) && IsSpellActive(MortalFlame)))
                    {
                        if (IsSpellActive(Bristle) && !HasStatusEffect(Buffs.Bristle))
                            return Bristle;

                        if (IsSpellActive(FeatherRain) && IsOffCooldown(FeatherRain))
                            return FeatherRain.Retarget(MoonFlute,
                                SimpleTarget.HardTarget.IfHostile() ??
                                SimpleTarget.LastHostileHardTarget);

                        if (IsSpellActive(SeaShanty) && IsOffCooldown(SeaShanty))
                            return SeaShanty;

                        if (IsSpellActive(BreathOfMagic) && !HasStatusEffect(Debuffs.BreathOfMagic, CurrentTarget, true))
                            return BreathOfMagic;
                        else if (IsSpellActive(MortalFlame) && !HasStatusEffect(Debuffs.MortalFlame, CurrentTarget, true))
                            return MortalFlame;
                    }
                }
                else
                {
                    if (IsSpellActive(WingedReprobation) && IsOffCooldown(WingedReprobation) && !WasLastSpell(WingedReprobation) && !WasLastAbility(FeatherRain) && (!HasStatusEffect(Buffs.WingedReprobation) || GetStatusEffect(Buffs.WingedReprobation)?.Param < 2))
                        return WingedReprobation;

                    if (IsSpellActive(FeatherRain) && IsOffCooldown(FeatherRain))
                        return FeatherRain.Retarget(MoonFlute,
                            SimpleTarget.HardTarget.IfHostile() ??
                            SimpleTarget.LastHostileHardTarget);

                    if (IsSpellActive(SeaShanty) && IsOffCooldown(SeaShanty))
                        return SeaShanty;
                }

                if (IsSpellActive(WingedReprobation) && IsOffCooldown(WingedReprobation) && !WasLastAbility(ShockStrike) && GetStatusEffect(Buffs.WingedReprobation)?.Param < 2)
                    return WingedReprobation;

                if (IsSpellActive(ShockStrike) && IsOffCooldown(ShockStrike))
                    return ShockStrike;

                if (IsSpellActive(BeingMortal) && IsOffCooldown(BeingMortal) && IsNotEnabled(Preset.BLU_NewMoonFluteOpener_DoTOpener))
                    return BeingMortal;

                if (IsSpellActive(Bristle) && !HasStatusEffect(Buffs.Bristle) && IsOffCooldown(MatraMagic) && IsSpellActive(MatraMagic))
                    return Bristle;

                if (IsOffCooldown(Role.Swiftcast))
                    return Role.Swiftcast;

                if (IsSpellActive(Surpanakha))
                {
                    if (GetRemainingCharges(Surpanakha) > 0)
                        return Surpanakha;
                }

                if (IsSpellActive(MatraMagic) && HasStatusEffect(Role.Buffs.Swiftcast))
                    return MatraMagic;

                if (IsSpellActive(BeingMortal) && IsOffCooldown(BeingMortal) && IsEnabled(Preset.BLU_NewMoonFluteOpener_DoTOpener))
                    return BeingMortal;

                if (IsSpellActive(PhantomFlurry) && IsOffCooldown(PhantomFlurry))
                    return PhantomFlurry;

                if (HasStatusEffect(Buffs.PhantomFlurry) && GetStatusEffect(Buffs.PhantomFlurry)?.RemainingTime < 2)
                    return OriginalHook(PhantomFlurry);

                if (HasStatusEffect(Buffs.MoonFlute))
                    return All.Cease;
            }

            return actionID;
        }
    }

    #endregion
}
