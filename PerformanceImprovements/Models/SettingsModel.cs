using UnityEngine;

namespace PerformanceImprovements.Models;

public class SettingsModel
{
    public ShadowResolution ShadowResolution { get; set; }
    
    public ShadowCascades ShadowCascades { get; set; }

    public static SettingsModel CreateDefault()
    {
        return new SettingsModel()
        {
            ShadowResolution = ShadowResolution.Medium,
            ShadowCascades = ShadowCascades.Two,
        };
    }
}

public enum ShadowCascades
{
    None,
    Two,
    Four,
}