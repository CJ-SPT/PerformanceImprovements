using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using PerformanceImprovements.Models;
using UnityEngine;
using Logger = PerformanceImprovements.Utils.Logger;

namespace PerformanceImprovements.Graphics;

public static class GraphicSettingsManager
{
    private static string _settingsPath;
    
    static GraphicSettingsManager()
    {
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var dir = Directory.GetParent(path).FullName;
        
        _settingsPath = Path.Combine(dir, "config", "com.dirtbikercj.performanceimprovements.graphicsettings.json");
    }
    
    public static SettingsModel SettingsModel { get; private set; }
    
    public static void ShadowResolutionSettingChanged(ShadowResolution resolution)
    {
        if (SettingsModel.ShadowResolution == resolution) return;
        
        Logger.Debug($"Setting Shadow Resolution: {resolution}");
        SettingsModel.ShadowResolution = resolution;
        
        SaveSettings();
    }

    public static void LoadSettings()
    {
        if (!File.Exists(_settingsPath))
        {
            Logger.Warn("Settings file not found, creating default settings.");
            
            SettingsModel = new SettingsModel();
            
            var str = JsonConvert.SerializeObject(SettingsModel, Formatting.Indented);
            File.WriteAllText(_settingsPath, str);
            return;
        }
        
        Logger.Info($"Loading graphics settings from: {_settingsPath}");
        SettingsModel = JsonConvert.DeserializeObject<SettingsModel>(File.ReadAllText(_settingsPath));
    }

    private static void SaveSettings()
    {
        File.WriteAllText(_settingsPath, JsonConvert.SerializeObject(SettingsModel, Formatting.Indented));
    }
}