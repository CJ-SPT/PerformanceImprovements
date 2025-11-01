using BepInEx;
using DrakiaXYZ.VersionChecker;
using System;
using System.Linq;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using CTap;
using PerformanceImprovements.Config;
using PerformanceImprovements.EFTProfiler;
using PerformanceImprovements.Utils;
using SPT.Reflection.Patching;

namespace PerformanceImprovements;

[BepInPlugin("com.dirtbikercj.performanceImprovements", "Performance Improvements", BuildInfo.Version)]
[BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    public const int TarkovVersion = 40087;
    public static bool IsFikaPresent { get; private set; }

#if DEBUG
    internal static ClassProfiler? Profiler;
#endif
    
    internal static ManualLogSource? Log;
    private PatchManager? _patchManager;
    
    private void Awake()
    {
        if (!VersionChecker.CheckEftVersion(Logger, Info, Config))
        {
            throw new Exception("Invalid EFT Version");
        }

        Log = Logger;
        
        IsFikaPresent = Chainloader.PluginInfos.Keys.Contains("com.fika.core");

        if (IsFikaPresent)
        {
            Log.LogWarning("Mods with incompatible features detected, disabling features.");
        }
        
        Settings.Bind(Config);
        R.GetReflectionInfo();

        _patchManager = new PatchManager(this, true);
        _patchManager.EnablePatches();

#if DEBUG
        Profiler = new ClassProfiler();
        ConsoleCommands.RegisterCommands();
#endif
        
        DontDestroyOnLoad(this);
    }

#if DEBUG
    private void Update()
    {
        if (Profiler is not null && Settings.DumpAnalytics!.Value.IsDown())
        {
            Profiler.DumpAnalytics();
        }
    }
#endif
}