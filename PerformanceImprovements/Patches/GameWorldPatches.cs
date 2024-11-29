using System.Reflection;
using EFT;
using HarmonyLib;
using PerformanceImprovements.Bots;
using SPT.Reflection.Patching;

namespace PerformanceImprovements.Patches;

public class OnGameStartedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.Awake));
    }

    [PatchPostfix]
    public static void PatchPostfix(GameWorld __instance)
    {
        //__instance.GetOrAddComponent<BotLimiter>();
    }
}