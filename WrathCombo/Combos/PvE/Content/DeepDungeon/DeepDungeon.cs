using WrathCombo.Combos.PvE.ALL;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;

namespace WrathCombo.Combos.PvE.Content.DeepDungeon;

internal static partial class DeepDungeon
{
    public static bool TryGetPoTDAction(ref uint actionID)
    {
        if (UseSustainingPotion())
        {
            actionID = Items.UseItem(PoTDSustainingPotion);
            return true;
        }
        return false;
    }

    public static bool UseSustainingPotion()
    {
        if (IsEnabled(Preset.PoTD_SustainingPotion) && Items.ItemReady(PoTDSustainingPotion) && !HasStatusEffect(Buffs.Rehabilitation) && PlayerHealthPercentageHp() <= Config.PoTD_SustainingPotion_HP)
            return true;

        return false;
    }

}

