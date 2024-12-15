using System.Reflection;
using EFT.EnvironmentEffect;
using HarmonyLib;
using PerformanceImprovements.Models;
using PerformanceImprovements.Performance.Graphics;
using SPT.Reflection.Patching;
using UnityEngine;

namespace PerformanceImprovements.Performance.Graphics.Patches;

public class EnvironmentManagerUpdatePatch : ModulePatch
{
    private static SettingsModel Settings => GraphicSettingsManager.SettingsModel;
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(EnvironmentManager), nameof(EnvironmentManager.Update));
    }

    [PatchPostfix]
    public static void PatchPostfix(EnvironmentManager __instance)
    {
        if (QualitySettings.shadowResolution != Settings.ShadowResolution)
        {
            QualitySettings.shadowResolution = Settings.ShadowResolution;
        }

        if (QualitySettings.shadowCascades != GraphicSettingsManager.GetShadowCascadesCount())
        {
            QualitySettings.shadowCascades = GraphicSettingsManager.GetShadowCascadesCount();
        }
    }
}