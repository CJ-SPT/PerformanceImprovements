using System.Reflection;
using EFT;
using HarmonyLib;
using PerformanceImprovements.Utils;
using SPT.Reflection.Patching;

namespace PerformanceImprovements.Performance.Bots.Patches;

[QBDisablePatch]
[FikaDisablePatch]
public class BotControllerInitPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return AccessTools.Method(typeof(BotsController), nameof(BotsController.Init));
	}

	[PatchPostfix]
	public static void PatchPostfix()
	{
		BotStandByUpdatePatch.DisabledBots = 0;
	}
}