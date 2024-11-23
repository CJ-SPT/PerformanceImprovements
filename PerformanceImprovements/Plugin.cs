using BepInEx;
using DrakiaXYZ.VersionChecker;
using System;
using BepInEx.Logging;
using EFT;
using JetBrains.Annotations;
using PerformanceImprovements.EFTProfiler;
using PerformanceImprovements.Patches;
using PerformanceImprovements.Utils;

namespace PerformanceImprovements;

[BepInPlugin("com.dirtbikercj.performanceImprovements", "Performance Improvements", "1.0.0")]
[BepInDependency("com.Arys.UnityToolkit")]
public class Plugin : BaseUnityPlugin
{
    public const int TarkovVersion = 33420;

    [CanBeNull] internal static ManualLogSource Log;
    [CanBeNull] private ClassProfiler _eftProfiler;
    
    internal void Awake()
    {
        if (!VersionChecker.CheckEftVersion(Logger, Info, Config))
        {
            throw new Exception("Invalid EFT Version");
        }

        Log = Logger;
        Settings.Bind(Config);
        
        _eftProfiler = new ClassProfiler(typeof(GClass888));
        _eftProfiler.Enable();
        
        DontDestroyOnLoad(this);
        
        // TODO: This breaks aggression
        //new BotsGroupAddMember().Enable();
        
        new DeadBodiesControllerAddBody().Enable();
    }

    internal void Update()
    {
        if (Settings.DumpAnalytics.Value.IsDown() && _eftProfiler is not null)
        {
            _eftProfiler.DumpAnalytics();
        }
    }
}