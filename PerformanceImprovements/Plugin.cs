using BepInEx;
using DrakiaXYZ.VersionChecker;
using System;
using BepInEx.Logging;
using JetBrains.Annotations;
using PerformanceImprovements.Core;
using PerformanceImprovements.EFTProfiler;
using PerformanceImprovements.Threading;
using PerformanceImprovements.Utils;
using UnityEngine;

namespace PerformanceImprovements;

[BepInPlugin("com.dirtbikercj.performanceImprovements", "Performance Improvements", "0.2.0")]
[BepInDependency("com.Arys.UnityToolkit")]
public class Plugin : BaseUnityPlugin
{
    public const int TarkovVersion = 33420;

    [CanBeNull] internal static ManualLogSource Log;
    [CanBeNull] internal static ClassProfiler Profiler;

    internal static GameObject HookObject;
    
    private void Awake()
    {
        if (!VersionChecker.CheckEftVersion(Logger, Info, Config))
        {
            throw new Exception("Invalid EFT Version");
        }

        Log = Logger;
        
        HookObject = new GameObject();
        DontDestroyOnLoad(HookObject);
        HookObject.AddComponent<UnityMainThreadDispatcher>();
        // HookObject.AddComponent<SceneCleaner>();
        
        Settings.Bind(Config);
        R.GetReflectionInfo();
        PatchManager.EnablePatches();

#if DEBUG
        Profiler = new ClassProfiler();
        ConsoleCommands.RegisterCommands();
#endif
        
        DontDestroyOnLoad(this);
    }

#if DEBUG
    private void Update()
    {
        if (Profiler is not null && Settings.DumpAnalytics.Value.IsDown())
        {
            Profiler.DumpAnalytics();
        }
    }
#endif
}