using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace PerformanceImprovements.Utils;

public static class Settings
{
    private static readonly List<ConfigEntryBase> ConfigEntries = [];
    
    private const string ProfilerSection = "Profiler";
    public static ConfigEntry<KeyboardShortcut> DumpAnalytics;

    private const string BotLimitSection = "Bot Limiter";
    public static ConfigEntry<bool> EnableBotLimiter;
    public static ConfigEntry<bool> DisableScavs;
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
    
    public static void Bind(ConfigFile config)
    {
#if DEBUG
        BindDebugOptions(config);
#endif
        BindBotLimiter(config);
        RecalcOrder();
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
        
        ConfigEntries.Add(DisableScavs = config.Bind(
            BotLimitSection,
            "Limit Scavs",
            true,
            new ConfigDescription(
                "Should scavs be disabled?",
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

    private static void BindDebugOptions(ConfigFile config)
    {
        ConfigEntries.Add(DumpAnalytics = config.Bind(
            ProfilerSection,
            "Dump Analytics",
            new KeyboardShortcut(KeyCode.F10),
            new ConfigDescription(
                "Dump the analytics json to disk",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Browsable = false})));
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
}