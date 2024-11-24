using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using EFT;
using HarmonyLib;
using PerformanceImprovements.Utils;
using SPT.Reflection.Patching;

namespace PerformanceImprovements.Bots.Patches;

[DisablePatch]
public class BotsGroupAddMember : ModulePatch
{ 
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotsGroup), nameof(BotsGroup.AddMember));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotsGroup __instance, BotOwner ally, bool onActivation, List<BotOwner> ____members)
    {
        UniTask.RunOnThreadPool(() => AddMember(__instance, ally, onActivation, ____members));
        return false;
    }

    private static UniTask AddMember(BotsGroup botsGroup, BotOwner ally, bool onActivation, List<BotOwner> members)
    {
        if (ally is null || members.Contains(ally) || botsGroup.Allies.Contains(ally) ||
            ally.BotState != EBotState.Active)
        {
            return UniTask.CompletedTask;
        }

        foreach (var enemy in botsGroup.Enemies)
        {
            ally.Memory.AddEnemy(enemy.Key, enemy.Value, onActivation);
        }

        if (ally.BotsGroup.BotZone == botsGroup.BotZone)
        {
            botsGroup.method_1(ally);
        }
        
        botsGroup.method_16();
        return UniTask.CompletedTask;
    }
}