using System;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using HarmonyLib;
using PerformanceImprovements.Utils;
using SPT.Reflection.Patching;

namespace PerformanceImprovements.Performance.Bots.Patches;

[ExperimentalPatch]
public class StaticManagerUpdatePatch : ModulePatch
{
    internal event Action BotsGroupUpdateAction;
    
    public static StaticManagerUpdatePatch Instance { get; private set; }
    private static bool _updateBotsGroups;
    
    protected override MethodBase GetTargetMethod()
    {
        Instance = this;
        return AccessTools.Method(typeof(StaticManager), nameof(StaticManager.Update));
    }

    [PatchPrefix]
    public static bool PatchPrefix(StaticManager __instance, Action ___action_0)
    {
        __instance?.TimerManager?.Update();
        
        if (_updateBotsGroups && Instance.BotsGroupUpdateAction != null)
        {
            Instance.BotsGroupUpdateAction?.Invoke();
        }
        
        ___action_0?.Invoke();
        
        _updateBotsGroups = !_updateBotsGroups;
        
        return false;
    }
}

[ExperimentalPatch]
public class BotsGroupCtorPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Constructor(typeof(BotsGroup), 
            [ typeof(BotZone), typeof(IBotGame), typeof(BotOwner), typeof(List<BotOwner>), 
                typeof(DeadBodiesController), typeof(List<Player>), typeof(bool) ]);
    }

    [PatchPostfix]
    public static void PatchPostfix(BotsGroup __instance)
    {
        StaticManager.Instance.StaticUpdate -= __instance.method_2;
        StaticManagerUpdatePatch.Instance.BotsGroupUpdateAction += __instance.method_2;
    }
}

[ExperimentalPatch]
public class BotsGroupDisposePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotsGroup), nameof(BotsGroup.Dispose));
    }

    [PatchPostfix]
    public static void PatchPostfix(BotsGroup __instance)
    {
        StaticManagerUpdatePatch.Instance.BotsGroupUpdateAction -= __instance.method_2;
    }
}