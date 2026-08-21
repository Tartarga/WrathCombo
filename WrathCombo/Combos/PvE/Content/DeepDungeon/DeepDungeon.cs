using WrathCombo.Combos.PvE.ALL;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;

namespace WrathCombo.Combos.PvE.Content.DeepDungeon;

internal static partial class DeepDungeon
{
    public static bool TryGetPoTDAction(ref uint actionID)
    {
        if (UseSustainingPotion(out var potionId))
        {
            actionID = Items.UseItem(potionId);
            return true;
        }
        return false;
    }

    public static bool UseSustainingPotion(out uint potionId)
    {
        potionId = 0;

        if (IsEnabled(Preset.PoTD_SustainingPotion) && Items.ItemReady(PoTDSustainingPotion) && !HasStatusEffect(Buffs.Rehabilitation) && PlayerHealthPercentageHp() <= Config.PoTD_SustainingPotion_HP)
        {
            potionId = PoTDSustainingPotion;
            return true;
        }

        if (IsEnabled(Preset.HoH_EmpyreanPotion) && Items.ItemReady(HoHEmpyreanPotion) && !HasStatusEffect(Buffs.Rehabilitation) && PlayerHealthPercentageHp() <= Config.HoH_SustainingPotion_HP)
        {
            potionId = HoHEmpyreanPotion;
            return true;
        }

        return false;
    }

}

