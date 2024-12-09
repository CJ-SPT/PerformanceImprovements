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
    public static void PatchPrefix(LocaleManagerClass __instance, string localeId, Dictionary<string, string> newLocale)
    {
        var json = EmbededResourceUtil.GetEmbededLocalizationJson();
        
        __instance.method_2(localeId, __instance.method_3(json));
        
        newLocale.AddRange(json);
    }
}

public class UpdateMainMenuLocale : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LocaleManagerClass), nameof(LocaleManagerClass.UpdateMainMenuLocales));
    }

    [PatchPostfix]
    public static void PatchPrefix(LocaleManagerClass __instance, string localeId, GClass2068 newLocale)
    {
        var json = EmbededResourceUtil.GetEmbededLocalizationJson();
        
        __instance.method_2(localeId, __instance.method_3(json));
        
        //newLocale.AddRange(EmbededResourceUtil.GetEmbededLocalizationJson());
    }
}