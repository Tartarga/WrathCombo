using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Resources.Localization.JobConfigs;
using static WrathCombo.Window.Functions.UserConfig;

namespace WrathCombo.Combos.PvE.Content.DeepDungeon;

internal static partial class DeepDungeon
{
    internal static class Config
    {
        public static UserInt
            PoTD_SustainingPotion_HP = new("PoTD_SustainingPotion_HP", 50);

        internal static void Draw(Preset preset)
        {
            switch(preset)
            {
                case Preset.PoTD_SustainingPotion:
                    DrawSliderInt(1, 100, PoTD_SustainingPotion_HP,
                        Generics.StopFriendlyHpPercent100, 200);
                    break;
            }
        }
    }

}

