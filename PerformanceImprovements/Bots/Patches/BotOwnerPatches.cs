using System.Reflection;
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
        
        UniTask.RunOnThreadPool(() => GetHit(
            __instance, 
            damageInfo, 
            bodyType, 
            damageReducedByArmor,
            Time.time));
        
        return false;
    }

    private static async UniTaskVoid GetHit(
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
            return;
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
        
        if (damageInfo.Player.iPlayer == null) return;

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
        
        await UniTask.Yield();
    }
}