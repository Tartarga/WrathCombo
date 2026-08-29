using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using Lumina.Excel;
using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using WrathCombo.Extensions;
using Status = Dalamud.Game.ClientState.Statuses.IStatus; // conflicts with structs if not defined
namespace WrathCombo.Data;

internal partial class CustomComboCache : IDisposable
{
    private const uint InvalidStatusID = 0;

    //Invalidate this
    private readonly ConcurrentDictionary<(uint StatusID, ulong? TargetID, ulong? SourceID), Status?> statusCache = new();

    /// <summary> Finds a status on the given object. </summary>
    /// <param name="statusID"> Status effect ID. </param>
    /// <param name="obj"> Object to look for effects on. </param>
    /// <param name="sourceID"> Source object ID. </param>
    /// <returns> Status object or null. </returns>
    internal Status? GetStatus(uint statusID, IGameObject? obj, ulong? sourceID)
    {
        if (obj is null)
            return null;

        var key = (statusID, obj.GameObjectId, sourceID);

        if (statusCache.TryGetValue(key, out var found))
            return found;

        if (obj is not IBattleChara chara)
            return statusCache[key] = null;

        var statuses = chara.SafeStatusList;

        if (statuses is null)
            return statusCache[key] = null;

        foreach (var status in statuses)
        {
            if (status.StatusId == InvalidStatusID)
                continue;

            if (status.StatusId == statusID &&
                (!sourceID.HasValue || status.SourceId == 0 || status.SourceId == InvalidObjectID || status.SourceId == sourceID))
            {
                return statusCache[key] = status;
            }
        }

        return statusCache[key] = null;
    }
}

public class StatusCache
{
    public static StatusCache Instance { get; internal set; } = null!;
    public static PausingStatuses PausingStatuses { get; internal set; } = null!;

    internal const uint WeaknessStatusId = 43;
    internal const uint BrinkOfDeathStatusId = 44;
    internal const uint OCDarkDefensesStatusId = 4355;

    public void Init()
    {
        StatusSheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>()!;
        ENStatusSheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>(ClientLanguage.English)!;

        string? damageDownName = ENStatusSheet.TryGetRow(62, out var ddRow) ? ddRow.Name.ToString() : null;
        string? damageUpName = ENStatusSheet.TryGetRow(61, out var duRow) ? duRow.Name.ToString() : null;
        string? evasionUpName = ENStatusSheet.TryGetRow(31, out var euRow) ? euRow.Name.ToString() : null;

        var damageDown = new List<uint>();
        var damageUp = new List<uint>();
        var evasionUp = new List<uint>();
        foreach (var row in ENStatusSheet)
        {
            var name = row.Name.ToString();
            if (damageDownName is not null &&
                name.Equals(damageDownName, StringComparison.CurrentCultureIgnoreCase))
                damageDown.Add(row.RowId);
            if (damageUpName is not null &&
                name.Contains(damageUpName, StringComparison.CurrentCultureIgnoreCase))
                damageUp.Add(row.RowId);
            if (evasionUpName is not null &&
                name.Contains(evasionUpName, StringComparison.CurrentCultureIgnoreCase))
                evasionUp.Add(row.RowId);
        }

        var cleansableDoom = new List<uint>();
        var dispellable = new List<uint>();
        var beneficial = new List<uint>();
        var invincible = new List<uint> { 151, 198, 469, 592, 1240, 1302, 1303, 1567, 1936, 2413, 2654, 3012, 3039, 3052, 3054, 4175 };
        var raiseInvuln = new List<uint>();
        var raise = new List<uint>();
        foreach (var row in StatusSheet)
        {
            if (row.CanDispel)
            {
                dispellable.Add(row.RowId);
                if (row.Icon == 215503)
                    cleansableDoom.Add(row.RowId);
            }

            if (row.StatusCategory == 1)
                beneficial.Add(row.RowId);

            switch (row.Icon)
            {
                case 215024:
                    invincible.Add(row.RowId);
                    break;
                case 215273:
                    raiseInvuln.Add(row.RowId);
                    break;
                case 210406:
                    raise.Add(row.RowId);
                    break;
            }
        }

        DamageDownStatuses = damageDown.ToFrozenSet();
        CleansableDoomStatuses = cleansableDoom.ToFrozenSet();
        DamageUpStatuses = damageUp.ToFrozenSet();
        EvasionUpStatuses = evasionUp.ToFrozenSet();
        DispellableStatuses = dispellable.ToFrozenSet();
        BeneficialStatuses = beneficial.ToFrozenSet();
        InvincibleStatuses = invincible.ToFrozenSet();
        RaiseInvincibilityStatuses = raiseInvuln.ToFrozenSet();
        RaiseStatuses = raise.ToFrozenSet();
        DoNotHealStatuses = new uint[] { 2852 }.ToFrozenSet();

        PausingStatuses = new();
        PausingStatuses.Init();
    }

    internal ExcelSheet<Lumina.Excel.Sheets.Status> StatusSheet = null!;
    internal ExcelSheet<Lumina.Excel.Sheets.Status> ENStatusSheet = null!;
    internal FrozenSet<uint> DamageDownStatuses = null!;
    internal FrozenSet<uint> CleansableDoomStatuses = null!;
    internal FrozenSet<uint> DamageUpStatuses = null!;
    internal FrozenSet<uint> EvasionUpStatuses = null!;
    internal FrozenSet<uint> DispellableStatuses = null!;
    internal FrozenSet<uint> BeneficialStatuses = null!;
    internal FrozenSet<uint> InvincibleStatuses = null!;
    internal FrozenSet<uint> RaiseInvincibilityStatuses = null!;
    internal FrozenSet<uint> RaiseStatuses = null!;
    internal FrozenSet<uint> DoNotHealStatuses = null!;
    public bool HasDamageDown(IGameObject? target) => HasStatusInCacheList(DamageDownStatuses, target);
    public bool HasCleansableDoom(IGameObject? target) => HasStatusInCacheList(CleansableDoomStatuses, target);
    public bool HasDamageUp(IGameObject? target) => HasStatusInCacheList(DamageUpStatuses, target);
    public bool HasEvasionUp(IGameObject? target) => HasStatusInCacheList(EvasionUpStatuses, target);
    public bool HasCleansableDebuff(IGameObject? target) => HasStatusInCacheList(DispellableStatuses, target);
    public bool HasBeneficialStatus(IGameObject? target) => HasStatusInCacheList(BeneficialStatuses, target);
    public bool HasRaiseInvincibility(IBattleChara? target) => HasStatusInCacheList(RaiseInvincibilityStatuses, target);
    public bool HasRaiseStatus(IBattleChara? target) => HasStatusInCacheList(RaiseStatuses, target);

    /// <summary>
    /// Looks up the name of a Status by ID in Lumina Sheets
    /// </summary>
    /// <param name="id">Status ID</param>
    /// <returns></returns>
    public static string GetStatusName(uint id) => Instance.StatusSheet.TryGetRow(id, out var status) ? status.Name.ToString() : "Unknown Status";

    /// <summary>
    /// Returns an uint List of Status IDs based on Name.
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public static List<uint>? GetStatusesByName(string status)
    {
        if (string.IsNullOrEmpty(status))
            return null;
        var statusIds = Instance.StatusSheet
            .Where(x => x.Name.ToString().Equals(status, StringComparison.CurrentCultureIgnoreCase))
            .Select(x => x.RowId)
            .ToList();
        return statusIds.Count != 0 ? statusIds : null;
    }

    /// <summary>
    /// Checks a GameObject's Status list against a set of Status IDs
    /// </summary>
    /// <param name="statusList">Hashset of Status IDs to check</param>
    /// <param name="gameObject">GameObject to check</param>
    /// <returns></returns>
    public static bool HasStatusInCacheList(FrozenSet<uint>? statusList, IGameObject? gameObject)
    {
        if (statusList is null)
            return false;

        if (gameObject is not IBattleChara chara)
            return false;

        var statuses = chara.SafeStatusList;
        if (statuses is null)
            return false;

        return statuses.Any(s => statusList.Contains(s.StatusId));
    }

}

public class PausingStatuses
{
    public void Init()
    {
        AccelerationBombs =
        new HashSet<uint>(
            StatusCache.Instance.StatusSheet
                .Where(row => row.Icon == 215727) // Acceleration Bomb Icon
                .Select(row => row.RowId)
        )
        {
            1132, // Baelsar's Wall - Extreme Caution
            4130 // Authority's Hold

        }.ToFrozenSet();

        Pyretics =
        new HashSet<uint>(
            StatusCache.Instance.StatusSheet
                .Where(row => row.Icon == 215647) // Pyretic Icon
                .Select(row => row.RowId)
        )
        {
            514 // Causality
        }.ToFrozenSet();

        Misc = new uint[] {
            1735 // The Orbonne Monastary - Heavenly Shield
        }.ToFrozenSet();
    }

    internal FrozenSet<uint> AccelerationBombs = null!;
    internal FrozenSet<uint> Pyretics = null!;
    internal FrozenSet<uint> Misc = null!;

}