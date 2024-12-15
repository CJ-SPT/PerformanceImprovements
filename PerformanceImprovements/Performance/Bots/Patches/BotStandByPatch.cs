using System;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using HarmonyLib;
using PerformanceImprovements.Config;
using PerformanceImprovements.Utils;
using SPT.Reflection.Patching;
using UnityEngine;

namespace PerformanceImprovements.Performance.Bots.Patches;

//[FikaDisablePatch]
internal class BotStandByUpdatePatch : ModulePatch
{
    private static bool IsLimitEnabled => Settings.EnableBotLimiter.Value;
    
    /// <summary>
    /// Reset in BotControllerInitPatch
    /// </summary>
    public static int DisabledBots;
    
    private static readonly Dictionary<string, Func<int>> LocationLimitDistances = new()
    {
        { "factory4_day", () => Settings.FactoryDisableDistance.Value },
        { "factory4_night", () => Settings.FactoryDisableDistance.Value },
        { "Woods", () => Settings.WoodsDisableDistance.Value },
        { "bigmap", () => Settings.CustomsDisableDistance.Value },
        { "Interchange", () => Settings.InterchangeDisableDistance.Value },
        { "RezervBase", () => Settings.ReserveDisableDistance.Value },
        { "Shoreline", () => Settings.ShorelineDisableDistance.Value },
        { "laboratory", () => Settings.LabsDisableDistance.Value },
        { "Lighthouse", () => Settings.LighthouseDisableDistance.Value },
        { "TarkovStreets", () => Settings.StreetsDisableDistance.Value },
        { "Sandbox", () => Settings.GroundZeroDisableDistance.Value },
        { "Sandbox_high", () => Settings.GroundZeroDisableDistance.Value }
    };
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotStandBy), nameof(BotStandBy.Update));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotStandBy __instance, BotOwner ___botOwner_0, BotStandByType ___standByType, ref float ____nextCheckTime)
    {
        switch (IsLimitEnabled)
        {
            case true:
                if (____nextCheckTime > Time.time)
                {
                    return false;
                }
                break;
            
            case false:
                // Limiter is disabled, run the base limiter code
                return true;
        }
        
        if (TryEnableBot(___botOwner_0))
        {
            ____nextCheckTime = Time.time + 10f;
            return false;
        }

        TryDisableBot(___botOwner_0);
        
        ____nextCheckTime = Time.time + 10f;
        return false;
    }

    /// <summary>
    /// Checks all the conditions required to enable a bot
    /// </summary>
    /// <param name="owner"></param>
    /// <returns>True if enabled a bot</returns>
    private static bool TryEnableBot(BotOwner owner)
    {
        // Bot is active, no need to check anything else.
        if (owner.StandBy.StandByType == BotStandByType.active) return false;
        
        // Bot cannot be disabled
        if (!CanBeDisabledByCount() || !CanBeDisabledByDistance(owner))
        {
            Utils.Logger.Debug($"bot {owner.ProfileId} has been activated");
            
            owner.StandBy.StandByType = BotStandByType.active;
            DisabledBots--;
            return true;
        }
        
        return false;
    }

    private static void TryDisableBot(BotOwner owner)
    {
        // Bot is already in Standby, or is actively in combat
        if (owner.StandBy.StandByType == BotStandByType.paused || BotIsInCombat(owner)) return;
        
        // Bot can be put to sleep
        if (CanBeDisabledByCount() && CanBotSideBeDisabled(owner.GetPlayer) && CanBeDisabledByDistance(owner))
        {
            Utils.Logger.Debug($"bot {owner.ProfileId} has been paused");
            
            owner.StandBy.StandByType = BotStandByType.paused;
            DisabledBots++;
        }
    }
    
    /// <summary>
    /// Check if the bot is actively in combat or needs to heal
    /// </summary>
    /// <param name="owner">Owner to check</param>
    /// <returns>True if bot is in combat or needs medical attention</returns>
    private static bool BotIsInCombat(BotOwner owner)
    {
        return owner.Medecine.FirstAid.Have2Do || owner.Memory.HaveEnemy;
    }
    
    /// <summary>
    /// Check if a bot can be disabled by type of bot
    /// </summary>
    /// <param name="bot">owner to check</param>
    /// <returns>True if bot can be disabled</returns>
    private static bool CanBotSideBeDisabled(Player bot)
    {
        if (bot.Side == EPlayerSide.Savage)
        {
            // Scavs
            if (Settings.DisableScavs.Value &&
                bot.Profile.Info.Settings.Role != WildSpawnType.marksman &&
                !GameUtils.Bosses.Contains(bot.Profile.Info.Settings.Role))
            {
                return true;
            }
            
            // Snipers
            if (Settings.DisableSniperScavs.Value && bot.Profile.Info.Settings.Role == WildSpawnType.marksman)
            {
                return true;
            }
            
            // Bosses
            if (Settings.DisableBosses.Value && GameUtils.Bosses.Contains(bot.Profile.Info.Settings.Role))
            {
                return true;
            }
        }
        
        // Pmcs
        if ((bot.Side == EPlayerSide.Bear || bot.Side == EPlayerSide.Usec) && Settings.DisablePmcs.Value)
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Check the sleeping bot count vs the config entry
    /// </summary>
    /// <returns>True if bot can be disabled</returns>
    private static bool CanBeDisabledByCount()
    {
        return Settings.MaxActiveBots.Value > DisabledBots;
    }
    
    /// <summary>
    /// Check of the bot can be disabled by distance
    /// </summary>
    /// <param name="owner">Owner to check</param>
    /// <returns>True if bot can be disabled</returns>
    private static bool CanBeDisabledByDistance(BotOwner owner)
    {
        var mainPlayer = GameUtils.GetMainPlayer();
        
        var disableDistance = LocationLimitDistances[mainPlayer.Location]();
        var enableDistance = disableDistance - 25;
        
        var trueDistance = Vector3.Distance(
            ((IPlayer)owner.GetPlayer).Position, 
            ((IPlayer)mainPlayer).Position);
        
        // Bot is closer than the enable difference
        if (trueDistance < enableDistance)
        {
            return false;
        }
        
        // Bot is outside the disable radius and is enabled
        return trueDistance > disableDistance;
    }
}

[FikaDisablePatch]
internal class BotStandByActivatePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotStandBy), nameof(BotStandBy.Activate));
    }

    [PatchPostfix]
    public static void PatchPostfix(BotStandBy __instance, BotOwner ___botOwner_0)
    {
        BotStandByUpdatePatch.DisabledBots--;
    }
}