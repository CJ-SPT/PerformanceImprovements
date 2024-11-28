using BepInEx;
using DrakiaXYZ.VersionChecker;
using System;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using EFT;
using JetBrains.Annotations;
using PerformanceImprovements.EFTProfiler;
using PerformanceImprovements.Utils;
using UnityEngine;

namespace PerformanceImprovements;

[BepInPlugin("com.dirtbikercj.performanceImprovements", "Performance Improvements", "0.1.0")]
[BepInDependency("com.Arys.UnityToolkit")]
public class Plugin : BaseUnityPlugin
{
    public const int TarkovVersion = 33420;

    [CanBeNull] internal static ManualLogSource Log;
    [CanBeNull] internal static ClassProfiler Profiler;
    
    internal void Awake()
    {
        if (!VersionChecker.CheckEftVersion(Logger, Info, Config))
        {
            throw new Exception("Invalid EFT Version");
        }

        Log = Logger;
        Settings.Bind(Config);
        PatchManager.EnablePatches();

#if DEBUG
        Profiler = new ClassProfiler();
        ConsoleCommands.RegisterCommands();
#endif
        
        DontDestroyOnLoad(this);
    }

#if DEBUG
    internal void Update()
    {
        if (Profiler is not null && Settings.DumpAnalytics.Value.IsDown())
        {
            Profiler.DumpAnalytics();
        }
    }
#endif
}