using WrathCombo.CustomComboNS;
using WrathCombo.Native;
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

            if (CanWeave())
            {
                if (UseKenki(ref actionID, false))
                    return actionID;
            }

            if (UseTsubame(false))
                return OriginalHook(TsubameGaeshi);

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

            if (CanWeave())
            {
                if (UseKenki(ref actionID, true))
                    return actionID;
            }

            if (UseTsubame(true))
                return OriginalHook(TsubameGaeshi);

            if (UseIaiJutsu(true))
                return OriginalHook(Iaijutsu);

            return HasStatusEffect(Buffs.MeikyoShisui)
                ? DoMeikyoCombo(actionID, true)
                : DoBasicCombo(true);
        }
    }
}
