﻿using System.Reflection;
using HarmonyLib;
using PerformanceImprovements.Utils;
using SPT.Reflection.Patching;
using UnityEngine;

namespace PerformanceImprovements.Bots.Patches;

[DisablePatch]
public class GoToPointPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotMover), nameof(BotMover.GoToPoint), 
            new []{typeof(Vector3), typeof(bool), typeof(float), typeof(bool), typeof(bool), typeof(bool), typeof(bool)});
    }

    [PatchPrefix]
    public static bool PatchPrefix()
    {
        return true;
    }
}