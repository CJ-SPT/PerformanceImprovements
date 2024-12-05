using BepInEx;
using DrakiaXYZ.VersionChecker;
using System;
using System.Linq;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using JetBrains.Annotations;
using PerformanceImprovements.Core;
using PerformanceImprovements.EFTProfiler;
using PerformanceImprovements.Threading;
using PerformanceImprovements.Utils;
using UnityEngine;

namespace PerformanceImprovements;

[BepInPlugin("com.dirtbikercj.performanceImprovements", "Performance Improvements", "0.1.3")]
[BepInDependency("com.Arys.UnityToolkit")]
[BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)] // Used to disable the bot limiter
[BepInDependency("com.DanW.QuestingBots", BepInDependency.DependencyFlags.SoftDependency)] // Used to disable the bot limiter
public class Plugin : BaseUnityPlugin
{
    public const int TarkovVersion = 33420;
    
    private static bool _isFikaPresent;
    private static bool _isQuestingBotsPresent;
    
    internal static readonly bool DisableBotLimiter = _isFikaPresent || _isQuestingBotsPresent;
    
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

        _isFikaPresent = Chainloader.PluginInfos.Keys.Contains("com.fika.core");
        _isQuestingBotsPresent = Chainloader.PluginInfos.Keys.Contains("com.DanW.QuestingBots");

        if (_isFikaPresent || _isQuestingBotsPresent)
        {
            Log.LogWarning("Mods with compatible features detected, disabling features.");
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