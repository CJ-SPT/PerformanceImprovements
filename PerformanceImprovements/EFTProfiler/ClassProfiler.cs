using System;
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
        
        Logger.Warn("Analytics dumped to disk...");
    }

    private void PatchAll()
    {
        try
        {
            foreach (var method in _currentTypeProfiling!.GetMethods())
            {
                if (!method.IsDeclaredMember() ||
                    method.IsGenericMethod) continue;
                
                Logger.Debug($"Patching method {method.Name}");
                
                var harmonyPrefix =
                    new HarmonyMethod(AccessTools.Method(typeof(ProfilerPatch), nameof(ProfilerPatch.Prefix)));
                var harmonyPostfix =
                    new HarmonyMethod(AccessTools.Method(typeof(ProfilerPatch), nameof(ProfilerPatch.Postfix)));

                _harmony.Patch(method, harmonyPrefix, harmonyPostfix);
            }
        }
        catch (Exception e)
        {
            Logger.Fatal(e.Message);
            throw;
        }
    }

    private void UnPatchAll()
    {
        _harmony.UnpatchSelf();
    }
    
    private static void AddEntry(MethodBase method, double elapsed, bool isMainThread)
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

            var milliseconds = __state.Elapsed.TotalMilliseconds;
            
            AddEntry(__originalMethod, milliseconds, !Thread.CurrentThread.IsBackground);
            
            if (__state.Elapsed.Milliseconds == 0) return;

            var methodString = $"{__originalMethod.DeclaringType!.Name}.{__originalMethod.Name}()";

            switch (milliseconds)
            {
                case > 5 and < 10:
                    Logger.Warn($"{methodString} took {milliseconds} ms");
                    break;
                case > 10:
                    Logger.Error($"{methodString} took {milliseconds} ms");
                    break;
            }
        }
    }
}