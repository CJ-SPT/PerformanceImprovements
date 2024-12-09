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

namespace PerformanceImprovements.Bots.Patches;

public class BotStandByPatch : ModulePatch
{
    private static bool IsLimitEnabled => Settings.EnableBotRangeLimiter.Value;
    private static IBotGame BotGame => Singleton<IBotGame>.Instance;
    
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
        
        // Limit is enabled but the bot cannot be disabled
        if (IsLimitEnabled && !CanBotBeDisabled(___botOwner_0.GetPlayer))
        {
            ____nextCheckTime = Time.time + 10f;
            return false;
        }
        
        var mainPlayer = GameUtils.GetMainPlayer();
        var trueDistance = Vector3.Distance(
            ((IPlayer)___botOwner_0.GetPlayer).Position, 
            ((IPlayer)mainPlayer).Position);

        DisableBot(__instance, ___standByType, mainPlayer, trueDistance);
        
        ____nextCheckTime = Time.time + 10f;
        return false;
    }
    
    private static bool CanBotBeDisabled(Player player)
    {
        if (player.Side == EPlayerSide.Savage)
        {
            // Scavs
            if (Settings.DisableScavs.Value && !GameUtils.Bosses.Contains(player.Profile.Info.Settings.Role))
            {
                return true;
            }
            
            // Bosses
            if (Settings.DisableBosses.Value && GameUtils.Bosses.Contains(player.Profile.Info.Settings.Role))
            {
                return true;
            }
        }
        
        // Pmcs
        if ((player.Side == EPlayerSide.Bear || player.Side == EPlayerSide.Usec) && Settings.DisablePmcs.Value)
        {
            return true;
        }
        
        return false;
    }

    private static void DisableBot(BotStandBy standBy, BotStandByType standByType, Player mainPlayer, float distance)
    {
        var disableDistance = LocationLimitDistances[mainPlayer.Location]();
        var enableDistance = disableDistance - 25;
        
        // Enable the bot
        if (distance < enableDistance && standByType == BotStandByType.paused)
        {
            standBy.StandByType = BotStandByType.active;
            return;
        }
        
        // Disable the bot
        if (distance > disableDistance && standByType == BotStandByType.active)
        {
            standBy.StandByType = BotStandByType.paused;
        }
    }
}