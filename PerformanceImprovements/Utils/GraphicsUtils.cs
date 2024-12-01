using System.Reflection;
using Comfort.Common;
using EFT.Settings.Graphics;
using HarmonyLib;
using UnityEngine;

namespace PerformanceImprovements.Utils;

public static class GraphicsUtils
{
    private static SharedGameSettingsClass _sharedGameSettings;
    private static GClass1039 _clonedSettings;
    private static FieldInfo SsaaImplField;
    
    static GraphicsUtils()
    {
        _sharedGameSettings = Singleton<SharedGameSettingsClass>.Instance;
        SsaaImplField = AccessTools.Field(typeof(CameraClass), "ssaaimpl_0");
    }
    
    private static SharedGameSettingsClass GetGameSettings()
    {
        if (_sharedGameSettings is null)
        {
            _sharedGameSettings = Singleton<SharedGameSettingsClass>.Instance;
        }

        return _sharedGameSettings;
    }
    
    private static bool IsDlssEnabled()
    {
        return GetGameSettings().Graphics.Settings.DLSSEnabled;
    }
    
    private static bool IsFsr2Enabled()
    {
        return GetGameSettings().Graphics.Settings.FSR2Enabled;
    }
    
    private static bool IsFsr3Enabled()
    {
        return GetGameSettings().Graphics.Settings.FSR3Enabled;
    }
    
    private static EAntialiasingMode GetAAMode()
    {
        return GetGameSettings().Graphics.Settings.AntiAliasing.Value;
    }

    private static float GetSuperSamplingFactor()
    {
        return GetGameSettings().Graphics.Settings.SuperSamplingFactor;
    }
    
    private static EDLSSMode GetCurrentDlssMode()
    {
        return GetGameSettings().Graphics.Settings.DLSSMode.Value;
    }
    
    private static EFSR2Mode GetCurrentFsr2Mode()
    {
        return GetGameSettings().Graphics.Settings.FSR2Mode.Value;
    }
    
    private static EFSR3Mode GetCurrentFsr3Mode()
    {
        return GetGameSettings().Graphics.Settings.FSR3Mode.Value;
    }
    
    public static void SetScopeResolution()
    {
        // Backup the current graphic settings
        _clonedSettings = GetGameSettings().Graphics.Settings.Clone();
        
        var camera = GameUtils.GetCameraClass();

        if (!IsDlssEnabled() && !IsFsr2Enabled() && !IsFsr3Enabled() && Settings.SamplingDownScale.Value < GetSuperSamplingFactor())
        {
            ((SSAAImpl)SsaaImplField.GetValue(camera))
                .Switch(Mathf.Clamp(Settings.SamplingDownScale.Value, 0f, 1f));
        }
        
        if (IsDlssEnabled())
        {
            camera.SetAntiAliasing(GetAAMode(), Settings.DlssMode.Value, GetCurrentFsr2Mode(), GetCurrentFsr3Mode());
            return;
        }

        if (IsFsr2Enabled())
        {
            camera.SetFSR2(Settings.Fsr2Mode.Value);
            return;
        }

        if (IsFsr3Enabled())
        {
            camera.SetFSR3(Settings.Fsr3Mode.Value);
        }
    }

    public static void SetDefaultResolution()
    {
        var camera = GameUtils.GetCameraClass();

        if (!_clonedSettings.DLSSEnabled || !_clonedSettings.FSR2Enabled || !_clonedSettings.FSR3Enabled)
        {
            ((SSAAImpl)SsaaImplField.GetValue(camera))
                .Switch(Mathf.Clamp(_clonedSettings.SuperSamplingFactor, 0f, 1f));
        }
        
        if (_clonedSettings.DLSSEnabled)
        {
            camera.SetAntiAliasing(
                _clonedSettings.AntiAliasing.Value, _clonedSettings.DLSSMode.Value, 
                _clonedSettings.FSR2Mode.Value, _clonedSettings.FSR3Mode.Value
                );
            
            return;
        }
        
        if (_clonedSettings.FSR2Enabled)
        {
            camera.SetFSR2(_clonedSettings.FSR2Mode.Value);
            return;
        }

        if (_clonedSettings.FSR3Enabled)
        {
            camera.SetFSR3(_clonedSettings.FSR3Mode.Value);
        }

        _clonedSettings = null;
    }
}