using BepInEx;
using DrakiaXYZ.VersionChecker;
using System;
using BepInEx.Logging;
using PerformanceImprovements.EFTProfiler;
using PerformanceImprovements.Patches;
using PerformanceImprovements.Utils;

namespace PerformanceImprovements;

[BepInPlugin("com.dirtbikercj.performanceImprovements", "Performance Improvements", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    public const int TarkovVersion = 33420;

    internal static ManualLogSource? Log;

    private ClassProfiler? _eftProfiler;
    
    internal void Awake()
    {
        if (!VersionChecker.CheckEftVersion(Logger, Info, Config))
        {
            throw new Exception("Invalid EFT Version");
        }

        Log = Logger;
        Settings.Bind(Config);
        
        _eftProfiler = new ClassProfiler(typeof(BotsGroup));
        _eftProfiler.Enable();
        
        DontDestroyOnLoad(this);
        
        // BotsGroup
        new BotsGroupMethod9().Enable();
        new BotsGroupMethod12().Enable();
        new BotsGroupMethod16().Enable();
    }

    internal void Update()
    {
        if (Settings.DumpAnalytics.Value.IsDown() && _eftProfiler is not null)
        {
            _eftProfiler.DumpAnalytics();
        }
    }
}