using WrathCombo.Core;
using WrathCombo.CustomComboNS;
using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Extensions;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;

namespace WrathCombo.Combos.PvE;

internal partial class BLU
{
    private static bool _surpanakhaReady;

    private static bool UsePrimalCDs(ref uint actionID, uint retargetFrom)
    {
        if (HasStatusEffect(Buffs.PhantomFlurry))
        {
            actionID = OriginalHook(PhantomFlurry);
            return true;
        }

        if (GetStatusEffect(Buffs.WingedReprobation)?.Param > 1 &&
            IsOffCooldown(WingedReprobation) &&
            IsSpellActive(WingedReprobation))
        {
            actionID = OriginalHook(WingedReprobation);
            return true;
        }

        if (IsOffCooldown(FeatherRain) && IsSpellActive(FeatherRain))
        {
            actionID = FeatherRain.Retarget(retargetFrom,
                SimpleTarget.HardTarget.IfHostile() ??
                SimpleTarget.LastHostileHardTarget);
            return true;
        }

        if (IsOffCooldown(Eruption) && IsSpellActive(Eruption))
        {
            actionID = Eruption;
            return true;
        }

        if (IsOffCooldown(ShockStrike) && IsSpellActive(ShockStrike))
        {
            actionID = ShockStrike;
            return true;
        }

        if (IsOffCooldown(RoseOfDestruction) && IsSpellActive(RoseOfDestruction))
        {
            actionID = RoseOfDestruction;
            return true;
        }

        if (IsOffCooldown(GlassDance) && IsSpellActive(GlassDance))
        {
            actionID = GlassDance;
            return true;
        }

        if (IsOffCooldown(JKick) && IsSpellActive(JKick))
        {
            actionID = JKick;
            return true;
        }

        if (IsOffCooldown(Nightbloom) && IsSpellActive(Nightbloom))
        {
            actionID = Nightbloom;
            return true;
        }

        if (IsOffCooldown(MatraMagic) &&
            HasStatusEffect(Buffs.DPSMimicry) &&
            IsSpellActive(MatraMagic))
        {
            actionID = MatraMagic;
            return true;
        }

        if (IsSpellActive(Surpanakha))
        {
            if (GetRemainingCharges(Surpanakha) == 4)
                _surpanakhaReady = true;
            if (_surpanakhaReady && GetRemainingCharges(Surpanakha) > 0)
            {
                actionID = Surpanakha;
                return true;
            }
            if (GetRemainingCharges(Surpanakha) == 0)
                _surpanakhaReady = false;
        }

        if (IsOffCooldown(WingedReprobation) && IsSpellActive(WingedReprobation))
        {
            actionID = OriginalHook(WingedReprobation);
            return true;
        }

        if (IsOffCooldown(SeaShanty) && IsSpellActive(SeaShanty))
        {
            actionID = SeaShanty;
            return true;
        }

        if (IsOffCooldown(PhantomFlurry) && IsSpellActive(PhantomFlurry))
        {
            actionID = PhantomFlurry;
            return true;
        }

        return false;
    }

    private static bool UseSharpenedKnife() =>
        IsSpellActive(SharpenedKnife) &&
        InActionRange(SharpenedKnife);

    private static uint DoSimpleDPS(uint actionID, uint retargetFrom, bool onAoE)
    {
        if (HasStatusEffect(Buffs.WaningNocturne))
            return actionID;

        if (UsePrimalCDs(ref actionID, retargetFrom))
            return actionID;

        if (!onAoE &&
            IsOffCooldown(TripleTrident) &&
            IsSpellActive(TripleTrident) &&
            InActionRange(TripleTrident))
            return TripleTrident;

        if (onAoE)
        {
            if (IsSpellActive(Electrogenesis))
                return Electrogenesis;
            if (IsSpellActive(HydroPull))
                return HydroPull;
        }
        else if (UseSharpenedKnife())
            return SharpenedKnife;

        return IsSpellActive(SonicBoom) ? SonicBoom : actionID;
    }
}
