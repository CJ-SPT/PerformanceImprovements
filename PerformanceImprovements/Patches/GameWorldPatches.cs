using System.Reflection;
using EFT;
using HarmonyLib;
using PerformanceImprovements.Core;
using SPT.Reflection.Patching;

namespace PerformanceImprovements.Patches;

public class GameWorldAwakePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
    }

    [PatchPostfix]
    public static void PatchPostfix(GameWorld __instance)
    {
        //__instance.GetOrAddComponent<SceneCleaner>();
    }
}