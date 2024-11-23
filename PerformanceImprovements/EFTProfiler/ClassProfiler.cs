using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using PerformanceImprovements.Utils;

namespace PerformanceImprovements.EFTProfiler;

public class ClassProfiler(Type targetType)
{
    private readonly Harmony _harmony = new($"{targetType.Name} Profiler");
    
    // { method, model } }
    private static readonly Dictionary<MethodBase, AnalyticsModel> AnalyticsModels = []; 
    
    private static string _path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    
    public void Enable()
    {
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
        
        File.WriteAllText(Path.Combine(_path, $"{targetType.Name}_timings.json"), json);
        
        Plugin.Log!.LogWarning("Analytics dumped to disk...");
    }

    private void PatchAll()
    {
        try
        {
            foreach (var method in targetType.GetMethods())
            {
                if (method.IsVirtual) continue;
                
                Plugin.Log!.LogInfo($"Patching method {method.Name}");
                
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

    private static void AddEntry(MethodBase method, long elapsed)
    {
        if (!AnalyticsModels.TryGetValue(method, out var model))
        {
            model = new AnalyticsModel();
            model.AllTimings.Add(elapsed);
            
            AnalyticsModels.Add(method, model);
            return;
        }
        
        AnalyticsModels[method].AllTimings.Add(elapsed);
    }
    
    private class ProfilerPatch()
    {
        public static void Prefix(out Stopwatch __state)
        {
            if (!Settings.EnableProfiler.Value)
            {
                __state = null;
                return;
            }
            
            __state = new Stopwatch();
            __state.Restart();
        }
        
        public static void Postfix(Stopwatch __state, MethodBase __originalMethod)
        {
            if (!Settings.EnableProfiler.Value) return;
            
            __state.Stop();

            AddEntry(__originalMethod, __state.ElapsedMilliseconds);
            
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