using BepInEx;
using DrakiaXYZ.VersionChecker;
using System;
using System.Linq;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using CTap;
using JetBrains.Annotations;
using PerformanceImprovements.Config;
using PerformanceImprovements.EFTProfiler;
using PerformanceImprovements.Performance.Graphics;
using PerformanceImprovements.Utils;
using UnityEngine;

namespace PerformanceImprovements;

[BepInPlugin("com.dirtbikercj.performanceImprovements", "Performance Improvements", BuildInfo.Version)]
[BepInDependency("com.Arys.UnityToolkit")]
[BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("me.sol.sain", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.DanW.QuestingBots", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    public const int TarkovVersion = 33420;
    
    public static bool IsFikaPresent { get; private set; }
    public static bool IsSainPresent { get; private set; }
    public static bool IsQuestingBotsPresent { get; private set; }
    internal static bool DisableBotManagement => IsFikaPresent || IsQuestingBotsPresent;
    
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
        
        GraphicSettingsManager.LoadSettings();
        
        IsFikaPresent = Chainloader.PluginInfos.Keys.Contains("com.fika.core");
        IsSainPresent = Chainloader.PluginInfos.Keys.Contains("me.sol.sain");
        IsQuestingBotsPresent = Chainloader.PluginInfos.Keys.Contains("com.DanW.QuestingBots");

        if (IsFikaPresent || IsQuestingBotsPresent)
        {
            Utils.Logger.Warn("Mods with incompatible features detected, disabling features.");
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