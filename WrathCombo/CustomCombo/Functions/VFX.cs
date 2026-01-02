using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using System;
using System.Collections.Frozen;
using System.Linq;
using WrathCombo.Extensions;

namespace WrathCombo.CustomComboNS.Functions;

internal abstract partial class CustomComboFunctions
{
    private const StringComparison Lower = StringComparison.OrdinalIgnoreCase;

    /// <summary>
    /// Text Comparison for Tank Buster VFX Paths
    /// </summary>
    /// <param name="vfx">The VFX to check the Path of</param>
    /// <returns>Bool if vfx path matches</returns>
    public static bool IsTankBusterEffectPath(VfxInfo vfx)
    {
        return TankbusterPaths.Any(x => vfx.Path.StartsWith(x, Lower));
    }

    private static readonly FrozenSet<string> TankbusterPaths = FrozenSet.ToFrozenSet<string>([
        "vfx/lockon/eff/tank",                          // Generic TB check
        "vfx/lockon/eff/x6fe_fan100_50_0t1",            // Necron Blue Shockwave - Cone Tankbuster
        "vfx/common/eff/mon_eisyo03t",                  // M10 Deep Impact AoE TB (also generic?)
        "vfx/lockon/eff/m0676trg_tw_d0t1p",             // M10 Hot Impact shared TB
        "vfx/lockon/eff/m0676trg_tw_s6_d0t1p",          // M11 Raw Steel
        "vfx/lockon/eff/z6r2b3_8sec_lockon_c0a1",       // Kam'lanaut Princely Blow
        "vfx/lockon/eff/m0742trg_b1t1",                 // M7 Abominable Blink
        "vfx/lockon/eff/x6r9_tank_lockonae"             // M9 Hardcore Large TB
    ], StringComparer.OrdinalIgnoreCase);

    // List of Multi-Hit Shared Damage Effect Paths
    private static readonly FrozenSet<string> MHSharedDmgPaths = FrozenSet.ToFrozenSet([
        "vfx/lockon/eff/com_share5a1",
        "vfx/lockon/eff/com_share4a1",
        "vfx/lockon/eff/m0922trg_t2w"
    ], StringComparer.OrdinalIgnoreCase);

    // List of Regular Shared Damage Effect Paths
    private static readonly FrozenSet<string> SharedDmgPaths = FrozenSet.ToFrozenSet([
        "vfx/lockon/eff/coshare",
        "vfx/lockon/eff/share_laser",
        "vfx/lockon/eff/com_share"
    ], StringComparer.OrdinalIgnoreCase);

    //private static readonly FrozenSet<ushort> NoObjectStackDuties = FrozenSet.ToFrozenSet<ushort>([
    //    1194 // The Skydeep Cenote
    //]);

    /// <summary>
    /// Checks for the presence of a shared damage effect on party members or some ground locations, and determines if effect is multi-hit.
    /// </summary>
    /// <remarks>Only effects targeting party members or used by hidden boss NPC helpers are considered.</remarks>
    /// <param name="partyMember">When this method returns, will return a party member IF they were targetted.</param>
    /// <param name="isMultiHit">When this method returns, contains a value indicating whether the detected shared damage effect is a multi-hit
    /// effect.</param>
    /// <param name="distance">When this method returns, contains the distance from the detected to the affected party member or ground location,
    /// or 'float.MaxValue' if no effect is found.</param>
    /// <returns>true if a shared damage effect is detected on a party member or invisible helper NPC; otherwise, false.</returns>
    public static bool CheckForSharedDamageEffect(out IBattleChara? partyMember, out bool isMultiHit, out float distance)
    {
        partyMember = null;
        distance = float.MaxValue;
        isMultiHit = false;

        var vfxEffects = VfxManager.TrackedEffects.FilterToTargeted();

        if (vfxEffects.Count == 0)
            return false;

        // Prioritize Multi-Hit Shared Damage Effects as their paths are more precise pathing over Shared Damage Effects paths
        VfxInfo sharedVfx = vfxEffects.FirstOrDefault(AoE => MHSharedDmgPaths.Any(PathInList => AoE.Path.StartsWith(PathInList, Lower)));
        // Found Multi-Hit Shared Damage Effect?
        if (sharedVfx.VfxID != 0) isMultiHit = true;
        // Else look for regular Shared Damage Effect
        else sharedVfx = vfxEffects.FirstOrDefault(AoE => SharedDmgPaths.Any(PathInList => AoE.Path.StartsWith(PathInList, Lower)));

        if (sharedVfx.VfxID == 0)
            return false;

        if (sharedVfx.TargetID.GetObject() is IBattleChara targetobj)
        {
            // Set Party Member if in party
            if (targetobj.IsInParty()) partyMember = targetobj;
            // Get Distance of target if it's only in the party or is an NPC (ground-targeted shared damage)
            if (partyMember != null || targetobj is IBattleNpc)
                distance = GetTargetDistance(targetobj);
            return true;
        }
        //else //Not in the object list, all players stack on a tower, not checking at all times.
        //if (NoObjectStackDuties.Contains(Svc.ClientState.TerritoryType) &&
        //    InBossEncounter())
        //{
        //    AoEEffects = VfxManager.TrackedEffects.FilterToNoTarget();
        //    sharedVfx = AoEEffects.FirstOrDefault(IsMultiHitSharedDamageEffectPath);
        //    if (sharedVfx.VfxID != 0) isMultiHit = true;
        //    else sharedVfx = AoEEffects.FirstOrDefault(IsShareDamageEffectPath);

        //    //Quick and dirty
        //    distance = Vector3.Distance(LocalPlayer.Position, sharedVfx.Placement.Position);
        //    return true;
        //}

        return false;
    }

    /// <summary>
    /// Attempts to retrieve the current target of a detected tank buster visual effect.
    /// </summary>
    /// <remarks>This method searches for an active tank buster visual effect and attempts to resolve its
    /// target to a battle character. If no such effect is present or the target cannot be resolved, target is set to
    /// null and the method returns false. Probably won't work in dual tank situation.</remarks>
    /// <param name="target">When this method returns, contains the battle character targeted by the tank buster effect, if found; otherwise,
    /// null. This parameter is passed uninitialized.</param>
    /// <returns>true if a tank buster target is found and assigned to target; otherwise, false.</returns>
    public static bool TryGetTankBusterTarget(out IBattleChara target)
    {
        target = null!;

        var tankBusterVfx = VfxManager.TrackedEffects
            .FilterToTargeted()
            .FilterToTargetRole(CombatRole.Tank)
            .Where(x => x.TargetID.GetObject().IsInParty())
            .FirstOrDefault(IsTankBusterEffectPath);

        if (tankBusterVfx.VfxID == 0)
            return false;

        if (tankBusterVfx.TargetID.GetObject() is not IBattleChara battleChara)
            return false;

        target = battleChara;
        return true;
    }

    /// <summary>
    /// Checks if the specified character has an active tank buster marker on them.
    /// </summary>
    /// <param name="targetObject">The character to check. Defaults to the local player.</param>
    /// <returns>true if the target has an active tank buster effect, false otherwise.</returns>
    public static bool HasIncomingTankBusterEffect(
        IGameObject? targetObject = null)
    {
        // Default to local player if none provided
        targetObject ??= Player.Object;

        if (targetObject == null)
            return false;

        ulong targetId = targetObject.GameObjectId;

        return VfxManager.TrackedEffects
            .FilterToTarget(targetId)
            .Any(IsTankBusterEffectPath);
    }
}
