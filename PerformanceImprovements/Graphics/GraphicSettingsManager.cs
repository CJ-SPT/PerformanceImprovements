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
        
        // Logger.Debug($"Setting Shadow Resolution: {resolution}");
        SettingsModel.ShadowResolution = resolution;
        
        SaveSettings();
    }
    
    public static void ShadowCascadeSettingChanged(ShadowCascades cascades)
    {
        if (SettingsModel.ShadowCascades == cascades || cascades == ShadowCascades.None) return;
        
        // Logger.Debug($"Setting Shadow Cascades: {cascades}");
        SettingsModel.ShadowCascades = cascades;
        
        SaveSettings();
    }

    public static int GetShadowCascadesCount()
    {
        switch (SettingsModel.ShadowCascades)
        {
            // case ShadowCascades.None:
               // return 0;
            case ShadowCascades.Two:
                return 2;
            case ShadowCascades.Four:
                return 4;
            default:
                return 2;
        }
    }
    
    public static void LoadSettings()
    {
        if (!File.Exists(_settingsPath))
        {
            Logger.Warn("Settings file not found, creating default settings.");
            
            SettingsModel = SettingsModel.CreateDefault();
            
            var str = JsonConvert.SerializeObject(SettingsModel, Formatting.Indented);
            File.WriteAllText(_settingsPath, str);
            return;
        }
        
        Logger.Info($"Loading graphics settings from: {_settingsPath}");
        SettingsModel = JsonConvert.DeserializeObject<SettingsModel>(File.ReadAllText(_settingsPath), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

        if (SettingsModel.ShadowCascades == ShadowCascades.None)
        {
            Logger.Warn("Fixing ShadowCascades to default.");
            SettingsModel.ShadowCascades = ShadowCascades.Two;
        }
    }

    private static void SaveSettings()
    {
        File.WriteAllText(_settingsPath, JsonConvert.SerializeObject(SettingsModel, Formatting.Indented));
    }
}