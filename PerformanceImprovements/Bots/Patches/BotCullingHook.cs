using System.Reflection;
using EFT;
using HarmonyLib;
using PerformanceImprovements.Utils;
using SPT.Reflection.Patching;

namespace PerformanceImprovements.Bots.Patches;

[FikaDisablePatch]
public class BotCullingHookPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return AccessTools.Method(typeof(BotsController), nameof(BotsController.Init));
	}

	[PatchPostfix]
	public static void PatchPrefix(BotsController __instance)
	{
		BotCullingManager.Instance = GameUtils.GetGameWorld().GetOrAddComponent<BotCullingManager>();
	}
}