using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace PerformanceImprovements.Patches;

public class DeadBodiesControllerAddBody : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(DeadBodiesController), nameof(DeadBodiesController.AddBody),
            new[] { typeof(IPlayer) });
    }

    [PatchPrefix]
    public static bool PatchPrefix()
    {
        UniTask.RunOnThreadPool(AddBody);
        return false;
    }

    private static UniTask AddBody()
    {
        
        
        return UniTask.CompletedTask;
    }
}