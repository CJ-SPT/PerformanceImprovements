using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace PerformanceImprovements.Patches;

public class BotsGroupMethod9 : ModulePatch
{
    private static readonly Dictionary<IPlayer, List<BotOwner>> Dict1 = [];
    private static readonly Dictionary<IPlayer, List<EnemyInfo>> Dict2 = [];
    private static readonly Func<EnemyInfo, float> PositionDiff = BotsGroup.Class266.class266_0.method_2;
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotsGroup), nameof(BotsGroup.method_9));
    }
    
    [PatchPrefix]
    public static bool PrefixPatch(List<BotOwner> ____members)
    {
        Dict1.Clear();
        Dict2.Clear();

        AddBotEnemiesToCollection(____members);
        SetBotCurrentTactic(____members);
        
        return false;
    }

    private static void AddBotEnemiesToCollection(List<BotOwner> members)
    {
        foreach (var botOwner in members)
        {
            if (botOwner.Memory.GoalEnemy is null) continue;
            
            if (!Dict1.TryGetValue(botOwner.Memory.GoalEnemy.Person, out var list))
            {
                list = [];
                Dict1.Add(botOwner.Memory.GoalEnemy.Person, list);
            }

            if (!Dict2.TryGetValue(botOwner.Memory.GoalEnemy.Person, out var list2))
            {
                list2 = [];
                Dict2.Add(botOwner.Memory.GoalEnemy.Person, list2);
            }

            list.Add(botOwner);
            list2.Add(botOwner.Memory.GoalEnemy);
        }
    }

    private static void SetBotCurrentTactic(List<BotOwner> members)
    {
        var num = Mathf.Clamp(
            (int)(members.Count * GClass583.Core.PERCENT_PERSON_SEARCH) + 1,
            GClass583.Core.MIN_MAX_PERSON_SEARCH, 
            100);

        var num2 = 0;

        foreach (var botOwner in Dict1)
        {
            if (!Dict2.TryGetValue(botOwner.Key, out var enemyInfos)) continue;
            if (enemyInfos.Count == 0) continue;

            var enemyInfosOrdered = enemyInfos.OrderBy(PositionDiff).ToArray();

            for (var i = 0; i < enemyInfosOrdered.Length; i++)
            {
                var isAttacking = num2 < num && enemyInfosOrdered[i].Owner.Tactic.IsCurTactic(BotsGroup.BotCurrentTactic.Attack);
                enemyInfosOrdered[i].CanISearch = isAttacking;

                if (isAttacking) 
                {
                    num2++;
                    enemyInfosOrdered[i].SearchIndex = i;
                    continue;
                }

                enemyInfosOrdered[i].SearchIndex = -1;
            }
        }
    }
}

public class BotsGroupMethod12 : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotsGroup), nameof(BotsGroup.method_12));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotsGroup __instance)
    {
        foreach (var enemy in __instance.Enemies.ToArray())
        {
            if (enemy.Key.HealthController.IsAlive) continue;

            __instance.DeletePlayerCauseDead(enemy.Key);
        }

        foreach (var neutral in __instance.Neutrals.ToArray())
        {
            if (neutral.Key.HealthController.IsAlive) continue;

            __instance.DeletePlayerCauseDead(neutral.Key);
        }
        
        return false;
    }
}

public class BotsGroupMethod16 : ModulePatch
{
    private static readonly Func<BotOwner, float> AggressionCoef = BotsGroup.Class266.class266_0.method_6;
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotsGroup), nameof(BotsGroup.method_16));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotsGroup __instance, List<BotOwner> ____members)
    {
        if (GClass583.Core.MAIN_TACTIC_ONLY_ATTACK)
        {
            foreach (var botOwner in ____members)
            {
                __instance.method_17(botOwner, BotsGroup.BotCurrentTactic.Attack);
            }

            return false;
        }

        var aggressionCoef = ____members.Sum(AggressionCoef) / ____members.Count;
        var currentTactic = (__instance.GroupPower * aggressionCoef < __instance.Single_0)
            ? BotsGroup.BotCurrentTactic.Ambush
            : BotsGroup.BotCurrentTactic.Attack;

        foreach (var botOwner in ____members)
        {
            __instance.method_17(botOwner, currentTactic);
        }
        
        return false;
    }
}