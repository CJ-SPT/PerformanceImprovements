using System;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using HarmonyLib;
using PerformanceImprovements.Config;
using PerformanceImprovements.Utils;
using SPT.Reflection.Patching;
using UnityEngine;

namespace PerformanceImprovements.Bots.Patches;

public class BotStandByPatch : ModulePatch
{
    private static bool IsEnabled => Settings.EnableBotLimiter.Value;

    private static readonly HashSet<WildSpawnType> Bosses = [
        WildSpawnType.bossBoar,             // Kaban
        WildSpawnType.bossBully,            // Reshala
        WildSpawnType.bossGluhar,           // Glukhar
        WildSpawnType.bossKilla,
        WildSpawnType.bossKnight,
        WildSpawnType.followerBigPipe,
        WildSpawnType.followerBirdEye,
        WildSpawnType.bossKolontay,
        WildSpawnType.bossKojaniy,          // Shturman
        WildSpawnType.bossPartisan,
        WildSpawnType.bossSanitar,
        WildSpawnType.bossTagilla,
        WildSpawnType.bossZryachiy,
        WildSpawnType.gifter,               // Santa
        WildSpawnType.arenaFighterEvent,    // Blood Hounds
        WildSpawnType.sectantPriest,        // Cultist Priest
        (WildSpawnType) 199,                // Legion
        (WildSpawnType) 801,                // Punisher
    ];

    private static readonly Dictionary<string, Func<int>> LocationDistances = new()
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
        if (Plugin.DisableBotLimiter) return true;
        
        if (!IsEnabled)
        {
            // Mod is disabled, reactivate bots and let the base code handle things
            if (__instance.StandByType == BotStandByType.paused)
            {
                __instance.StandByType = BotStandByType.active;
            }
            
            return true;
        }
        
        if (____nextCheckTime > Time.time && !CanBotBeDisabled(___botOwner_0.GetPlayer)) return false;
    
        ____nextCheckTime = Time.time + 10f;
        
        var mainPlayer = GameUtils.GetMainPlayer();
        
        var trueDistance = Vector3.Distance(
            ((IPlayer)___botOwner_0.GetPlayer).Position, 
            ((IPlayer)mainPlayer).Position);
        
        var disableDistance = LocationDistances[mainPlayer.Location].Invoke();
        var enableDistance = disableDistance - 25;
        
        if (trueDistance < enableDistance)
        {
            if (___standByType != BotStandByType.paused) return false;
            
            __instance.StandByType = BotStandByType.active;
            return false;
        }
        
        if (trueDistance > disableDistance)
        {
            if (___standByType != BotStandByType.active) return false;

            __instance.StandByType = BotStandByType.paused;
        }
        
        return false;
    }
    
    private static bool CanBotBeDisabled(Player player)
    {
        if (player.Side == EPlayerSide.Savage)
        {
            // Scavs
            if (Settings.DisableScavs.Value && !Bosses.Contains(player.Profile.Info.Settings.Role))
            {
                return true;
            }
            
            // Bosses
            if (Settings.DisableBosses.Value && Bosses.Contains(player.Profile.Info.Settings.Role))
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
}