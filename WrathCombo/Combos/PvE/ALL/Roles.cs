using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;

namespace WrathCombo.Combos.PvE
{
    class MagicRole //All Magic Classes
    {
        public const uint
            LucidDreaming =
                7562, //Lv14 WHM/BLM/SMN/SCH/AST/RDM/BLU/SGE, instant, 60.0s CD (group 45), range 0, single-target, targets=self
            Swiftcast =
                7561, //Lv18 WHM/BLM/SMN/SCH/AST/RDM/BLU/SGE, instant, 60.0s CD (group 44), range 0, single-target, targets=self
            Surecast =
                7559; //Lv44 WHM/BLM/SMN/SCH/AST/RDM/BLU/SGE, instant, 120.0s CD (group 48), range 0, single-target, targets=self

        public class Buffs
        {
            public const ushort
                Raise = 148,
                Swiftcast = 167,
                Surecast = 160;
        }

        public static bool CanLucidDream(int MPThreshold, bool spellweave = true) =>
            ActionReady(LucidDreaming)
            && LocalPlayer.CurrentMp <= MPThreshold
            && (!spellweave || CanSpellWeave());

        public static bool CanSwiftcast(bool spellweave = true) =>
            ActionReady(Swiftcast) && (!spellweave || CanSpellWeave());

    }
    class CasterRole : MagicRole //Offensive Magic
    {
        public const uint
            Skyshard = 203, //LB1, 2.0s cast, range 25, AOE 8 circle, targets=area, castAnimLock=3.100
            Starstorm = 204, //LB2, 3.0s cast, range 25, AOE 10 circle, targets=area, castAnimLock=5.100
            Addle = 7560, //Lv8 BLM/SMN/RDM/BLU, instant, 90.0s CD (group 46), range 25, single-target, targets=hostile
            Sleep = 25880; //Lv10 BLM/SMN/RDM/BLU, 2.5s cast, GCD, range 30, AOE 5 circle, targets=hostile

        public class Debuffs
        {
            public const ushort
                Addle = 1203;
        }

        public static bool CanAddle() =>
            ActionReady(Addle) && !TargetHasEffectAny(Debuffs.Addle);
    }

    class HealerRole : MagicRole //Healers
    {
        public const uint
            HealingWind = 206, //LB1, 2.0s cast, range 0, AOE 50 circle, targets=self, castAnimLock=2.100
            BreathOfTheEarth = 207, //LB2, 2.0s cast, range 0, AOE 50 circle, targets=self, castAnimLock=5.130
            Repose = 16560, //Lv8, 2.5s cast, GCD, range 30, single-target, targets=hostile
            Esuna = 7568, //Lv10, 1.0s cast, GCD, range 30, single-target, targets=self/party/alliance/friendly
            Rescue = 7571; //Lv48, instant, 120.0s CD (group 49), range 30, single-target, targets=party
    }

    class PhysicalRole //Base Shared Physical Dmg Classes
    {
        public const uint
            SecondWind =
                7541, //Lv8 MNK/DRG/BRD/NIN/MCH/SAM/DNC/RPR, instant, 120.0s CD (group 49), range 0, single-target, targets=self
            ArmsLength =
                7548; //Lv32 PLD/MNK/WAR/DRG/BRD/NIN/MCH/DRK/SAM/GNB/DNC/RPR, instant, 120.0s CD (group 48), range 0, single-target, targets=self

        public class Buffs
        {
            public const ushort
                ArmsLength = 1209;
        }

        public static bool CanSecondWind(int healthpercent) =>
            ActionReady(SecondWind) && PlayerHealthPercentageHp() <= healthpercent;
    }

    class RangedRole : PhysicalRole
    {
        public const uint
            BigShot = 4238, //LB1, 2.0s cast, range 30, AOE 30+R width 4 rect, targets=hostile, castAnimLock=3.100
            Desperado = 4239, //LB2, 3.0s cast, range 30, AOE 30+R width 5 rect, targets=hostile, castAnimLock=3.100
            LegGraze = 7554, //Lv6, instant, 30.0s CD (group 42), range 25, single-target, targets=hostile
            FootGraze = 7553, //Lv10, instant, 30.0s CD (group 41), range 25, single-target, targets=hostile
            Peloton = 7557, //Lv20, instant, 5.0s CD (group 40), range 0, AOE 30 circle, targets=self
            HeadGraze = 7551; //Lv24, instant, 30.0s CD (group 43), range 25, single-target, targets=hostile
        public new class Buffs : PhysicalRole.Buffs
        {
            public const ushort
                Peloton = 1199;
        }

        public static bool CanHeadGraze(CustomComboPreset preset, WeaveTypes weave = WeaveTypes.None) =>
            IsEnabled(preset) && CanInterruptEnemy() && IsOffCooldown(HeadGraze) && CheckWeave(weave);
    }

    class MeleeRole : PhysicalRole
    {
        public const uint
            Braver = 200, //LB1, 2.0s cast, range 8, single-target, targets=hostile, castAnimLock=3.860
            Bladedance = 201, //LB2, 3.0s cast, range 8, single-target, targets=hostile, castAnimLock=3.860
            LegSweep = 7863, //Lv10, instant, 40.0s CD (group 41), range 3, single-target, targets=hostile
            Bloodbath = 7542, //Lv12, instant, 90.0s CD (group 46), range 0, single-target, targets=self
            Feint = 7549, //Lv22, instant, 90.0s CD (group 47), range 10, single-target, targets=hostile
            TrueNorth = 7546; //Lv50, instant, 45.0s CD (group 45/50) (2 charges), range 0, single-target, targets=self


        public new class Buffs : PhysicalRole.Buffs
        {
            public const ushort
                BloodBath = 84,
                TrueNorth = 1250;
        }

        public static class Debuffs
        {
            public const ushort
                Feint = 1195;
        }

        public static bool CanBloodBath(int healthpercent) =>
            ActionReady(Bloodbath) && PlayerHealthPercentageHp() <= healthpercent;
        public static bool CanTrueNorth() =>
            ActionReady(TrueNorth) && TargetNeedsPositionals() && !HasEffect(Buffs.TrueNorth);
    }

    class TankRole : PhysicalRole
    {
        public const uint
            ShieldWall = 197, //LB1, instant, range 0, AOE 50 circle, targets=self, animLock=1.930
            Stronghold = 198, //LB2, instant, range 0, AOE 50 circle, targets=self, animLock=3.860
            Rampart = 7531, //Lv8, instant, 90.0s CD (group 46), range 0, single-target, targets=self
            LowBlow = 7540, //Lv12, instant, 25.0s CD (group 41), range 3, single-target, targets=hostile
            Provoke = 7533, //Lv15, instant, 30.0s CD (group 42), range 25, single-target, targets=hostile
            Interject = 7538, //Lv18, instant, 30.0s CD (group 43), range 3, single-target, targets=hostile
            Reprisal = 7535, //Lv22, instant, 60.0s CD (group 44), range 0, AOE 5 circle, targets=self
            Shirk = 7537; //Lv48, instant, 120.0s CD (group 49), range 25, single-target, targets=party
        
        public static class Debuffs
        {
            public const ushort
                Reprisal = 1193; //applied by Reprisal to target
        }
        public static bool CanInterject() =>
            ActionReady(Interject) && CanInterruptEnemy();

        public static bool CanRampart(int healthpercent) =>
            ActionReady(Rampart) && PlayerHealthPercentageHp() < healthpercent;

        public static bool CanReprisal(int healthpercent) => //? May need more fleshing out by tank masters
            ActionReady(Reprisal) && InActionRange(Reprisal) && PlayerHealthPercentageHp() < healthpercent;



    }
}
