using BepInEx;
using DrakiaXYZ.VersionChecker;
using System;
using System.Linq;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using JetBrains.Annotations;
using PerformanceImprovements.Config;
using PerformanceImprovements.EFTProfiler;
using PerformanceImprovements.Threading;
using PerformanceImprovements.Utils;
using UnityEngine;
using Logger = PerformanceImprovements.Utils.Logger;

namespace PerformanceImprovements;

[BepInPlugin("com.dirtbikercj.performanceImprovements", "Performance Improvements", "0.1.4")]
[BepInDependency("com.Arys.UnityToolkit")]
[BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)] // Used to disable the bot limiter
[BepInDependency("com.DanW.QuestingBots", BepInDependency.DependencyFlags.SoftDependency)] // Used to disable the bot limiter
public class Plugin : BaseUnityPlugin
{
    public const int TarkovVersion = 33420;
    
    private static bool _isFikaPresent;
    private static bool _isQuestingBotsPresent;
    internal static bool DisableBotLimiter => _isFikaPresent || _isQuestingBotsPresent;
    
    [CanBeNull] internal static ClassProfiler Profiler;

    internal static GameObject HookObject;
    internal static ManualLogSource Log;
    
    private void Awake()
    {
        if (!VersionChecker.CheckEftVersion(Logger, Info, Config))
        {
            throw new Exception("Invalid EFT Version");
        }

        Log = Logger;
        
        _isFikaPresent = Chainloader.PluginInfos.Keys.Contains("com.fika.core");
        _isQuestingBotsPresent = Chainloader.PluginInfos.Keys.Contains("com.DanW.QuestingBots");

        if (_isFikaPresent || _isQuestingBotsPresent)
        {
            Utils.Logger.Warn("Mods with compatible features detected, disabling features.");
        }
        
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