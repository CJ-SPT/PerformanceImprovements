using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace PerformanceImprovements.Config;

public static class Settings
{
    private static readonly List<ConfigEntryBase> ConfigEntries = [];
    
    private const string ProfilerSection = "Profiler";
    public static ConfigEntry<KeyboardShortcut>? DumpAnalytics;
    
    public static void Bind(ConfigFile config)
    {
#if DEBUG
        BindDebugOptions(config);
#endif
        
        RecalcOrder();
        
        config.SettingChanged += OnConfigUpdated;
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