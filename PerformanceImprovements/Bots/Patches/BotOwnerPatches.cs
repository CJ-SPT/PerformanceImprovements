using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using EFT;
using HarmonyLib;
using PerformanceImprovements.Utils;
using SPT.Reflection.Patching;
using UnityEngine;

namespace PerformanceImprovements.Bots.Patches;

[ExperimentalPatch]
public class BotOwnerMethod9 : ModulePatch
{
    private static GameWorld _gameWorld;
    
    private static CancellationToken _cancellationToken;
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotOwner), nameof(BotOwner.method_9));
    }

    [PatchPrefix]
    public static bool PatchPrefix(
        BotOwner __instance,
        DamageInfoStruct damageInfo,
        EBodyPart bodyType,
        float damageReducedByArmor)
    {
        if (!_gameWorld)
        {
            _gameWorld = GameUtils.GetGameWorld();
        }
        
        Task.Run(() => GetHit(
            __instance, 
            damageInfo, 
            bodyType, 
            damageReducedByArmor,
            Time.time));
        
        return false;
    }

    private static Task GetHit(
        BotOwner owner,
        DamageInfoStruct damageInfo,
        EBodyPart bodyType,
        float damageReducedByArmor,
        float time
        )
    {
        owner.StandBy.GetHit();

        if (damageInfo.Player == null)
        {
            return Task.CompletedTask;
        }

        // This did a lot of unnecessary checks before
        if (!damageInfo.Player.IsAI && damageInfo.Player.iPlayer.Side == EPlayerSide.Savage)
        {
            _gameWorld.GetAlivePlayerByProfileID(damageInfo.Player.iPlayer.ProfileId)
                ?.Loyalty
                ?.MarkAsCanBeFreeKilled();
        }
        
        owner.BotPersonalStats.GetHit(damageInfo, bodyType);
        owner.Memory.GetHit(damageInfo);
        
        if (damageInfo.Player.iPlayer == null) return Task.CompletedTask;

        lock (owner.EnemiesController.EnemyInfos)
        {
            if (owner.EnemiesController.EnemyInfos.TryGetValue(damageInfo.Player.iPlayer, out var enemyInfo))
            {
                enemyInfo.LastGetHitTime = time;
            }
        }

        if (damageInfo.Player.iPlayer.Side == owner.Side)
        {
            owner.BotTalk.TrySay(EPhraseTrigger.FriendlyFire);
        }
        
        return Task.CompletedTask;
    }
}