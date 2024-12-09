using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PerformanceImprovements.Utils;
using SPT.Reflection.Patching;

namespace PerformanceImprovements.Patches;

public class AddLocalizationsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LocaleManagerClass), nameof(LocaleManagerClass.UpdateLocales));
    }

    [PatchPrefix]
    public static void PatchPrefix(LocaleManagerClass __instance, Dictionary<string, string> newLocale)
    {
        newLocale.AddRange(EmbededResourceUtil.GetEmbededLocalizationJson());
    }
}