using System.Collections.Generic;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace PerformanceImprovements.Bots.Patches;

public class BotsClassGetEnemiesPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotsClass), nameof(BotsClass.GetEnemies));
    }

    [PatchPrefix]
    public static bool PatchPrefix(
        BotsClass __instance, 
        BotOwner owner, 
        ref IEnumerable<BotOwner> __result, 
        HashSet<BotOwner> ___hashSet_0)
    {

        __result = [];
        foreach (var enemy in ___hashSet_0)
        {
            if (__instance.method_0(enemy, owner, owner.Settings.FileSettings))
            {
                ___hashSet_0.AddItem(enemy);
            }
        }
        
        return false;
    }
}