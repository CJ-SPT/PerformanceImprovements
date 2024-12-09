using System.Reflection;
using EFT.Settings.Graphics;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace PerformanceImprovements.Graphics.Patches;

public class GetSuperSamplingFromQualityPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GClass1039), nameof(GClass1039.GetSuperSamplingFromQuality));
    }

    [PatchPrefix]
    public static bool PatchPrefix(ESamplingMode quality, ref float __result)
    {
        switch (quality)
        {
            case ESamplingMode.DownX05:
                __result = Mathf.Sqrt(0.5f); // 0.70
                break;
            case ESamplingMode.DownX06:
                __result = Mathf.Sqrt(0.60f); // 0.77
                break;
            case ESamplingMode.DownX07:
                __result = Mathf.Sqrt(0.70f); // 0.83
                break;
            case ESamplingMode.DownX075:
                __result = Mathf.Sqrt(0.75f); // 0.86
                break;
            case ESamplingMode.DownX08:
                __result = Mathf.Sqrt(0.80f); // 0.89
                break;
            case ESamplingMode.DownX09:
                __result = Mathf.Sqrt(0.90f); // 0.94
                break;
            
            default:
                __result = 1f;
                break;
        }

        return false;
    }
}