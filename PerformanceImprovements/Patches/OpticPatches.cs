using System.Reflection;
using EFT;
using EFT.CameraControl;
using HarmonyLib;
using PerformanceImprovements.Config;
using PerformanceImprovements.Graphics;
using PerformanceImprovements.Utils;
using SPT.Reflection.Patching;

namespace PerformanceImprovements.Patches;

public class OpticEnablePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(OpticSight), nameof(OpticSight.OnEnable));
    }

    [PatchPostfix]
    public static void PatchPostfix()
    {
        if (!Settings.EnableScopeResolutionMod.Value) return;

        var player = GameUtils.GetMainPlayer();
        var isAiming = player.ProceduralWeaponAnimation.IsAiming;
        var aimMod = player.ProceduralWeaponAnimation.CurrentAimingMod;
        var isOptic = player.ProceduralWeaponAnimation.CurrentScope.IsOptic;
        
        if (isAiming && isOptic && aimMod is not null)
        {
            GraphicsUtils.SetScopeResolution();
        }
    }
}

public class OpticDisablePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(OpticSight), nameof(OpticSight.OnDisable));
    }

    [PatchPostfix]
    public static void PatchPostfix()
    {
        if (!Settings.EnableScopeResolutionMod.Value) return;
        
        GraphicsUtils.SetDefaultResolution();
    }
}

public class ChangeAimingModePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.ChangeAimingMode));
    }

    [PatchPostfix]
    public static void PatchPostfix()
    {
        if (!Settings.EnableScopeResolutionMod.Value) return;
        
        var player = GameUtils.GetMainPlayer();
        var isAiming = player.ProceduralWeaponAnimation.IsAiming;
        var aimMod = player.ProceduralWeaponAnimation.CurrentAimingMod;
        var isOptic = player.ProceduralWeaponAnimation.CurrentScope.IsOptic;
        
        if (isAiming && aimMod is not null)
        {
            if (isOptic)
            {
                GraphicsUtils.SetScopeResolution();
            }
            else
            {
                GraphicsUtils.SetDefaultResolution();
            }
        }
    }
}