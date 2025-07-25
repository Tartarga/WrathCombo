using WrathCombo.CustomComboNS;
using static WrathCombo.Combos.PvE.SAM.Config;
namespace WrathCombo.Combos.PvE;

internal partial class SAM : Melee
{
    internal class SAM_ST_GeckoCombo : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.SAM_ST_GekkoCombo;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Gekko)
                return actionID;

            if (SAM_Gekko_KenkiOvercap && CanWeave() &&
                Kenki >= SAM_Gekko_KenkiOvercapAmount && LevelChecked(Shinten))
                return OriginalHook(Shinten);

            if (HasStatusEffect(Buffs.MeikyoShisui) && LevelChecked(Gekko))
                return OriginalHook(Gekko);

            if (ComboTimer > 0)
            {
                if (ComboAction == OriginalHook(Hakaze) && LevelChecked(Jinpu))
                    return OriginalHook(Jinpu);

                if (ComboAction is Jinpu && LevelChecked(Gekko))
                    return OriginalHook(Gekko);
            }

            return OriginalHook(Hakaze);
        }
    }

    internal class SAM_ST_KashaCombo : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.SAM_ST_KashaCombo;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Kasha)
                return actionID;

            if (SAM_Kasha_KenkiOvercap && CanWeave() &&
                Kenki >= SAM_Kasha_KenkiOvercapAmount && LevelChecked(Shinten))
                return OriginalHook(Shinten);

            if (HasStatusEffect(Buffs.MeikyoShisui) && LevelChecked(Kasha))
                return OriginalHook(Kasha);

            if (ComboTimer > 0)
            {
                if (ComboAction == OriginalHook(Hakaze) && LevelChecked(Shifu))
                    return OriginalHook(Shifu);

                if (ComboAction is Shifu && LevelChecked(Kasha))
                    return OriginalHook(Kasha);
            }

            return OriginalHook(Hakaze);
        }
    }

    internal class SAM_ST_YukikazeCombo : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.SAM_ST_YukikazeCombo;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Yukikaze)
                return actionID;

            if (SAM_Yukaze_KenkiOvercap && CanWeave() &&
                Kenki >= SAM_Yukaze_KenkiOvercapAmount && LevelChecked(Shinten))
                return OriginalHook(Shinten);

            if (HasStatusEffect(Buffs.MeikyoShisui) && LevelChecked(Yukikaze))
                return OriginalHook(Yukikaze);

            if (ComboTimer > 0 && ComboAction == OriginalHook(Hakaze) && LevelChecked(Yukikaze))
                return OriginalHook(Yukikaze);

            return OriginalHook(Hakaze);
        }
    }

    internal class SAM_ST_SimpleMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.SAM_ST_SimpleMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Hakaze or Gyofu))
                return actionID;

            //Meikyo to start before combat
            if (!HasStatusEffect(Buffs.MeikyoShisui) && ActionReady(MeikyoShisui) &&
                !InCombat() && HasBattleTarget())
                return MeikyoShisui;

            if (Variant.CanCure(CustomComboPreset.SAM_Variant_Cure, SAM_VariantCure))
                return Variant.Cure;

            if (Variant.CanRampart(CustomComboPreset.SAM_Variant_Rampart))
                return Variant.Rampart;

            if (OccultCrescent.ShouldUsePhantomActions())
                return OccultCrescent.BestPhantomAction();

            //oGCDs
            if (CanWeave() && M6SReady)
            {
                //Meikyo Features
                if (UseMeikyo())
                    return MeikyoShisui;

                //Ikishoten Features
                if (ActionReady(Ikishoten) &&
                    !HasStatusEffect(Buffs.ZanshinReady))
                {
                    switch (Kenki)
                    {
                        //Dumps Kenki in preparation for Ikishoten
                        case >= 50:
                            return Shinten;

                        case < 50:
                            return Ikishoten;
                    }
                }

                switch (Kenki)
                {
                    //Senei Features
                    case >= 25 when ActionReady(Senei):
                        return Senei;

                    //Guren if no Senei
                    case >= 25 when !LevelChecked(Senei) &&
                                    ActionReady(Guren) &&
                                    InActionRange(Guren):
                        return Guren;
                }

                //Zanshin Usage
                //TODO Buffcheck
                if (ActionReady(Zanshin) && Kenki >= 50 &&
                    InActionRange(Zanshin) &&
                    HasStatusEffect(Buffs.ZanshinReady) &&
                    (JustUsed(Higanbana) ||
                     JustUsed(OriginalHook(OgiNamikiri)) ||
                     GetStatusEffectRemainingTime(Buffs.ZanshinReady) <= 8))
                    return Zanshin;

                if (ActionReady(Shoha) &&
                    MeditationStacks is 3 &&
                    InActionRange(Shoha))
                    return Shoha;

                if (ActionReady(Shinten) &&
                    !HasStatusEffect(Buffs.ZanshinReady) && !ActionReady(Senei) &&
                    (Kenki >= 65 || GetTargetHPPercent() <= 1 && Kenki >= 25))
                    return Shinten;

                // healing
                if (Role.CanSecondWind(25))
                    return Role.SecondWind;

                if (Role.CanBloodBath(40))
                    return Role.Bloodbath;
            }

            //Ranged
            if (ActionReady(Enpi) &&
                !InMeleeRange() &&
                HasBattleTarget())
                return Enpi;
            if (UseTsubame)
                return OriginalHook(TsubameGaeshi);

            //Ogi Namikiri Features
            if (ActionReady(OgiNamikiri) && M6SReady &&
                InActionRange(OriginalHook(OgiNamikiri)) &&
                HasStatusEffect(Buffs.OgiNamikiriReady) &&
                (JustUsed(Higanbana, 5f) ||
                 !TargetIsBoss() ||
                 GetStatusEffectRemainingTime(Buffs.OgiNamikiriReady) <= 8) || NamikiriReady)
                return OriginalHook(OgiNamikiri);

            // Iaijutsu Features
            if (UseIaijutsu() && !IsMoving())
                return OriginalHook(Iaijutsu);

            if (HasStatusEffect(Buffs.MeikyoShisui))
            {
                if (LevelChecked(Gekko) &&
                    (!HasStatusEffect(Buffs.Fugetsu) ||
                     !HasGetsu && HasStatusEffect(Buffs.Fuka)))
                    return Role.CanTrueNorth() && !OnTargetsRear()
                        ? Role.TrueNorth
                        : Gekko;

                if (LevelChecked(Kasha) &&
                    (!HasStatusEffect(Buffs.Fuka) ||
                     !HasKa && HasStatusEffect(Buffs.Fugetsu)))
                    return Role.CanTrueNorth() && !OnTargetsFlank()
                        ? Role.TrueNorth
                        : Kasha;

                if (LevelChecked(Yukikaze) && !HasSetsu)
                    return Yukikaze;
            }

            if (ComboTimer > 0)
            {
                if (ComboAction is Hakaze or Gyofu && LevelChecked(Jinpu))
                {
                    if (LevelChecked(Yukikaze) && !HasSetsu &&
                        HasStatusEffect(Buffs.Fugetsu) && HasStatusEffect(Buffs.Fuka))
                        return Yukikaze;

                    if (!LevelChecked(Kasha) &&
                        (RefreshFugetsu || !HasStatusEffect(Buffs.Fugetsu)) ||
                        LevelChecked(Kasha) &&
                        (!HasStatusEffect(Buffs.Fugetsu) ||
                         HasStatusEffect(Buffs.Fuka) && !HasGetsu ||
                         SenCount is 3 && RefreshFugetsu))
                        return Jinpu;

                    if (LevelChecked(Shifu) &&
                        (!LevelChecked(Kasha) &&
                         (RefreshFuka || !HasStatusEffect(Buffs.Fuka)) ||
                         LevelChecked(Kasha) &&
                         (!HasStatusEffect(Buffs.Fuka) ||
                          HasStatusEffect(Buffs.Fugetsu) && !HasKa ||
                          SenCount is 3 && RefreshFuka)))
                        return Shifu;
                }

                if (ComboAction is Jinpu && LevelChecked(Gekko))
                    return Gekko;

                if (ComboAction is Shifu && LevelChecked(Kasha))
                    return Kasha;
            }
            return actionID;
        }
    }

    internal class SAM_ST_AdvancedMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.SAM_ST_AdvancedMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Hakaze or Gyofu))
                return actionID;

            int kenkiOvercap = SAM_ST_KenkiOvercapAmount;
            int shintenTreshhold = SAM_ST_ExecuteThreshold;

            // Opener for SAM
            if (IsEnabled(CustomComboPreset.SAM_ST_Opener) &&
                Opener().FullOpener(ref actionID))
                return actionID;

            //Meikyo to start before combat
            if (IsEnabled(CustomComboPreset.SAM_ST_CDs) &&
                IsEnabled(CustomComboPreset.SAM_ST_CDs_MeikyoShisui) &&
                ActionReady(MeikyoShisui) &&
                !HasStatusEffect(Buffs.MeikyoShisui) &&
                !InCombat() && HasBattleTarget())
                return MeikyoShisui;

            if (Variant.CanCure(CustomComboPreset.SAM_Variant_Cure, SAM_VariantCure))
                return Variant.Cure;

            if (Variant.CanRampart(CustomComboPreset.SAM_Variant_Rampart))
                return Variant.Rampart;

            if (OccultCrescent.ShouldUsePhantomActions())
                return OccultCrescent.BestPhantomAction();

            //oGCDs
            if (CanWeave() && M6SReady)
            {
                if (IsEnabled(CustomComboPreset.SAM_ST_CDs))
                {
                    //Meikyo Features
                    if (IsEnabled(CustomComboPreset.SAM_ST_CDs_MeikyoShisui) &&
                        UseMeikyo())
                        return MeikyoShisui;

                    //Ikishoten Features
                    if (IsEnabled(CustomComboPreset.SAM_ST_CDs_Ikishoten) &&
                        ActionReady(Ikishoten) && !HasStatusEffect(Buffs.ZanshinReady))
                    {
                        switch (Kenki)
                        {
                            //Dumps Kenki in preparation for Ikishoten
                            case >= 50:
                                return Shinten;

                            case < 50:
                                return Ikishoten;
                        }
                    }
                }

                if (IsEnabled(CustomComboPreset.SAM_ST_Damage))
                {
                    //Senei Features
                    if (IsEnabled(CustomComboPreset.SAM_ST_CDs_Senei)
                        && Kenki >= 25)
                    {
                        if (ActionReady(Senei))
                            return Senei;

                        //Guren if no Senei
                        if (SAM_ST_CDs_Guren &&
                            !LevelChecked(Senei) &&
                            ActionReady(Guren) && InActionRange(Guren))
                            return Guren;
                    }

                    //Zanshin Usage
                    //TODO Buffcheck
                    if (IsEnabled(CustomComboPreset.SAM_ST_CDs_Zanshin) &&
                        ActionReady(Zanshin) && Kenki >= 50 &&
                        InActionRange(Zanshin) &&
                        HasStatusEffect(Buffs.ZanshinReady) &&
                        (JustUsed(Higanbana) ||
                         JustUsed(OriginalHook(OgiNamikiri)) ||
                         SAM_ST_Higanbana_Suboption == 1 && !TargetIsBoss() ||
                         GetStatusEffectRemainingTime(Buffs.ZanshinReady) <= 8))
                        return Zanshin;

                    if (IsEnabled(CustomComboPreset.SAM_ST_CDs_Shoha) &&
                        ActionReady(Shoha) && MeditationStacks is 3 &&
                        InActionRange(Shoha))
                        return Shoha;
                }
                if (IsEnabled(CustomComboPreset.SAM_ST_Shinten) &&
                    ActionReady(Shinten) && !HasStatusEffect(Buffs.ZanshinReady) &&
                    (IsEnabled(CustomComboPreset.SAM_ST_CDs_Senei) && !ActionReady(Senei) ||
                     IsNotEnabled(CustomComboPreset.SAM_ST_CDs_Senei)) &&
                    (Kenki >= kenkiOvercap || GetTargetHPPercent() <= shintenTreshhold && Kenki >= 25))
                    return Shinten;

                // healing
                if (IsEnabled(CustomComboPreset.SAM_ST_ComboHeals))
                {
                    if (Role.CanSecondWind(SAM_STSecondWindThreshold))
                        return Role.SecondWind;

                    if (Role.CanBloodBath(SAM_STBloodbathThreshold))
                        return Role.Bloodbath;
                }
            }

            //Ranged
            if (IsEnabled(CustomComboPreset.SAM_ST_RangedUptime) &&
                ActionReady(Enpi) && !InMeleeRange() && HasBattleTarget())
                return Enpi;

            if (IsEnabled(CustomComboPreset.SAM_ST_Damage))
            {
                if (IsEnabled(CustomComboPreset.SAM_ST_CDs_Iaijutsu) &&
                    SAM_ST_CDs_IaijutsuOption[3] && UseTsubame)
                    return OriginalHook(TsubameGaeshi);

                //Ogi Namikiri Features
                if (IsEnabled(CustomComboPreset.SAM_ST_CDs_OgiNamikiri) &&
                    (!SAM_ST_CDs_OgiNamikiri_Movement || !IsMoving()) &&
                    ActionReady(OgiNamikiri) && InActionRange(OriginalHook(OgiNamikiri)) &&
                    HasStatusEffect(Buffs.OgiNamikiriReady) && M6SReady &&
                    (JustUsed(Higanbana, 5f) ||
                     SAM_ST_Higanbana_Suboption == 1 && !TargetIsBoss() ||
                     GetStatusEffectRemainingTime(Buffs.OgiNamikiriReady) <= 8) || NamikiriReady)
                    return OriginalHook(OgiNamikiri);

                // Iaijutsu Features
                if (IsEnabled(CustomComboPreset.SAM_ST_CDs_Iaijutsu) &&
                    (!IsEnabled(CustomComboPreset.SAM_ST_CDs_Iaijutsu_Movement) || !IsMoving()) &&
                    UseIaijutsu())
                    return OriginalHook(Iaijutsu);
            }

            if (HasStatusEffect(Buffs.MeikyoShisui))
            {
                if (IsEnabled(CustomComboPreset.SAM_ST_Gekko) &&
                    LevelChecked(Gekko) &&
                    (!HasStatusEffect(Buffs.Fugetsu) ||
                     !HasGetsu && HasStatusEffect(Buffs.Fuka)))
                    return IsEnabled(CustomComboPreset.SAM_ST_TrueNorth) &&
                           Role.CanTrueNorth() && !OnTargetsRear()
                        ? Role.TrueNorth
                        : Gekko;

                if (IsEnabled(CustomComboPreset.SAM_ST_Kasha) &&
                    LevelChecked(Kasha) &&
                    (!HasStatusEffect(Buffs.Fuka) ||
                     !HasKa && HasStatusEffect(Buffs.Fugetsu)))
                    return IsEnabled(CustomComboPreset.SAM_ST_TrueNorth) &&
                           Role.CanTrueNorth() && !OnTargetsFlank()
                        ? Role.TrueNorth
                        : Kasha;

                if (IsEnabled(CustomComboPreset.SAM_ST_Yukikaze) &&
                    LevelChecked(Yukikaze) && !HasSetsu)
                    return Yukikaze;
            }

            if (ComboTimer > 0)
            {
                if (ComboAction is Hakaze or Gyofu && LevelChecked(Jinpu))
                {
                    if (IsEnabled(CustomComboPreset.SAM_ST_Yukikaze) &&
                        !HasSetsu && LevelChecked(Yukikaze) &&
                        HasStatusEffect(Buffs.Fugetsu) && HasStatusEffect(Buffs.Fuka))
                        return Yukikaze;

                    if (IsEnabled(CustomComboPreset.SAM_ST_Gekko) &&
                        !LevelChecked(Kasha) &&
                        (RefreshFugetsu || !HasStatusEffect(Buffs.Fugetsu)) ||
                        LevelChecked(Kasha) &&
                        (!HasStatusEffect(Buffs.Fugetsu) ||
                         HasStatusEffect(Buffs.Fuka) && !HasGetsu ||
                         SenCount is 3 && RefreshFugetsu))
                        return Jinpu;

                    if (IsEnabled(CustomComboPreset.SAM_ST_Kasha) &&
                        LevelChecked(Shifu) &&
                        (!LevelChecked(Kasha) &&
                         (RefreshFuka || !HasStatusEffect(Buffs.Fuka)) ||
                         LevelChecked(Kasha) &&
                         (!HasStatusEffect(Buffs.Fuka) ||
                          HasStatusEffect(Buffs.Fugetsu) && !HasKa ||
                          SenCount is 3 && RefreshFuka)))
                        return Shifu;
                }

                if (ComboAction is Jinpu && LevelChecked(Gekko))
                    return Gekko;

                if (IsEnabled(CustomComboPreset.SAM_ST_Kasha) &&
                    ComboAction is Shifu && LevelChecked(Kasha))
                    return Kasha;
            }

            return actionID;
        }
    }

    internal class SAM_AoE_OkaCombo : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.SAM_AoE_OkaCombo;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Oka)
                return actionID;

            if (SAM_Oka_KenkiOvercap &&
                Kenki >= SAM_Oka_KenkiOvercapAmount &&
                LevelChecked(Kyuten) && CanWeave())
                return Kyuten;

            if (HasStatusEffect(Buffs.MeikyoShisui) ||
                ComboTimer > 0 && LevelChecked(Oka) &&
                ComboAction == OriginalHook(Fuko))
                return Oka;

            return OriginalHook(Fuko);
        }
    }

    internal class SAM_AoE_MangetsuCombo : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.SAM_AoE_MangetsuCombo;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Mangetsu)
                return actionID;

            if (SAM_Mangetsu_KenkiOvercap && Kenki >= SAM_Mangetsu_KenkiOvercapAmount &&
                LevelChecked(Kyuten) && CanWeave())
                return Kyuten;

            if (HasStatusEffect(Buffs.MeikyoShisui) ||
                ComboTimer > 0 && LevelChecked(Mangetsu) &&
                ComboAction == OriginalHook(Fuko))
                return Mangetsu;

            return OriginalHook(Fuko);
        }
    }

    internal class SAM_AoE_SimpleMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.SAM_AoE_SimpleMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Fuga or Fuko))
                return actionID;

            if (Variant.CanCure(CustomComboPreset.SAM_Variant_Cure, SAM_VariantCure))
                return Variant.Cure;

            if (Variant.CanRampart(CustomComboPreset.SAM_Variant_Rampart))
                return Variant.Rampart;

            if (OccultCrescent.ShouldUsePhantomActions())
                return OccultCrescent.BestPhantomAction();

            //oGCD Features
            if (CanWeave() && M6SReady)
            {
                if (OriginalHook(Iaijutsu) is MidareSetsugekka && LevelChecked(Hagakure))
                    return Hagakure;

                if (ActionReady(Ikishoten) && !HasStatusEffect(Buffs.ZanshinReady))
                {
                    switch (Kenki)
                    {
                        //Dumps Kenki in preparation for Ikishoten
                        case >= 50:
                            return Kyuten;

                        case < 50:
                            return Ikishoten;
                    }
                }

                if (ActionReady(MeikyoShisui) && !HasStatusEffect(Buffs.MeikyoShisui))
                    return MeikyoShisui;

                if (ActionReady(Zanshin) && HasStatusEffect(Buffs.ZanshinReady) && Kenki >= 50)
                    return Zanshin;

                if (ActionReady(Guren) && Kenki >= 25)
                    return Guren;

                if (ActionReady(Shoha) && MeditationStacks is 3)
                    return Shoha;

                if (ActionReady(Kyuten) && Kenki >= 50 &&
                    !ActionReady(Guren))
                    return Kyuten;

                // healing
                if (Role.CanSecondWind(25))
                    return Role.SecondWind;

                if (Role.CanBloodBath(40))
                    return Role.Bloodbath;
            }

            if (ActionReady(OgiNamikiri) && M6SReady &&
                !IsMoving() && (HasStatusEffect(Buffs.OgiNamikiriReady) || NamikiriReady))
                return OriginalHook(OgiNamikiri);

            if (LevelChecked(TenkaGoken))
            {
                if (LevelChecked(TsubameGaeshi) &&
                    (HasStatusEffect(Buffs.KaeshiGokenReady) ||
                     HasStatusEffect(Buffs.TendoKaeshiGokenReady)))
                    return OriginalHook(TsubameGaeshi);

                if (!IsMoving() &&
                    (OriginalHook(Iaijutsu) is TenkaGoken ||
                     OriginalHook(Iaijutsu) is TendoGoken))
                    return OriginalHook(Iaijutsu);
            }

            if (HasStatusEffect(Buffs.MeikyoShisui))
            {
                if (!HasGetsu && HasStatusEffect(Buffs.Fuka) ||
                    !HasStatusEffect(Buffs.Fugetsu))
                    return Mangetsu;

                if (!HasKa && HasStatusEffect(Buffs.Fugetsu) ||
                    !HasStatusEffect(Buffs.Fuka))
                    return Oka;
            }

            if (ComboTimer > 0 &&
                ComboAction is Fuko or Fuga && LevelChecked(Mangetsu))
            {
                if (!HasGetsu ||
                    RefreshFugetsu ||
                    !HasStatusEffect(Buffs.Fugetsu) ||
                    !LevelChecked(Oka))
                    return Mangetsu;

                if (LevelChecked(Oka) &&
                    (!HasKa ||
                     RefreshFuka ||
                     !HasStatusEffect(Buffs.Fuka)))
                    return Oka;
            }

            return actionID;
        }
    }

    internal class SAM_AoE_AdvancedMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.SAM_AoE_AdvancedMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Fuga or Fuko))
                return actionID;

            float kenkiOvercapAoE = SAM_AoE_KenkiOvercapAmount;

            if (Variant.CanCure(CustomComboPreset.SAM_Variant_Cure, SAM_VariantCure))
                return Variant.Cure;

            if (Variant.CanRampart(CustomComboPreset.SAM_Variant_Rampart))
                return Variant.Rampart;

            if (OccultCrescent.ShouldUsePhantomActions())
                return OccultCrescent.BestPhantomAction();

            //oGCD Features
            if (CanWeave() && M6SReady)
            {
                if (IsEnabled(CustomComboPreset.SAM_AoE_Hagakure) &&
                    OriginalHook(Iaijutsu) is MidareSetsugekka && LevelChecked(Hagakure))
                    return Hagakure;

                if (IsEnabled(CustomComboPreset.SAM_AoE_CDs))
                {
                    if (IsEnabled(CustomComboPreset.SAM_AoE_MeikyoShisui) &&
                        ActionReady(MeikyoShisui) && !HasStatusEffect(Buffs.MeikyoShisui))
                        return MeikyoShisui;

                    if (IsEnabled(CustomComboPreset.SAM_AOE_CDs_Ikishoten) &&
                        ActionReady(Ikishoten) && !HasStatusEffect(Buffs.ZanshinReady))
                    {
                        switch (Kenki)
                        {
                            //Dumps Kenki in preparation for Ikishoten
                            case >= 50:
                                return Kyuten;

                            case < 50:
                                return Ikishoten;
                        }
                    }
                }

                if (IsEnabled(CustomComboPreset.SAM_AoE_Damage))
                {
                    if (IsEnabled(CustomComboPreset.SAM_AoE_Zanshin) &&
                        ActionReady(Zanshin) && HasStatusEffect(Buffs.ZanshinReady) && Kenki >= 50)
                        return Zanshin;

                    if (IsEnabled(CustomComboPreset.SAM_AoE_Guren) &&
                        ActionReady(Guren) && Kenki >= 25)
                        return Guren;

                    if (IsEnabled(CustomComboPreset.SAM_AoE_Shoha) &&
                        ActionReady(Shoha) && MeditationStacks is 3)
                        return Shoha;
                }

                if (IsEnabled(CustomComboPreset.SAM_AoE_Kyuten) &&
                    ActionReady(Kyuten) && Kenki >= kenkiOvercapAoE &&
                    !ActionReady(Guren))
                    return Kyuten;

                if (IsEnabled(CustomComboPreset.SAM_AoE_ComboHeals))
                {
                    if (Role.CanSecondWind(SAM_AoESecondWindThreshold))
                        return Role.SecondWind;

                    if (Role.CanBloodBath(SAM_AoEBloodbathThreshold))
                        return Role.Bloodbath;
                }
            }

            if (IsEnabled(CustomComboPreset.SAM_AoE_Damage))
            {
                if (IsEnabled(CustomComboPreset.SAM_AoE_OgiNamikiri) &&
                    ActionReady(OgiNamikiri) && M6SReady &&
                    (!IsMoving() && HasStatusEffect(Buffs.OgiNamikiriReady) || NamikiriReady))
                    return OriginalHook(OgiNamikiri);

                if (IsEnabled(CustomComboPreset.SAM_AoE_TenkaGoken) &&
                    LevelChecked(TenkaGoken))
                {
                    if (LevelChecked(TsubameGaeshi) &&
                        (HasStatusEffect(Buffs.KaeshiGokenReady) ||
                         HasStatusEffect(Buffs.TendoKaeshiGokenReady)))
                        return OriginalHook(TsubameGaeshi);

                    if (!IsMoving() &&
                        (OriginalHook(Iaijutsu) is TenkaGoken ||
                         OriginalHook(Iaijutsu) is TendoGoken))
                        return OriginalHook(Iaijutsu);
                }
            }

            if (HasStatusEffect(Buffs.MeikyoShisui))
            {
                if (!HasGetsu && HasStatusEffect(Buffs.Fuka) ||
                    !HasStatusEffect(Buffs.Fugetsu))
                    return Mangetsu;

                if (IsEnabled(CustomComboPreset.SAM_AoE_Oka) &&
                    (!HasKa && HasStatusEffect(Buffs.Fugetsu) ||
                     !HasStatusEffect(Buffs.Fuka)))
                    return Oka;
            }

            if (ComboTimer > 0 &&
                ComboAction is Fuko or Fuga && LevelChecked(Mangetsu))
            {
                if (IsNotEnabled(CustomComboPreset.SAM_AoE_Oka) ||
                    !HasGetsu || RefreshFugetsu ||
                    !HasStatusEffect(Buffs.Fugetsu) ||
                    !LevelChecked(Oka))
                    return Mangetsu;

                if (IsEnabled(CustomComboPreset.SAM_AoE_Oka) &&
                    LevelChecked(Oka) &&
                    (!HasKa || RefreshFuka ||
                     !HasStatusEffect(Buffs.Fuka)))
                    return Oka;
            }
            return actionID;
        }
    }

    internal class SAM_MeikyoSens : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.SAM_MeikyoSens;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not MeikyoShisui || !HasStatusEffect(Buffs.MeikyoShisui))
                return actionID;

            if (!HasStatusEffect(Buffs.Fugetsu) ||
                !HasGetsu)
                return Gekko;

            if (!HasStatusEffect(Buffs.Fuka) ||
                !HasKa)
                return Kasha;

            if (!HasSetsu)
                return Yukikaze;

            return actionID;
        }
    }

    internal class SAM_Iaijutsu : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.SAM_Iaijutsu;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Iaijutsu)
                return actionID;

            bool canAddShoha = IsEnabled(CustomComboPreset.SAM_Iaijutsu_Shoha) &&
                               ActionReady(Shoha) &&
                               MeditationStacks is 3;

            if (canAddShoha && CanWeave())
                return Shoha;

            if (IsEnabled(CustomComboPreset.SAM_Iaijutsu_OgiNamikiri) &&
                (ActionReady(OgiNamikiri) && HasStatusEffect(Buffs.OgiNamikiriReady) || NamikiriReady))
                return OriginalHook(OgiNamikiri);

            if (IsEnabled(CustomComboPreset.SAM_Iaijutsu_TsubameGaeshi) &&
                SenCount is not 1 &&
                (LevelChecked(TsubameGaeshi) &&
                 (HasStatusEffect(Buffs.TsubameReady) ||
                  HasStatusEffect(Buffs.KaeshiGokenReady)) ||
                 LevelChecked(TendoKaeshiSetsugekka) &&
                 (HasStatusEffect(Buffs.TendoKaeshiSetsugekkaReady) ||
                  HasStatusEffect(Buffs.TendoKaeshiGokenReady))))
                return OriginalHook(TsubameGaeshi);

            if (canAddShoha)
                return Shoha;

            return actionID;
        }
    }

    internal class SAM_Shinten : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.SAM_Shinten;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is Shinten)
            {
                if (IsEnabled(CustomComboPreset.SAM_Shinten_Senei) &&
                    ActionReady(Senei))
                    return Senei;

                if (IsEnabled(CustomComboPreset.SAM_Shinten_Zanshin) &&
                    HasStatusEffect(Buffs.ZanshinReady))
                    return Zanshin;

                if (IsEnabled(CustomComboPreset.SAM_Shinten_Shoha) &&
                    ActionReady(Shoha) && MeditationStacks is 3)
                    return Shoha;
            }

            return actionID;
        }
    }

    internal class SAM_Kyuten : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.SAM_Kyuten;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is Kyuten)
            {
                if (IsEnabled(CustomComboPreset.SAM_Kyuten_Guren) &&
                    ActionReady(Guren))
                    return Guren;

                if (IsEnabled(CustomComboPreset.SAM_Kyuten_Zanshin) &&
                    HasStatusEffect(Buffs.ZanshinReady))
                    return Zanshin;

                if (IsEnabled(CustomComboPreset.SAM_Kyuten_Shoha) &&
                    ActionReady(Shoha) && MeditationStacks is 3)
                    return Shoha;
            }
            return actionID;
        }
    }

    internal class SAM_Ikishoten : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.SAM_Ikishoten;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Ikishoten)
                return actionID;

            if (IsEnabled(CustomComboPreset.SAM_Ikishoten_Shoha) &&
                ActionReady(Shoha) &&
                HasStatusEffect(Buffs.OgiNamikiriReady) &&
                MeditationStacks is 3)
                return Shoha;

            if (IsEnabled(CustomComboPreset.SAM_Ikishoten_Namikiri) &&
                (ActionReady(OgiNamikiri) && HasStatusEffect(Buffs.OgiNamikiriReady) || NamikiriReady))
                return OriginalHook(OgiNamikiri);

            return actionID;
        }
    }

    internal class SAM_GyotenYaten : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.SAM_GyotenYaten;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is Gyoten && Kenki >= 10)
            {
                if (InMeleeRange())
                    return Yaten;

                if (!InMeleeRange())
                    return Gyoten;
            }

            return actionID;
        }
    }

    internal class SAM_MeikyoShisuiProtection : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.SAM_MeikyoShisuiProtection;

        protected override uint Invoke(uint actionID) =>
            actionID is MeikyoShisui &&
            HasStatusEffect(Buffs.MeikyoShisui) &&
            ActionReady(MeikyoShisui)
                ? All.SavageBlade
                : actionID;
    }

    internal class SAM_SeneiGuren : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.SAM_SeneiGuren;

        protected override uint Invoke(uint actionID) =>
            actionID is Senei && !LevelChecked(Senei)
                ? Guren
                : actionID;
    }
}
