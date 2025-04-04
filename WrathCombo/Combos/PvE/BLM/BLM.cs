using WrathCombo.CustomComboNS;
namespace WrathCombo.Combos.PvE;

internal partial class BLM : CasterJob
{
    internal class BLM_ST_SimpleMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.BLM_ST_SimpleMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Fire)
                return actionID;

            if (Variant.CanCure(CustomComboPreset.BLM_Variant_Cure, Config.BLM_VariantCure))
                return Variant.Cure;

            if (Variant.CanRampart(CustomComboPreset.BLM_Variant_Rampart))
                return Variant.Rampart;

            if (CanSpellWeave())
            {
                if (ActionReady(Amplifier) && Gauge.EnochianTimer >= 20000 &&
                    !HasMaxPolyglotStacks)
                    return Amplifier;

                if (ActionReady(LeyLines) && !HasEffect(Buffs.LeyLines))
                    return LeyLines;

                if (Gauge.InAstralFire)
                {
                    if (ActionReady(Manafont) && (JustUsed(Despair) || !LevelChecked(Despair) && CurMp == 0))
                        return Manafont;

                    if (JustUsed(Despair) && !JustUsed(Manafont) || !LevelChecked(Despair) && CurMp < Fire)
                        return Transpose;
                }

                if (Gauge.InUmbralIce)
                {
                    if (ActionReady(Role.Swiftcast) && JustUsed(Transpose))
                        return Role.Swiftcast;

                    if (JustUsed(Paradox) || !LevelChecked(Paradox) && CurMp is MP.MaxMP)
                        return Transpose;
                }
            }

            if (HasMaxPolyglotStacks && Gauge.EnochianTimer < 3000)
                return LevelChecked(Xenoglossy)
                    ? Xenoglossy
                    : Foul;

            if (HasEffect(Buffs.Thunderhead) && LevelChecked(Thunder) &&
                InBossEncounter() &&
                (ThunderDebuffST is null || ThunderDebuffST.RemainingTime <= 3))
                return OriginalHook(Thunder);

            if (IsMoving() && InCombat())
            {
                if (ActionReady(Triplecast) && !HasEffect(Buffs.Triplecast))
                    return Triplecast;

                if (ActionReady(Paradox) &&
                    Gauge.InAstralFire && Gauge.IsParadoxActive)
                    return Paradox;

                if (ActionReady(Role.Swiftcast) && !HasEffect(Buffs.Triplecast))
                    return Role.Swiftcast;

                if (HasPolyglotStacks())
                    return LevelChecked(Xenoglossy)
                        ? Xenoglossy
                        : Foul;
            }

            if (Gauge.InAstralFire)
            {
                if (LevelChecked(Amplifier) && HasPolyglotStacks() &&
                    (GetCooldownRemainingTime(Amplifier) < 3 || GetCooldownRemainingTime(Amplifier) > 100) ||
                    !LevelChecked(Amplifier) && HasPolyglotStacks())
                    return LevelChecked(Xenoglossy)
                        ? Xenoglossy
                        : Foul;

                if (Gauge.IsParadoxActive && JustUsed(Transpose, 5) &&
                    !HasEffect(Buffs.Firestarter) && (Gauge.AstralFireStacks < 3 || JustUsed(FlareStar) ||
                                                      !LevelChecked(FlareStar) && ActionReady(Despair)))
                    return Paradox;

                if (ActionReady(Fire3) && HasEffect(Buffs.Firestarter) && Gauge.AstralFireStacks < 3)
                    return Fire3;

                if (FlarestarReady)
                    return FlareStar;

                if (ActionReady(FireSpam))
                    return FireSpam;

                if (ActionReady(Despair))
                    return Despair;
            }

            if (Gauge.InUmbralIce)
            {
                if (Gauge.UmbralHearts is 3 && Gauge.IsParadoxActive)
                    return Paradox;

                if (ActionReady(Blizzard4) && Gauge.UmbralIceStacks is 3)
                    return Blizzard4;

                if (ActionReady(Blizzard3) && Gauge.UmbralIceStacks < 3)
                    return Blizzard3;
            }

            if (LevelChecked(Fire3))
            {
                return CurMp >= 7500
                    ? Fire3
                    : Blizzard3;
            }

            return actionID;
        }
    }

    internal class BLM_ST_AdvancedMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.BLM_ST_AdvancedMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Fire)
                return actionID;

            if (Variant.CanCure(CustomComboPreset.BLM_Variant_Cure, Config.BLM_VariantCure))
                return Variant.Cure;

            if (Variant.CanRampart(CustomComboPreset.BLM_Variant_Rampart))
                return Variant.Rampart;

            // Opener
            if (IsEnabled(CustomComboPreset.BLM_ST_Opener))
                if (Opener().FullOpener(ref actionID))
                    return actionID;

            if (CanSpellWeave())
            {
                if (IsEnabled(CustomComboPreset.BLM_ST_Amplifier) &&
                    ActionReady(Amplifier) && Gauge.EnochianTimer >= 20000 &&
                    !HasMaxPolyglotStacks)
                    return Amplifier;

                if (IsEnabled(CustomComboPreset.BLM_ST_LeyLines) &&
                    ActionReady(LeyLines) && !HasEffect(Buffs.LeyLines) &&
                    GetRemainingCharges(LeyLines) > Config.BLM_ST_LeyLinesCharges)
                    return LeyLines;

                if (Gauge.InAstralFire)
                {
                    if (IsEnabled(CustomComboPreset.BLM_ST_Manafont) &&
                        ActionReady(Manafont) && (JustUsed(Despair) || !LevelChecked(Despair) && CurMp == 0))
                        return Manafont;

                    if (JustUsed(Despair) && !JustUsed(Manafont) || !LevelChecked(Despair) && CurMp < Fire)
                        return Transpose;
                }

                if (Gauge.InUmbralIce)
                {
                    if (IsEnabled(CustomComboPreset.BLM_ST_Swiftcast) &&
                        ActionReady(Role.Swiftcast) && JustUsed(Transpose))
                        return Role.Swiftcast;

                    if (JustUsed(Paradox) || !LevelChecked(Paradox) && CurMp is MP.MaxMP)
                        return Transpose;
                }
            }

            if (IsEnabled(CustomComboPreset.BLM_ST_UsePolyglot) &&
                HasMaxPolyglotStacks && Gauge.EnochianTimer < 3000)
                return LevelChecked(Xenoglossy)
                    ? Xenoglossy
                    : Foul;

            if (IsEnabled(CustomComboPreset.BLM_ST_Thunder) &&
                HasEffect(Buffs.Thunderhead) && LevelChecked(Thunder) &&
                (Config.BLM_ST_Thunder_SubOption == 0 ||
                 Config.BLM_ST_Thunder_SubOption == 1 && InBossEncounter()) &&
                (ThunderDebuffST is null || ThunderDebuffST.RemainingTime <= 3))
                return OriginalHook(Thunder);

            if (IsMoving() && InCombat())
            {
                if (Config.BLM_ST_MovementOption[0] &&
                    ActionReady(Triplecast) && !HasEffect(Buffs.Triplecast))
                    return Triplecast;

                if (Config.BLM_ST_MovementOption[1] &&
                    ActionReady(Paradox) &&
                    Gauge.InAstralFire && Gauge.IsParadoxActive)
                    return Paradox;

                if (Config.BLM_ST_MovementOption[2] &&
                    ActionReady(Role.Swiftcast) && !HasEffect(Buffs.Triplecast))
                    return Role.Swiftcast;

                if (Config.BLM_ST_MovementOption[3] &&
                    HasPolyglotStacks())
                    return LevelChecked(Xenoglossy)
                        ? Xenoglossy
                        : Foul;
            }

            if (Gauge.InAstralFire)
            {
                if (IsEnabled(CustomComboPreset.BLM_ST_UsePolyglot) &&
                    LevelChecked(Amplifier) && HasPolyglotStacks() &&
                    (GetCooldownRemainingTime(Amplifier) < 3 || GetCooldownRemainingTime(Amplifier) > 100) ||
                    !LevelChecked(Amplifier) && HasPolyglotStacks())
                    return LevelChecked(Xenoglossy)
                        ? Xenoglossy
                        : Foul;

                if (Gauge.IsParadoxActive && JustUsed(Transpose, 5) &&
                    !HasEffect(Buffs.Firestarter) && (Gauge.AstralFireStacks < 3 || JustUsed(FlareStar) ||
                                                      !LevelChecked(FlareStar) && ActionReady(Despair)))
                    return Paradox;

                if (ActionReady(Fire3) && HasEffect(Buffs.Firestarter) && Gauge.AstralFireStacks < 3)
                    return Fire3;

                if (IsEnabled(CustomComboPreset.BLM_ST_FlareStar) &&
                    FlarestarReady)
                    return FlareStar;

                if (ActionReady(FireSpam))
                    return FireSpam;

                if (IsEnabled(CustomComboPreset.BLM_ST_Despair) &&
                    ActionReady(Despair))
                    return Despair;
            }
            if (Gauge.InUmbralIce)
            {
                if (Gauge.UmbralHearts is 3 && Gauge.IsParadoxActive)
                    return Paradox;

                if (ActionReady(Blizzard4) && Gauge.UmbralIceStacks is 3)
                    return Blizzard4;

                if (ActionReady(Blizzard3) && Gauge.UmbralIceStacks < 3)
                    return Blizzard3;
            }

            if (LevelChecked(Fire3))
            {
                return CurMp >= 7500
                    ? Fire3
                    : Blizzard3;
            }

            return actionID;
        }
    }

    internal class BLM_AoE_SimpleMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.BLM_AoE_SimpleMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Blizzard2 or HighBlizzard2))
                return actionID;

            if (Variant.CanCure(CustomComboPreset.BLM_Variant_Cure, Config.BLM_VariantCure))
                return Variant.Cure;

            if (Variant.CanRampart(CustomComboPreset.BLM_Variant_Rampart))
                return Variant.Rampart;

            if (CanSpellWeave())
            {
                if (ActionReady(Transpose) &&
                    (JustUsed(OriginalHook(Thunder2), 5) || JustUsed(Foul, 5)))
                    return Transpose;

                if (ActionReady(Amplifier) && RemainingPolyglotCD >= 20000)
                    return Amplifier;

                if (ActionReady(LeyLines) && !HasEffect(Buffs.LeyLines))
                    return LeyLines;

                if (Gauge.InAstralFire && ActionReady(Manafont) &&
                    (JustUsed(FlareStar, 5) || !LevelChecked(FlareStar) && CurMp is 0))
                    return Manafont;
            }

            if (HasPolyglotStacks() && ActionReady(Foul) &&
                (JustUsed(Freeze) || JustUsed(FlareStar)))
                return Foul;

            if (HasEffect(Buffs.Thunderhead) && LevelChecked(Thunder2) &&
                GetTargetHPPercent() > 5 &&
                (JustUsed(Freeze) || JustUsed(FlareStar) || !LevelChecked(Freeze)))
                return OriginalHook(Thunder2);

            if (Gauge.InAstralFire)
            {
                if (FlarestarReady)
                    return FlareStar;

                if (ActionReady(Flare))
                    return Flare;

                if (!LevelChecked(Flare) && LevelChecked(Fire2))
                    return Fire2;
            }

            if (Gauge.InUmbralIce)
            {
                if (ActionReady(Freeze))
                    return Freeze;

                if (!LevelChecked(Freeze) && ActionReady(Blizzard2))
                    return Blizzard2;
            }

            return actionID;
        }
    }

    internal class BLM_AoE_AdvancedMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.BLM_AoE_AdvancedMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Blizzard2 or HighBlizzard2))
                return actionID;

            if (Variant.CanCure(CustomComboPreset.BLM_Variant_Cure, Config.BLM_VariantCure))
                return Variant.Cure;

            if (Variant.CanRampart(CustomComboPreset.BLM_Variant_Rampart))
                return Variant.Rampart;

            if (CanSpellWeave())
            {
                if (ActionReady(Transpose) &&
                    (JustUsed(OriginalHook(Thunder2), 5) || JustUsed(Foul, 5)))
                    return Transpose;

                if (IsEnabled(CustomComboPreset.BLM_AoE_Amplifier) &&
                    ActionReady(Amplifier) && RemainingPolyglotCD >= 20000)
                    return Amplifier;

                if (IsEnabled(CustomComboPreset.BLM_AoE_LeyLines) &&
                    ActionReady(LeyLines) && !HasEffect(Buffs.LeyLines) &&
                    GetRemainingCharges(LeyLines) > Config.BLM_AoE_LeyLinesCharges)
                    return LeyLines;

                if (IsEnabled(CustomComboPreset.BLM_AoE_Manafont) &&
                    Gauge.InAstralFire && ActionReady(Manafont) &&
                    (JustUsed(FlareStar, 5) || !LevelChecked(FlareStar) && CurMp is 0))
                    return Manafont;
            }

            if (IsEnabled(CustomComboPreset.BLM_AoE_UsePolyglot) &&
                HasPolyglotStacks() && ActionReady(Foul) &&
                (JustUsed(Freeze) || JustUsed(FlareStar)))
                return Foul;

            if (IsEnabled(CustomComboPreset.BLM_AoE_Thunder) &&
                HasEffect(Buffs.Thunderhead) && LevelChecked(Thunder2) &&
                GetTargetHPPercent() > Config.BLM_AoE_ThunderHP &&
                (JustUsed(Freeze) || JustUsed(FlareStar) || !LevelChecked(Freeze)))
                return OriginalHook(Thunder2);

            if (Gauge.InAstralFire)
            {
                if (FlarestarReady)
                    return FlareStar;

                if (ActionReady(Flare))
                    return Flare;

                if (!LevelChecked(Flare) && LevelChecked(Fire2))
                    return Fire2;
            }

            if (Gauge.InUmbralIce)
            {
                if (ActionReady(Freeze))
                    return Freeze;

                if (!LevelChecked(Freeze) && ActionReady(Blizzard2))
                    return Blizzard2;
            }

            return actionID;
        }
    }

    internal class BLM_Variant_Raise : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.BLM_Variant_Raise;

        protected override uint Invoke(uint actionID) =>
            actionID is Role.Swiftcast && Variant.CanRaise(CustomComboPreset.BLM_Variant_Raise)
                ? Variant.Raise
                : actionID;
    }

    internal class BLM_Scathe_Xeno : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.BLM_Scathe_Xeno;

        protected override uint Invoke(uint actionID) =>
            actionID is Scathe && LevelChecked(Xenoglossy) && HasPolyglotStacks()
                ? Xenoglossy
                : actionID;
    }

    internal class BLM_Blizzard_1to3 : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.BLM_Blizzard_1to3;

        protected override uint Invoke(uint actionID)
        {
            switch (actionID)
            {
                case Blizzard when LevelChecked(Freeze) && !Gauge.InUmbralIce:
                    return Blizzard3;

                case Freeze when !LevelChecked(Freeze):
                    return Blizzard2;

                default:
                    return actionID;
            }
        }
    }

    internal class BLM_Fire_1to3 : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.BLM_Fire_1to3;

        protected override uint Invoke(uint actionID) =>
            actionID is Fire &&
            (LevelChecked(Fire3) && !Gauge.InAstralFire ||
             HasEffect(Buffs.Firestarter))
                ? Fire3
                : actionID;
    }

    internal class BLM_Between_The_LeyLines : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.BLM_Between_The_LeyLines;

        protected override uint Invoke(uint actionID) =>
            actionID is LeyLines && HasEffect(Buffs.LeyLines) && LevelChecked(BetweenTheLines)
                ? BetweenTheLines
                : actionID;
    }

    internal class BLM_Aetherial_Manipulation : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.BLM_Aetherial_Manipulation;

        protected override uint Invoke(uint actionID) =>
            actionID is AetherialManipulation && ActionReady(BetweenTheLines) &&
            HasEffect(Buffs.LeyLines) && !HasEffect(Buffs.CircleOfPower) && !IsMoving()
                ? BetweenTheLines
                : actionID;
    }

    internal class BLM_UmbralSoul : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.BLM_UmbralSoul;

        protected override uint Invoke(uint actionID) =>
            actionID is Transpose && Gauge.InUmbralIce && LevelChecked(UmbralSoul)
                ? UmbralSoul
                : actionID;
    }

    internal class BLM_TriplecastProtection : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.BLM_TriplecastProtection;

        protected override uint Invoke(uint actionID) =>
            actionID is Triplecast && HasEffect(Buffs.Triplecast) && LevelChecked(Triplecast)
                ? All.SavageBlade
                : actionID;
    }

    internal class BLM_FireandIce : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.BLM_FireandIce;

        protected override uint Invoke(uint actionID)
        {
            switch (actionID)
            {
                case Fire4 when Gauge.InAstralFire && LevelChecked(Fire4):
                    return Fire4;

                case Fire4 when Gauge.InUmbralIce && LevelChecked(Blizzard4):
                    return Blizzard4;

                case Flare when Gauge.InAstralFire && LevelChecked(Flare):
                    return Flare;

                case Flare when Gauge.InUmbralIce && LevelChecked(Freeze):
                    return Freeze;

                default:
                    return actionID;
            }
        }
    }

    internal class BLM_FireFlarestar : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.BLM_FireFlarestar;

        protected override uint Invoke(uint actionID) =>
            actionID == Fire4 && Gauge.InAstralFire && FlarestarReady && LevelChecked(FlareStar) ||
            actionID == Flare && Gauge.InAstralFire && FlarestarReady && LevelChecked(FlareStar)
                ? FlareStar
                : actionID;
    }
}
