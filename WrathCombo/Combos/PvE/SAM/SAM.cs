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

            return DoBasicCombo(false);
        }
    }

    internal class SAM_AoE_SimpleMode : CustomCombo
    {
        protected internal override Preset Preset => Preset.SAM_AoE_SimpleMode;

        protected override uint Invoke(uint actionID)
        {
            if (!CustomActionHelper.OneButtonRotationChecker(actionID, CustomActionType.AoEDPS, Fuga, Fuko))
                return actionID;

            return DoBasicCombo(onAoE: true);
        }
    }
}
