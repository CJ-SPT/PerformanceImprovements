using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace PerformanceImprovements.Utils;

public static class Settings
{
    private static readonly List<ConfigEntryBase> ConfigEntries = [];

    public static ConfigEntry<bool> EnableProfiler;
    public static ConfigEntry<KeyboardShortcut> DumpAnalytics;
    
    
    public static void Bind(ConfigFile config)
    {
        ConfigEntries.Add(EnableProfiler = config.Bind(
            "Profiler",
            "Enable Profiler",
            false,
            new ConfigDescription(
                "Enable the profiler.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true })));
        
        ConfigEntries.Add(DumpAnalytics = config.Bind(
            "Profiler",
            "Dump Analytics",
            new KeyboardShortcut(KeyCode.F10),
            new ConfigDescription(
                "Dump the analytics json to disk",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true })));
        
        RecalcOrder();
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