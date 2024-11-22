using System;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace PerformanceImprovements.EFTProfiler;

public class Timer(string callerName)
{
    private readonly Stopwatch _sw = new();
    
    public void Start()
    {
        _sw.Restart();
    }
    
    public void Stop()
    {
        if (_sw.ElapsedMilliseconds <= 0) return; 
        
        Plugin.Log!.LogDebug($"{callerName} took {_sw.ElapsedMilliseconds} ms");
    }
}

/// <summary>
/// Helper class patch to profile method run times
/// </summary>
/// <param name="type"></param>
/// <param name="method"></param>
public class MethodProfiler(Type type, string method) : ModulePatch
{
    private static Timer? _timer;
    
    protected override MethodBase GetTargetMethod()
    {
        _timer = new Timer($"{nameof(type)}.{method}");
        
        return AccessTools.Method(type, method);
    }

    [PatchPrefix]
    public static void PatchPrefix()
    {
        _timer!.Start();
    }

    [PatchPostfix]
    public static void PatchPostfix()
    {
        _timer!.Stop();
    }
}