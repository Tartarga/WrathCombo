﻿using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Linq;
using WrathCombo.Data;
using WrathCombo.Services;

namespace WrathCombo.CustomComboNS.Functions
{
    internal abstract partial class CustomComboFunctions
    {
        /// <summary> Calls the original hook. </summary>
        /// <param name="actionID"> Action ID. </param>
        /// <returns> The result from the hook. </returns>
        public static uint OriginalHook(uint actionID) => Service.ActionReplacer.OriginalHook(actionID);

        /// <summary> Compare the original hook to the given action ID. </summary>
        /// <param name="actionID"> Action ID. </param>
        /// <returns> A value indicating whether the action would be modified. </returns>
        public static bool IsOriginal(uint actionID) => Service.ActionReplacer.OriginalHook(actionID) == actionID;

        /// <summary> Checks if the player is high enough level to use the passed Action ID. </summary>
        /// <param name="actionid"> ID of the action. </param>
        /// <returns></returns>
        public static bool LevelChecked(uint actionid) => LocalPlayer.Level >= GetLevel(actionid) && IsActionUnlocked(actionid);

        /// <summary> Checks if the player is high enough level to use the passed Trait ID. </summary>
        /// <param name="traitid"> ID of the action. </param>
        /// <returns></returns>
        public static bool TraitLevelChecked(uint traitid) => LocalPlayer.Level >= GetTraitLevel(traitid);

        /// <summary> Returns the name of an action from its ID. </summary>
        /// <param name="id"> ID of the action. </param>
        /// <returns></returns>
        public static string GetActionName(uint id) => ActionWatching.GetActionName(id);

        /// <summary> Returns the level required for an action from its ID. </summary>
        /// <param name="id"> ID of the action. </param>
        /// <returns></returns>
        public static int GetLevel(uint id) => ActionWatching.GetLevel(id);

        /// <summary> Get the Cast time of an action. </summary>
        /// <param name="id"> Action ID to check. </param>
        /// <returns> Returns the cast time of an action. </returns>
        internal static float GetActionCastTime(uint id) => ActionWatching.GetActionCastTime(id);

        /// <summary> Checks if the player is in range to use an action. Best used with actions with irregular ranges.</summary>
        /// <param name="id"> ID of the action. </param>
        /// <param name="optionalTarget"> Optional target to check range against. </param>
        /// <returns></returns>
        public static bool InActionRange(uint id, IGameObject? optionalTarget = null)
        {
            int range = ActionWatching.GetActionRange(id);
            switch (range)
            {
                case 0: //Self Use Skills (Second Wind) or attacks (Art of War, Dyskrasia)
                    {
                        //NOTES: HOUSING DUMMIES ARE FUCKING CURSED BASTARDS THAT DON'T REGISTER ATTACKS CORRECTLY WITH SELF RADIUS ATTACKS
                        //Use Explorer Mode dungeon, field map dummies, or let Thancred tank.

                        //Check if there is a radius
                        float radius = ActionWatching.GetActionEffectRange(id);
                        //Player has a 0.5y radius inside hitbox.
                        //GetTargetDistance measures hitbox to hitbox (correct usage for ranged abilities so far)
                        //But attacks from player must include personal space (0.5y).
                        if (radius > 0)
                        {   //Do not nest with above
                            if (HasTarget() || optionalTarget != null) return GetTargetDistance(optionalTarget) <= radius; else return false;
                        }
                        else return true; //Self use targets (Second Wind) have no radius
                    }
                default:
                    return GetTargetDistance(optionalTarget) <= range;
            }
        }

        /// <summary> Returns the level of a trait. </summary>
        /// <param name="id"> ID of the action. </param>
        /// <returns></returns>
        public static int GetTraitLevel(uint id) => ActionWatching.GetTraitLevel(id);

        /// <summary> Checks if the player can use an action based on the level required and whether it has charges / is off cooldown. </summary>
        /// <param name="id"> ID of the action. </param>
        /// <returns> Non-charge actions have a charge value of 1 when off cooldown; otherwise they have a value of 0. </returns>
        public static unsafe bool ActionReady(uint id)
        {
            uint hookedId = OriginalHook(id);

            return ((GetCooldownRemainingTime(hookedId) <= RemainingGCD + 0.5f && ActionWatching.GetAttackType(hookedId) != ActionWatching.ActionAttackType.Ability) ||
                HasCharges(hookedId)) && ActionManager.Instance()->GetActionStatus(ActionType.Action, hookedId, checkRecastActive: false, checkCastingActive: false) is 0 or 582 or 580;
        }

        /// <summary> Checks if all passed actions are ready to be used. </summary>
        /// <param name="ids"> IDs of the actions. </param>
        /// <returns></returns>
        public static bool ActionsReady(uint[] ids)
        {
            foreach (var id in ids)
                if (!ActionReady(id)) return false;

            return true;
        }

        /// <summary> Checks if the last action performed was the passed ID. </summary>
        /// <param name="id"> ID of the action. </param>
        /// <returns></returns>
        public static bool WasLastAction(uint id) => ActionWatching.CombatActions.Count > 0 && ActionWatching.CombatActions.LastOrDefault() == id;

        /// <summary> Returns how many times in a row the last action was used. </summary>
        /// <returns></returns>
        public static int LastActionCounter() => ActionWatching.LastActionUseCount;

        /// <summary> Checks if the last weaponskill used was the passed ID. Does not have to be the last action performed, just the last weaponskill used. </summary>
        /// <param name="id"> ID of the action. </param>
        /// <returns></returns>
        public static bool WasLastWeaponskill(uint id) => ActionWatching.LastWeaponskill == id;

        /// <summary> Checks if the last spell used was the passed ID. Does not have to be the last action performed, just the last spell used. </summary>
        /// <param name="id"> ID of the action. </param>
        /// <returns></returns>
        public static bool WasLastSpell(uint id) => ActionWatching.LastSpell == id;

        /// <summary> Checks if the last ability used was the passed ID. Does not have to be the last action performed, just the last ability used. </summary>
        /// <param name="id"> ID of the action. </param>
        /// <returns></returns>
        public static bool WasLastAbility(uint id) => ActionWatching.LastAbility == id;

        /// <summary> Returns if the player has set the spell as active in the Blue Mage Spellbook </summary>
        /// <param name="id"> ID of the BLU spell. </param>
        /// <returns></returns>
        public static bool IsSpellActive(uint id) => Service.Configuration.ActiveBLUSpells.Contains(id);

        /// <summary> Calculate the best action to use, based on cooldown remaining. If there is a tie, the original is used. </summary>
        /// <param name="original"> The original action. </param>
        /// <param name="actions"> Action data. </param>
        /// <returns> The appropriate action to use. </returns>
        public static uint CalcBestAction(uint original, params uint[] actions)
        {
            static (uint ActionID, CooldownData Data) Compare(
                uint original,
                (uint ActionID, CooldownData Data) a1,
                (uint ActionID, CooldownData Data) a2)
            {
                // Neither, return the first parameter
                if (!a1.Data.IsCooldown && !a2.Data.IsCooldown)
                    return original == a1.ActionID ? a1 : a2;

                // Both, return soonest available
                if (a1.Data.IsCooldown && a2.Data.IsCooldown)
                {
                    if (a1.Data.HasCharges && a2.Data.HasCharges)
                    {
                        if (a1.Data.RemainingCharges == a2.Data.RemainingCharges)
                        {
                            return a1.Data.ChargeCooldownRemaining < a2.Data.ChargeCooldownRemaining
                                ? a1 : a2;
                        }

                        return a1.Data.RemainingCharges > a2.Data.RemainingCharges
                            ? a1 : a2;
                    }

                    else if (a1.Data.HasCharges)
                    {
                        if (a1.Data.RemainingCharges > 0)
                            return a1;

                        return a1.Data.ChargeCooldownRemaining < a2.Data.CooldownRemaining
                            ? a1 : a2;
                    }

                    else if (a2.Data.HasCharges)
                    {
                        if (a2.Data.RemainingCharges > 0)
                            return a2;

                        return a2.Data.ChargeCooldownRemaining < a1.Data.CooldownRemaining
                            ? a2 : a1;
                    }

                    else
                    {
                        return a1.Data.CooldownRemaining < a2.Data.CooldownRemaining
                            ? a1 : a2;
                    }
                }

                // One or the other
                return a1.Data.IsCooldown ? a2 : a1;
            }

            static (uint ActionID, CooldownData Data) Selector(uint actionID) => (actionID, GetCooldown(actionID));

            return actions
                .Select(Selector)
                .Aggregate((a1, a2) => Compare(original, a1, a2))
                .ActionID;
        }

        /// <summary> Checks if the provided actionID has enough cooldown remaining to weave against it without causing clipping.</summary>
        /// <param name="weaveTime"> Time when weaving window is over. Defaults to 0.7. </param>
        /// 
        /// <returns> True or false. </returns>
        public static bool CanWeave(double weaveTime = 0.7) => RemainingGCD > weaveTime;

        /// <summary> Checks if the provided actionID has enough cooldown remaining to weave against it without causing clipping and checks if you're casting a spell. </summary>
        /// <param name="weaveTime"> Time when weaving window is over. Defaults to 0.6. </param>
        /// 
        /// <returns> True or false. </returns>
        public static bool CanSpellWeave(double weaveTime = 0.6)
        {
            float castTimeRemaining = LocalPlayer.TotalCastTime - LocalPlayer.CurrentCastTime;

            if (RemainingGCD > weaveTime &&                          // Prevent GCD delay
                castTimeRemaining <= 0.5 &&                                                     // Show in last 0.5sec of cast so game can queue ability
                RemainingGCD - castTimeRemaining - weaveTime >= 0)   // Don't show if spell is still casting in weave window
                return true;
            return false;
        }

        /// <summary> Checks if the provided actionID has enough cooldown remaining to weave against it in the later portion of the GCD without causing clipping. </summary>
        /// <param name="start"> Time (in seconds) to start to check for the weave window. If this value is greater than half of a GCD, it will instead use half a GCD instead to ensure it lands in the latter half.</param>
        /// <param name="end"> Time (in seconds) to end the check for the weave window. </param>
        /// 
        /// <returns> True or false. </returns>
        public static unsafe bool CanDelayedWeave(double start = 1.25, double end = 0.6)
        {
            float halfGCD = GCDTotal / 2f;
            return RemainingGCD <= (start > halfGCD ? halfGCD : start) && RemainingGCD >= end;
        }

        public enum WeaveTypes
        {
            None,
            Weave,
            DelayWeave,
            SpellWeave
        }
        public static bool CheckWeave(WeaveTypes weave) => weave switch
        {
            WeaveTypes.None => true,
            WeaveTypes.Weave => CanWeave(),
            WeaveTypes.DelayWeave => CanDelayedWeave(),
            WeaveTypes.SpellWeave => CanSpellWeave(),
            _ => false
        };

        /// <summary>
        /// Returns the current combo timer.
        /// </summary>
        public static unsafe float ComboTimer => ActionManager.Instance()->Combo.Timer;

        /// <summary>
        /// Returns the last combo action.
        /// </summary>
        public static unsafe uint ComboAction => ActionManager.Instance()->Combo.Action;

        /// <summary>
        /// Gets the current Limit Break action (PVE only)
        /// </summary>
        public static unsafe uint LimitBreakAction => LimitBreakController.Instance()->GetActionId(Player.Object.Character(), (byte)Math.Max(0, (LimitBreakLevel - 1)));

        public static unsafe bool CanQueue(uint actionID)
        {
            bool alreadyQueued = ActionManager.Instance()->QueuedActionId != 0;
            bool inSlidecast = (LocalPlayer.TotalCastTime - LocalPlayer.CurrentCastTime) <= 0.5f;
            bool animLocked = ActionManager.Instance()->AnimationLock > 0;

            bool ret = !alreadyQueued && inSlidecast && !animLocked && ActionReady(actionID);
            return ret;
        }

        private static bool _raidwideInc;
        public static unsafe bool RaidWideCasting(float timeRemaining = 0f)
        {
            if (!EzThrottler.Throttle("RaidWideCheck", 100))
                return _raidwideInc;

            foreach (var caster in Svc.Objects.Where(x => x is IBattleChara chara && chara.IsHostile() && chara.IsCasting()).Cast<IBattleChara>())
            {
                if (Svc.Data.Excel.GetSheet<Lumina.Excel.Sheets.Action>().TryGetRow(caster.CastActionId, out var spell))
                {
                    byte type = spell.CastType;
                    byte range = spell.EffectRange;

                    if (type is 2 or 5 && range >= 30)
                    {
                        if (timeRemaining == 0f)
                            return _raidwideInc = true;
                       
                        if ((caster.TotalCastTime - caster.CurrentCastTime) <= timeRemaining)
                            return _raidwideInc = true;

                    }
                }
            }

            return _raidwideInc = false;
        }

        private static bool _beingTargetedHostile;
        public static bool BeingTargetedHostile
        {
            get
            {
                if (!EzThrottler.Throttle("BeingTargetedHostile", 100))
                    return _beingTargetedHostile;

                return _beingTargetedHostile = Svc.Objects.Any(x => x.IsHostile() && x is IBattleChara chara && chara.CastTargetObjectId == LocalPlayer.GameObjectId);
            }
        }

        /// <summary>
        /// Counts how many times an action has been used since combat started.
        /// </summary>
        /// <param name="actionId"></param>
        /// <returns></returns>
        public static int ActionCount(uint actionId) => ActionWatching.CombatActions.Count(x => x == OriginalHook(actionId));

        /// <summary>
        /// Counts how many times multiple actions have been used since combat started.
        /// </summary>
        /// <param name="actionIds"></param>
        /// <returns></returns>
        public static int ActionCount(uint[] actionIds)
        {
            int output = 0;
            foreach (var a in actionIds)
                output += ActionCount(a);

            return output;
        }

        /// <summary>
        /// Counts how many times an action has been used in combat since using another action
        /// </summary>
        /// <param name="actionToCheckAgainst"></param>
        /// <param name="actionToCount"></param>
        /// <returns></returns>
        public static int TimesUsedSinceOtherAction(uint actionToCheckAgainst, uint actionToCount)
        {
            if (!ActionWatching.CombatActions.Any(x => x == actionToCheckAgainst)) return 0;

            int startIdx = ActionWatching.CombatActions.LastIndexOf(actionToCheckAgainst);

            int output = 0;
            for (int i = startIdx; i < ActionWatching.CombatActions.Count; i++)
            {
                if (ActionWatching.CombatActions[i] == actionToCount)
                    output++;
            }

            return output;
        }

        /// <summary>
        /// Counts how many times multiple actions have been used in combat since using another action
        /// </summary>
        /// <param name="actionToCheckAgainst"></param>
        /// <param name="actionsToCount"></param>
        /// <returns></returns>
        public static int TimesUsedSinceOtherAction(uint actionToCheckAgainst, uint[] actionsToCount)
        {
            int output = 0;
            foreach(uint a in actionsToCount)
            {
                output += TimesUsedSinceOtherAction(actionToCheckAgainst, a);
            }

            return output;
        }
    }
}
