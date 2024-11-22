using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;

namespace PerformanceImprovements.EFTProfiler;

public class ClassProfiler(Type type)
{
    private readonly Harmony _harmony = new($"{type.Name} Profiler");
    private static readonly Dictionary<Type, List<AnalyticsModel>> AnalyticsModels = []; 
    
    public void Enable()
    {
        PatchAll();
    }
    
    public void DumpAnalytics()
    {
        
    }
    
    private void PatchAll()
    {
        foreach (var method in type.GetMethods())
        {
            Plugin.Log!.LogInfo($"Patching method {method.Name}");
         
            var harmonyPrefix = new HarmonyMethod(AccessTools.Method(typeof(ProfilerPatch), nameof(ProfilerPatch.Prefix)));
            var harmonyPostfix = new HarmonyMethod(AccessTools.Method(typeof(ProfilerPatch), nameof(ProfilerPatch.Postfix)));
            
            _harmony.Patch(method, harmonyPrefix, harmonyPostfix);
        }
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