using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace PerformanceImprovements.Bots.Patches;

public class BotEnemyChooserBetterEnemyPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotEnemyChooser), nameof(BotEnemyChooser.BetterEnemy));
    }

    [PatchPrefix]
    public static bool PatchPrefix(
        BotEnemyChooser __instance,
        ref EnemyInfo __result,
        List<EnemyInfo> enemiesInfos,
        BotOwner ___botOwner_0)
    {
        if (enemiesInfos.Count == 0)
        {
            __result = null;
            return false;
        }

        IPlayer player = null;
        var weight = float.MaxValue;
        
        foreach (var enemyInfo in enemiesInfos)
        {
            if (enemyInfo.IgnoreUntilAggression) continue;
            
            var distance = enemyInfo.Distance;
            if (distance >= ___botOwner_0.Settings.FileSettings.Mind.MAX_AGGRO_BOT_DIST && 
                distance > ___botOwner_0.Settings.FileSettings.Mind.MAX_AGGRO_BOT_DIST_UPPER_LIMIT &&
                enemyInfo.IsVisible) continue;

            var calculatedWeight = __instance.CalcWeight(distance, enemyInfo);
            
            if (calculatedWeight < weight)
            {
                weight = calculatedWeight;
                player = enemyInfo.Person;
            }
        }

        if (player is null)
        {
            __result = null;
            return false;
        }
        
        __result = ___botOwner_0.EnemiesController.EnemyInfos[player];
        return false;
    }
}