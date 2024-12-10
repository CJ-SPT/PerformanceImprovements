using System;
using System.Collections.Generic;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.AssetsManager;
using HarmonyLib;
using PerformanceImprovements.Config;
using PerformanceImprovements.Utils;
using SPT.Reflection.Patching;
using UnityEngine;
using Logger = PerformanceImprovements.Utils.Logger;

namespace PerformanceImprovements.Bots.Patches;

internal class BotStandByUpdatePatch : ModulePatch
{
    private static bool IsLimitEnabled => Settings.EnableBotRangeLimiter.Value;
    private static IBotGame BotGame => Singleton<IBotGame>.Instance;
    public static List<BotOwner> SleepingOwners { get; } = [];
    
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
        // Disabled - Fika/QB
        if (Plugin.DisableBotManagement) return false;
        
        // Not time to check this bot yet
        if (IsLimitEnabled && ____nextCheckTime > Time.time) return false;
        
        // Bot Limiter is disabled and the bot is disabled
        if (!IsLimitEnabled && __instance.StandByType == BotStandByType.paused)
        {
            ____nextCheckTime = Time.time + 10f;
            __instance.StandByType = BotStandByType.active;
            return false;
        }
        
        // Bot has first-aid to do or is in combat, let them do it, add 30 seconds
        if (___botOwner_0.Medecine.FirstAid.Have2Do || ___botOwner_0.Memory.HaveEnemy)
        {
            Logger.LogWarning($"{___botOwner_0.Profile.Nickname} needs medical or is in combat");
            
            if (__instance.StandByType != BotStandByType.active)
            {
                EnableBot(___botOwner_0, __instance);
            }
            
            ____nextCheckTime = Time.time + 30f;
            return false;
        }

        // We are over the cap allowed to disable bots
        if (SleepingOwners.Count > Settings.MaxSleepingBots.Value)
        {
            if (__instance.StandByType != BotStandByType.active)
            {
                Logger.LogWarning($"{___botOwner_0.Profile.Nickname} Enable Reason: Over SleepingOwners Count");
                EnableBot(___botOwner_0, __instance);
            }
            
            ____nextCheckTime = Time.time + 10f;
            return false;
        }
        
        // Bot cannot be disabled, it is too close
        if (!CanBeDisabledByDistance(___botOwner_0))
        {
            if (__instance.StandByType != BotStandByType.active)
            {
                Logger.LogWarning($"{___botOwner_0.Profile.Nickname} Enable Reason: To Close to player");
                EnableBot(___botOwner_0, __instance);
            }
            
            ____nextCheckTime = Time.time + 10f;
            return false;
        }
        
        // Bot can be put to sleep
        if (CanBeDisabledByCount() && CanBotSideBeDisabled(___botOwner_0.GetPlayer) && CanBeDisabledByDistance(___botOwner_0))
        {
            if (___standByType != BotStandByType.paused)
            {
                Logger.LogWarning($"{___botOwner_0.Profile.Nickname} Disable Reason: General StandBy");
                DisableBot(___botOwner_0, __instance);
            }
            
            ____nextCheckTime = Time.time + 10f;
        }
        
        return false;
    }
    
    private static bool CanBotSideBeDisabled(Player bot)
    {
        if (bot.Side == EPlayerSide.Savage)
        {
            // Scavs
            if (Settings.DisableScavs.Value && !GameUtils.Bosses.Contains(bot.Profile.Info.Settings.Role))
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
    
    private static bool CanBeDisabledByCount()
    {
        return SleepingOwners.Count < Settings.MaxSleepingBots.Value;
    }
    
    private static bool CanBeDisabledByDistance(BotOwner owner)
    {
        var mainPlayer = GameUtils.GetMainPlayer();
        
        var disableDistance = LocationLimitDistances[mainPlayer.Location]();
        var enableDistance = disableDistance - 25;
        
        var trueDistance = Vector3.Distance(
            ((IPlayer)owner.GetPlayer).Position, 
            ((IPlayer)mainPlayer).Position);
        
        // Bot is closer than the enable difference and is disabled
        if (trueDistance < enableDistance)
        {
            return false;
        }
        
        // Bot is outside the disable radius and is enabled
        return trueDistance > disableDistance;
    }

    private static void DisableBot(BotOwner owner, BotStandBy standBy)
    {
        Utils.Logger.Warn($"Disabled Bot ({owner.Profile.Nickname})");
        
        standBy.StandByType = BotStandByType.paused;
        SleepingOwners.Add(owner);
    }

    private static void EnableBot(BotOwner owner, BotStandBy standBy)
    {
        Utils.Logger.Warn($"Enabled Bot ({owner.Profile.Nickname})");
        
        standBy.StandByType = BotStandByType.active;
        SleepingOwners.Remove(owner);
    }
}

internal class BotStandByActivatePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotStandBy), nameof(BotStandBy.Activate));
    }

    [PatchPostfix]
    public static void PatchPostfix(BotStandBy __instance, BotOwner ___botOwner_0)
    {
        if (__instance.StandByType == BotStandByType.active)
        {
            BotStandByUpdatePatch.SleepingOwners.Remove(___botOwner_0);
        }
    }
}