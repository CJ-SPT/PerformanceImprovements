using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx.Configuration;
using EFT.Settings.Graphics;
using Newtonsoft.Json;
using PerformanceImprovements.Models;
using UnityEngine;

namespace PerformanceImprovements.Config;

public static class Settings
{
    private static readonly List<ConfigEntryBase> ConfigEntries = [];
    
    private const string ProfilerSection = "Profiler";
    public static ConfigEntry<KeyboardShortcut> DumpAnalytics;

    private const string GeneralSection = "General";
    public static ConfigEntry<bool> UseExperimentalPatches;
    
    private const string GraphicsSection = "Graphics";
    public static ConfigEntry<bool> EnableShadowSettings;
    
    private const string BotLimitSection = "Bot Limiter";
    public static ConfigEntry<bool> EnableBotLimiter;
    public static ConfigEntry<int> MaxActiveBots;
    public static ConfigEntry<bool> DisableScavs;
    public static ConfigEntry<bool> DisableSniperScavs;
    public static ConfigEntry<bool> DisablePmcs;
    public static ConfigEntry<bool> DisableBosses;
    public static ConfigEntry<int> FactoryDisableDistance;
    public static ConfigEntry<int> WoodsDisableDistance;
    public static ConfigEntry<int> CustomsDisableDistance;
    public static ConfigEntry<int> InterchangeDisableDistance;
    public static ConfigEntry<int> ReserveDisableDistance;
    public static ConfigEntry<int> ShorelineDisableDistance;
    public static ConfigEntry<int> LabsDisableDistance;
    public static ConfigEntry<int> LighthouseDisableDistance;
    public static ConfigEntry<int> StreetsDisableDistance;
    public static ConfigEntry<int> GroundZeroDisableDistance;
    
    private const string ScopeResolutionSection = "Scope Resolution";
    public static ConfigEntry<bool> EnableScopeResolutionMod;
    public static ConfigEntry<float> SamplingDownScale;
    public static ConfigEntry<EDLSSMode> DlssMode;
    public static ConfigEntry<EFSR2Mode> Fsr2Mode;
    public static ConfigEntry<EFSR3Mode> Fsr3Mode;
    
    private const string SceneCleanerSection = "Clutter";
    public static ConfigEntry<bool> EnableSceneCleaner;
    public static ConfigEntry<bool> DisableGarbage;
    public static ConfigEntry<bool> DisableHeaps;
    public static ConfigEntry<bool> DisableSpentCartridges;
    public static ConfigEntry<bool> DisableFoodDrink;
    public static ConfigEntry<bool> DisableDecals;
    public static ConfigEntry<bool> DisablePuddles;
    public static ConfigEntry<bool> DisableShards;
    
    public static void Bind(ConfigFile config)
    {
#if DEBUG
        BindDebugOptions(config);
#endif
        BindGeneral(config);
        BindGraphics(config);
        
        // Disable the bot limiter if incompatible plugins are found.
        if (!Plugin.DisableBotManagement)
        { 
            BindBotLimiter(config);
        }
        
        BindScopeResolutionOptions(config);
        // BindSceneCleanerOptions(config);
        
        RecalcOrder();
        
        config.SettingChanged += OnConfigUpdated;
    }

    private static void BindGeneral(ConfigFile config)
    {
        ConfigEntries.Add(UseExperimentalPatches = config.Bind(
            GeneralSection,
            "Use Experimental Patches(Requires Restart)",
            false,
            new ConfigDescription(
                "Enables or disables the use of experimental patches. Requires a restart. " +
                "These range from logic optimizations to threading optimizations. ",
                null,
                new ConfigurationManagerAttributes { })));
    }

    private static void BindGraphics(ConfigFile config)
    {
        ConfigEntries.Add(EnableShadowSettings = config.Bind(
            GraphicsSection,
            "Enable Experimental Graphic Settings",
            true,
            new ConfigDescription(
                "Enables experimental graphic settings in the EFT graphics settings page. (REQUIRES RESTART)",
                null,
                new ConfigurationManagerAttributes { })));
    }
    
    private static void BindBotLimiter(ConfigFile config)
    {
        ConfigEntries.Add(EnableBotLimiter = config.Bind(
            BotLimitSection,
            "Enable Bot Distance Limiter",
            true,
            new ConfigDescription(
                "Deactivates bots that are a set distance away from you.",
                null,
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(MaxActiveBots = config.Bind(
            BotLimitSection,
            "Max Active Bot Count",
            16,
            new ConfigDescription(
                "The amount of bots that can be active at any given time",
                new AcceptableValueRange<int>(1, 50),
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(DisableScavs = config.Bind(
            BotLimitSection,
            "Limit Scavs",
            true,
            new ConfigDescription(
                "Should scavs be disabled?",
                null,
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(DisableSniperScavs = config.Bind(
            BotLimitSection,
            "Limit Sniper Scavs",
            true,
            new ConfigDescription(
                "Should snipers be disabled?",
                null,
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(DisablePmcs = config.Bind(
            BotLimitSection,
            "Limit Pmcs",
            true,
            new ConfigDescription(
                "Should pmcs be disabled?",
                null,
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(DisableBosses = config.Bind(
            BotLimitSection,
            "Limit Bosses",
            true,
            new ConfigDescription(
                "Should bosses be disabled?",
                null,
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(FactoryDisableDistance = config.Bind(
            BotLimitSection,
            "Factory Distance",
            50,
            new ConfigDescription(
                "Distance at which to disable bots.",
                new AcceptableValueRange<int>(25, 1000),
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(WoodsDisableDistance = config.Bind(
            BotLimitSection,
            "Woods Distance",
            250,
            new ConfigDescription(
                "Distance at which to disable bots.",
                new AcceptableValueRange<int>(150, 1000),
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(CustomsDisableDistance = config.Bind(
            BotLimitSection,
            "Customs Distance",
            225,
            new ConfigDescription(
                "Distance at which to disable bots.",
                new AcceptableValueRange<int>(125, 1000),
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(InterchangeDisableDistance = config.Bind(
            BotLimitSection,
            "Interchange Distance",
            175,
            new ConfigDescription(
                "Distance at which to disable bots.",
                new AcceptableValueRange<int>(125, 1000),
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(ReserveDisableDistance = config.Bind(
            BotLimitSection,
            "Reserve Distance",
            175,
            new ConfigDescription(
                "Distance at which to disable bots.",
                new AcceptableValueRange<int>(125, 1000),
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(ShorelineDisableDistance = config.Bind(
            BotLimitSection,
            "Shoreline Distance",
            250,
            new ConfigDescription(
                "Distance at which to disable bots.",
                new AcceptableValueRange<int>(150, 1000),
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(LabsDisableDistance = config.Bind(
            BotLimitSection,
            "Labs Distance",
            75,
            new ConfigDescription(
                "Distance at which to disable bots.",
                new AcceptableValueRange<int>(50, 1000),
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(LighthouseDisableDistance = config.Bind(
            BotLimitSection,
            "Lighthouse Distance",
            225,
            new ConfigDescription(
                "Distance at which to disable bots.",
                new AcceptableValueRange<int>(125, 1000),
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(StreetsDisableDistance = config.Bind(
            BotLimitSection,
            "Streets Distance",
            150,
            new ConfigDescription(
                "Distance at which to disable bots.",
                new AcceptableValueRange<int>(75, 1000),
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(GroundZeroDisableDistance = config.Bind(
            BotLimitSection,
            "GroundZero Distance",
            175,
            new ConfigDescription(
                "Distance at which to disable bots.",
                new AcceptableValueRange<int>(125, 1000),
                new ConfigurationManagerAttributes { })));
    }
    
    private static void BindScopeResolutionOptions(ConfigFile config)
    {
        ConfigEntries.Add(EnableScopeResolutionMod = config.Bind(
            ScopeResolutionSection,
            "Enable Dynamic Scope Resolution",
            false,
            new ConfigDescription(
                "Should rendering resolutions change when scoped in?",
                null,
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(SamplingDownScale = config.Bind(
            ScopeResolutionSection,
            "Super Sampling",
            0.5f,
            new ConfigDescription(
                "How much down sampling to apply, this only works if there is no DLSS or FSR mode enabled.",
                new AcceptableValueRange<float>(0.01f, 0.99f),
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(DlssMode = config.Bind(
            ScopeResolutionSection,
            "DLSS mode",
            EDLSSMode.Off,
            new ConfigDescription(
                "The DLSS mode to apply to optics, this is only used if DLSS is the selected scaling mode in the EFT graphics settings.",
                null,
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(Fsr2Mode = config.Bind(
            ScopeResolutionSection,
            "FSR2 mode",
            EFSR2Mode.Off,
            new ConfigDescription(
                "The FSR2 mode to apply to optics, this is only used if FSR2 is the selected scaling mode in the EFT graphics settings.",
                null,
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(Fsr3Mode = config.Bind(
            ScopeResolutionSection,
            "FSR3 mode",
            EFSR3Mode.Off,
            new ConfigDescription(
                "The FSR3 mode to apply to optics, this is only used if FSR3 is the selected scaling mode in the EFT graphics settings.",
                null,
                new ConfigurationManagerAttributes { })));
    }

    private static void BindSceneCleanerOptions(ConfigFile config)
    {
        ConfigEntries.Add(EnableSceneCleaner = config.Bind(
            SceneCleanerSection,
            "Scene Cleaner Enabled",
            true,
            new ConfigDescription(
                "Master scene cleaner option, needs to be enabled for anything else in this section to function.",
                null,
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(DisableGarbage = config.Bind(
            SceneCleanerSection,
            "Disable Garbage",
            true,
            new ConfigDescription(
                "The garbage man came to town.",
                null,
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(DisableHeaps = config.Bind(
            SceneCleanerSection,
            "Disable Heaps",
            true,
            new ConfigDescription(
                "The garbage man came to town.",
                null,
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(DisableSpentCartridges = config.Bind(
            SceneCleanerSection,
            "Disable Spent Cartridges",
            true,
            new ConfigDescription(
                "Pick up your brass.",
                null,
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(DisableFoodDrink = config.Bind(
            SceneCleanerSection,
            "Disable Food&Drink",
            true,
            new ConfigDescription(
                "Does anyone even read these?",
                null,
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(DisableDecals = config.Bind(
            SceneCleanerSection,
            "Disable Decals",
            true,
            new ConfigDescription(
                "The stickers? ",
                null,
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(DisablePuddles = config.Bind(
            SceneCleanerSection,
            "Disable Puddles",
            true,
            new ConfigDescription(
                "No more wet feet.",
                null,
                new ConfigurationManagerAttributes { })));
        
        ConfigEntries.Add(DisableShards = config.Bind(
            SceneCleanerSection,
            "Disable Shards",
            true,
            new ConfigDescription(
                "Stepping on glass sucks.",
                null,
                new ConfigurationManagerAttributes { })));
    }
    
    private static void BindDebugOptions(ConfigFile config)
    {
        ConfigEntries.Add(DumpAnalytics = config.Bind(
            ProfilerSection,
            "Dump Analytics",
            new KeyboardShortcut(KeyCode.F10),
            new ConfigDescription(
                "Dump the analytics json to disk",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Browsable = false })));
    }
    
    private static void RecalcOrder()
    {
        // Set the Order field for all settings, to avoid unnecessary changes when adding new settings
        var settingOrder = ConfigEntries.Count;
        foreach (var entry in ConfigEntries)
        {
            var attributes = entry.Description.Tags[0] as ConfigurationManagerAttributes;
            if (attributes != null)
            {
                attributes.Order = settingOrder;
            }

            settingOrder--;
        }
    }

    private static void OnConfigUpdated(object sender, SettingChangedEventArgs e)
    {
        
    }
}