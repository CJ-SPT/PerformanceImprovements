﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using EFT.UI;
using HarmonyLib;
using JetBrains.Annotations;
using PerformanceImprovements.Utils;

namespace PerformanceImprovements.EFTProfiler;

public class ClassProfiler()
{
    private readonly Harmony _harmony = new("Profiler");
    private static readonly Dictionary<MethodBase, AnalyticsModel> AnalyticsModels = []; 
    private static readonly string AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    [CanBeNull] private Type _currentTypeProfiling;
    private static bool _isEnabled;
    
    public void TryEnableOrDisable(bool enabled)
    {
        if (_currentTypeProfiling is null)
        {
            ConsoleScreen.LogError("No type is set to profile, use `set_class_to_profile` first.");
            return;
        }

        if (_isEnabled == enabled)
        {
            var str = enabled ? "Enabled" : "Disabled";
            ConsoleScreen.LogError($"Profiler is already {str}");
            return;
        }
        
        switch (enabled)
        {
            case true:
                AnalyticsModels.Clear();
                PatchAll();
                break;
            
            case false:
                UnPatchAll();
                break;
        }

        _isEnabled = enabled;
    }
    
    public void SetTypeToProfile(string typeString)
    {
        var type = AccessTools.TypeByName(typeString);
        
        if (type is null)
        {
            ConsoleScreen.LogError($"Could not resolve type `{typeString}`");
            return;
        }

        _currentTypeProfiling = type;
        _isEnabled = true;
        UnPatchAll();
        PatchAll();
    }
    
    public void DumpAnalytics()
    {
        foreach (var method in AnalyticsModels.Values)
        {
            method.CalculateBenchmark();
        }

        var json = AnalyticsModels
            .OrderByDescending(t => t.Value.MaxTime)
            .ThenByDescending(t => t.Value.AvgTime)
            .ToJson();
        
        File.WriteAllText(Path.Combine(AssemblyPath, $"{_currentTypeProfiling!.Name}_timings.json"), json);
        
        Plugin.Log!.LogWarning("Analytics dumped to disk...");
    }

    private void PatchAll()
    {
        try
        {
            foreach (var method in _currentTypeProfiling!.GetMethods())
            {
                if (method.IsVirtual || !method.IsDeclaredMember()) continue;
                
                Plugin.Log!.LogDebug($"Patching method {method.Name}");
                
                var harmonyPrefix =
                    new HarmonyMethod(AccessTools.Method(typeof(ProfilerPatch), nameof(ProfilerPatch.Prefix)));
                var harmonyPostfix =
                    new HarmonyMethod(AccessTools.Method(typeof(ProfilerPatch), nameof(ProfilerPatch.Postfix)));

                _harmony.Patch(method, harmonyPrefix, harmonyPostfix);
            }
        }
        catch (Exception e)
        {
            Plugin.Log!.LogFatal(e);
            throw;
        }
    }

    private void UnPatchAll()
    {
        _harmony.UnpatchSelf();
    }
    
    private static void AddEntry(MethodBase method, long elapsed, bool isMainThread)
    {
        if (!AnalyticsModels.TryGetValue(method, out var model))
        {
            model = new AnalyticsModel();
            model.AllTimings.Add(elapsed);
            model.IsMainThread = isMainThread;
            
            AnalyticsModels.Add(method, model);
            return;
        }
        
        AnalyticsModels[method].AllTimings.Add(elapsed);
        AnalyticsModels[method].IsMainThread = isMainThread;
    }
    
    private class ProfilerPatch()
    {
        public static void Prefix(out Stopwatch __state)
        {
            __state = new Stopwatch();
            __state.Restart();
        }
        
        public static void Postfix(Stopwatch __state, MethodBase __originalMethod)
        {
            __state.Stop();

            AddEntry(__originalMethod, __state.ElapsedMilliseconds, !Thread.CurrentThread.IsBackground);
            
            if (__state.ElapsedMilliseconds == 0) return;

            var methodString = $"{__originalMethod.DeclaringType!.Name}.{__originalMethod.Name}()";
            
            if (__state.ElapsedMilliseconds > 20)
            {
                Plugin.Log!.LogError($"{methodString} took {__state.ElapsedMilliseconds} ms");
            }
            else if (__state.ElapsedMilliseconds > 10)
            {
                Plugin.Log!.LogWarning($"{methodString} took {__state.ElapsedMilliseconds} ms");
            }
            else
            {
                Plugin.Log!.LogInfo($"{methodString} took {__state.ElapsedMilliseconds} ms");
            }
        }
    }
}