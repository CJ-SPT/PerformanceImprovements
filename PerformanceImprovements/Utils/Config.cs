using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace PerformanceImprovements.Utils;

public static class Settings
{
    private static readonly List<ConfigEntryBase> ConfigEntries = [];
    
    #nullable disable
    public static ConfigEntry<KeyboardShortcut> DumpAnalytics;
    
    #nullable enable
    
    public static void Bind(ConfigFile config)
    {
        ConfigEntries.Add(DumpAnalytics = config.Bind(
            "Analytics",
            "Dump Analytics",
            new KeyboardShortcut(KeyCode.A),
            new ConfigDescription(
                "Dump the analytics json to disk",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true })));
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