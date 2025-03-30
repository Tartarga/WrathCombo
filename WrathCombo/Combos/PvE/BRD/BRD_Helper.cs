﻿#region Dependencies

using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Statuses;
using System;
using System.Collections.Generic;
using WrathCombo.CustomComboNS;
using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Data;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;

#endregion

namespace WrathCombo.Combos.PvE;

internal partial class BRD
{
    #region Variables

    // Gauge Stuff
    internal static BRDGauge? gauge = GetJobGauge<BRDGauge>();
    internal static int SongTimerInSeconds => gauge.SongTimer / 1000;
    internal static bool SongNone => gauge.Song == Song.None;
    internal static bool SongWanderer => gauge.Song == Song.Wanderer;
    internal static bool SongMage => gauge.Song == Song.Mage;
    internal static bool SongArmy => gauge.Song == Song.Army;
    //Dot Management
    internal static Status? Purple => FindTargetEffect(Debuffs.CausticBite) ?? FindTargetEffect(Debuffs.VenomousBite);
    internal static Status? Blue => FindTargetEffect(Debuffs.Stormbite) ?? FindTargetEffect(Debuffs.Windbite);
    internal static float PurpleRemaining => Purple?.RemainingTime ?? 0;
    internal static float BlueRemaining => Blue?.RemainingTime ?? 0;

    //Useful Bools
    internal static bool BardHasTarget => HasBattleTarget();
    internal static bool CanBardWeave => CanWeave() && !ActionWatching.HasDoubleWeaved();
    internal static bool CanWeaveDelayed => CanDelayedWeave(0.9) && !ActionWatching.HasDoubleWeaved();
    internal static bool CanIronJaws => LevelChecked(IronJaws);
    internal static bool BuffTime => GetCooldownRemainingTime(RagingStrikes) < 2.7;
    internal static bool BuffWindow => HasEffect(Buffs.RagingStrikes) && 
                                       (HasEffect(Buffs.BattleVoice) || !LevelChecked(BattleVoice)) &&
                                       (HasEffect(Buffs.RadiantFinale) || !LevelChecked(RadiantFinale));

    //Buff Tracking
    internal static float RagingCD => GetCooldownRemainingTime(RagingStrikes);
    internal static float BattleVoiceCD => GetCooldownRemainingTime(BattleVoice);
    internal static float EmpyrealCD => GetCooldownRemainingTime(EmpyrealArrow);
    internal static float RadiantCD => GetCooldownRemainingTime(RadiantFinale);
    internal static float RagingStrikesDuration => GetBuffRemainingTime(Buffs.RagingStrikes);
    internal static float RadiantFinaleDuration => GetBuffRemainingTime(Buffs.RadiantFinale);

    // Charge Tracking
    internal static uint RainOfDeathCharges => LevelChecked(RainOfDeath) ? GetRemainingCharges(RainOfDeath) : 0;
    internal static uint BloodletterCharges => GetRemainingCharges(Bloodletter);

    #endregion

    #region Functions

    // Pooled Apex Logic
    internal static bool UsePooledApex()
    {
        if (gauge.SoulVoice >= 80)
        {
            if (BuffWindow && RagingStrikesDuration < 18 || RagingCD >= 50 && RagingCD <= 58)
                return true;
        }
        return false;
    }

    // Pitch Perfect Logic
    internal static bool PitchPerfected()
    {
       if (LevelChecked(PitchPerfect) && SongWanderer &&
            (gauge.Repertoire == 3 || LevelChecked(EmpyrealArrow) && gauge.Repertoire == 2 && EmpyrealCD < 2))
            return true;
        
       return false;
    }

    //Sidewinder Logic
    internal static bool UsePooledSidewinder()
    {
        if (BuffWindow && RagingStrikesDuration < 18 || RagingCD >= 10)
                return true;
            
       return false;
    }

    //Bloodletter & Rain of Death Logic
    internal static bool UsePooledBloodRain()
    {
        if ((!JustUsed(OriginalHook(Bloodletter)) || !JustUsed(OriginalHook(RainOfDeath))) && 
           (EmpyrealCD > 1 || !LevelChecked(EmpyrealArrow)))
        {
            if (BloodletterCharges == 3 && TraitLevelChecked(Traits.EnhancedBloodletter) || 
                BloodletterCharges == 2 && !TraitLevelChecked(Traits.EnhancedBloodletter) ||
                BloodletterCharges > 0 && (BuffWindow || RagingCD > 30))
                return true; 
        }
        return false;
    }

    #endregion

    #region ID's

    public const byte ClassID = 5;
    public const byte JobID = 23;

    public const uint
        HeavyShot = 97,
        StraightShot = 98,
        VenomousBite = 100,
        RagingStrikes = 101,
        QuickNock = 106,
        Barrage = 107,
        Bloodletter = 110,
        Windbite = 113,
        MagesBallad = 114,
        ArmysPaeon = 116,
        RainOfDeath = 117,
        BattleVoice = 118,
        EmpyrealArrow = 3558,
        WanderersMinuet = 3559,
        IronJaws = 3560,
        TheWardensPaeon = 3561,
        Sidewinder = 3562,
        PitchPerfect = 7404,
        Troubadour = 7405,
        CausticBite = 7406,
        Stormbite = 7407,
        RefulgentArrow = 7409,
        BurstShot = 16495,
        ApexArrow = 16496,
        Shadowbite = 16494,
        Ladonsbite = 25783,
        BlastArrow = 25784,
        RadiantFinale = 25785,
        WideVolley = 36974,
        HeartbreakShot = 36975,
        ResonantArrow = 36976,
        RadiantEncore = 36977;

    public static class Buffs
    {
        public const ushort
            RagingStrikes = 125,
            Barrage = 128,
            MagesBallad = 135,
            ArmysPaeon = 137,
            BattleVoice = 141,
            WanderersMinuet = 865,
            Troubadour = 1934,
            BlastArrowReady = 2692,
            RadiantFinale = 2722,
            ShadowbiteReady = 3002,
            HawksEye = 3861,
            ResonantArrowReady = 3862,
            RadiantEncoreReady = 3863;
    }

    public static class Debuffs
    {
        public const ushort
            VenomousBite = 124,
            Windbite = 129,
            CausticBite = 1200,
            Stormbite = 1201;
    }

    internal static class Traits
    {
        internal const ushort
            EnhancedBloodletter = 445;
    }

    #endregion

    #region Openers

    public static BRDStandard Opener1 = new();
    public static BRDAdjusted Opener2 = new();
    public static BRDComfy Opener3 = new();
    internal static WrathOpener Opener()
    {
        if (IsEnabled(CustomComboPreset.BRD_ST_AdvMode))
        {
            if (Config.BRD_Adv_Opener_Selection == 0 && Opener1.LevelChecked) return Opener1;
            if (Config.BRD_Adv_Opener_Selection == 1 && Opener2.LevelChecked) return Opener2;
            if (Config.BRD_Adv_Opener_Selection == 2 && Opener3.LevelChecked) return Opener3;
        }

        if (Opener1.LevelChecked) return Opener1;
        return WrathOpener.Dummy;
    }

    internal class BRDStandard : WrathOpener
    {
        public override List<uint> OpenerActions { get; set; } =
        [
            Stormbite,
            WanderersMinuet,
            EmpyrealArrow,
            CausticBite,
            BattleVoice,
            BurstShot,
            RadiantFinale,
            RagingStrikes,
            BurstShot,
            RadiantEncore,
            Barrage,
            RefulgentArrow,
            Sidewinder,
            ResonantArrow,
            EmpyrealArrow,
            BurstShot,
            BurstShot,
            IronJaws,
            BurstShot
        ];

        public override List<(int[], uint, Func<bool>)> SubstitutionSteps { get; set; } =
        [
            ([6, 9, 16, 17, 19], RefulgentArrow, () => HasEffect(Buffs.HawksEye))
        ];

        public override List<int> DelayedWeaveSteps { get; set; } =
        [
            5
        ];
        public override int MinOpenerLevel => 100;
        public override int MaxOpenerLevel => 109;

        internal override UserData ContentCheckConfig => Config.BRD_Balance_Content;

        public override bool HasCooldowns() =>
            IsOffCooldown(WanderersMinuet) &&
            IsOffCooldown(BattleVoice) &&
            IsOffCooldown(RadiantFinale) &&
            IsOffCooldown(RagingStrikes) &&
            IsOffCooldown(Barrage) &&
            IsOffCooldown(Sidewinder);
    }

    internal class BRDAdjusted : WrathOpener
    {
        public override List<uint> OpenerActions { get; set; } =
        [
            HeartbreakShot,
            Stormbite,
            WanderersMinuet,
            EmpyrealArrow,
            CausticBite,
            BattleVoice,
            BurstShot,
            RadiantFinale,
            RagingStrikes,
            BurstShot,
            Barrage,
            RefulgentArrow,
            Sidewinder,
            RadiantEncore,
            ResonantArrow,
            EmpyrealArrow,
            BurstShot,
            BurstShot,
            IronJaws,
            BurstShot
        ];

        public override List<(int[], uint, Func<bool>)> SubstitutionSteps { get; set; } =
        [
            ([7, 10, 17, 18, 20], RefulgentArrow, () => HasEffect(Buffs.HawksEye))
        ];

        public override List<int> DelayedWeaveSteps { get; set; } =
        [
            6
        ];

        public override int MinOpenerLevel => 100;
        public override int MaxOpenerLevel => 109;

        internal override UserData ContentCheckConfig => Config.BRD_Balance_Content;

        public override bool HasCooldowns() =>
            IsOffCooldown(WanderersMinuet) &&
            IsOffCooldown(BattleVoice) &&
            IsOffCooldown(RadiantFinale) &&
            IsOffCooldown(RagingStrikes) &&
            IsOffCooldown(Barrage) &&
            IsOffCooldown(Sidewinder);
    }

    internal class BRDComfy : WrathOpener
    {
        public override List<uint> OpenerActions { get; set; } =
        [
            Stormbite,
            HeartbreakShot,
            WanderersMinuet,
            CausticBite,
            EmpyrealArrow,
            RadiantFinale,
            BurstShot,
            BattleVoice,
            RagingStrikes,
            BurstShot,
            Barrage,
            RefulgentArrow,
            Sidewinder,
            RadiantEncore,
            ResonantArrow,
            BurstShot,
            EmpyrealArrow,
            BurstShot,
            IronJaws,
            BurstShot
        ];

        public override List<(int[], uint, Func<bool>)> SubstitutionSteps { get; set; } =
        [
            ([7, 10, 16, 18, 20], RefulgentArrow, () => HasEffect(Buffs.HawksEye))
        ];

        public override int MinOpenerLevel => 100;
        public override int MaxOpenerLevel => 109;

        internal override UserData ContentCheckConfig => Config.BRD_Balance_Content;

        public override bool HasCooldowns() =>
            IsOffCooldown(WanderersMinuet) &&
            IsOffCooldown(BattleVoice) &&
            IsOffCooldown(RadiantFinale) &&
            IsOffCooldown(RagingStrikes) &&
            IsOffCooldown(Barrage) &&
            IsOffCooldown(Sidewinder);
    }

    #endregion
}
