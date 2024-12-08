using System.Reflection;
using Comfort.Common;
using EFT.Settings.Graphics;
using HarmonyLib;
using PerformanceImprovements.Config;
using PerformanceImprovements.Utils;
using UnityEngine;
using Logger = PerformanceImprovements.Utils.Logger;

namespace PerformanceImprovements.Graphics;

public static class GraphicsUtils
{
    private static SharedGameSettingsClass _sharedGameSettings;
    private static FieldInfo SsaaImplField;
    
    private static EDLSSMode _defaultDlssMode;
    private static EFSR2Mode _defaultFsr2Mode;
    private static EFSR3Mode _defaultRsr3Mode;
    
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
        return GetGameSettings().Graphics.Settings.DLSSMode.Value != EDLSSMode.Off;
    }
    
    private static bool IsFsr2Enabled()
    {
        return GetGameSettings().Graphics.Settings.FSR2Mode.Value != EFSR2Mode.Off;
    }
    
    private static bool IsFsr3Enabled()
    {
        return GetGameSettings().Graphics.Settings.FSR3Mode.Value != EFSR3Mode.Off;
    }
    
    private static EAntialiasingMode GetDefaultAAMode()
    {
        return GetGameSettings().Graphics.Settings.AntiAliasing.Value;
    }

    private static float GetDefaultSsFactor()
    {
        return GetGameSettings().Graphics.Settings.SuperSamplingFactor;
    }
    
    private static EDLSSMode GetDefaultDlssMode()
    {
        return GetGameSettings().Graphics.Settings.DLSSMode.Value;
    }
    
    private static EFSR2Mode GetDefaultFsr2Mode()
    {
        return GetGameSettings().Graphics.Settings.FSR2Mode.Value;
    }
    
    private static EFSR3Mode GetDefaultFsr3Mode()
    {
        return GetGameSettings().Graphics.Settings.FSR3Mode.Value;
    }
    
    public static void SetScopeResolution()
    {
        var camera = GameUtils.GetCameraClass();
        
        //Logger.Info($"DLSS ENABLED: {IsDlssEnabled()} MODE: {GetDefaultDlssMode()}");
        //Logger.Info($"FSR2 ENABLED: {IsFsr2Enabled()} MODE: {GetDefaultFsr2Mode()}");
        //Logger.Info($"FSR3 ENABLED: {IsFsr3Enabled()} MODE: {GetDefaultFsr3Mode()}");
        //Logger.Info($"SS Factor: {GetDefaultSsFactor()}");
        
        if (!IsDlssEnabled() && !IsFsr2Enabled() && !IsFsr3Enabled() && Settings.SamplingDownScale.Value < GetDefaultSsFactor())
        {
            ((SSAAImpl)SsaaImplField.GetValue(camera))
                .Switch(Mathf.Clamp(Settings.SamplingDownScale.Value, 0.01f, 0.99f));
            
            return;
        }
        
        if (IsDlssEnabled() && GetDefaultDlssMode() != Settings.DlssMode.Value)
        {
            camera.SetAntiAliasing(EAntialiasingMode.None, Settings.DlssMode.Value, EFSR2Mode.Off, EFSR3Mode.Off);
            return;
        }

        if (IsFsr2Enabled() && GetDefaultFsr2Mode() != Settings.Fsr2Mode.Value)
        {
            camera.SetFSR2(Settings.Fsr2Mode.Value);
            return;
        }

        if (IsFsr3Enabled() && GetDefaultFsr3Mode() != Settings.Fsr3Mode.Value)
        {
            camera.SetFSR3(Settings.Fsr3Mode.Value);
        }
    }

    public static void SetDefaultResolution()
    {
        var camera = GameUtils.GetCameraClass();
        
        Logger.Info($"DLSS ENABLED: {IsDlssEnabled()} MODE: {GetDefaultDlssMode()}");
        Logger.Info($"FSR2 ENABLED: {IsFsr2Enabled()} MODE: {GetDefaultFsr2Mode()}");
        Logger.Info($"FSR3 ENABLED: {IsFsr3Enabled()} MODE: {GetDefaultFsr3Mode()}");
        Logger.Info($"SS Factor: {GetDefaultSsFactor()}");
        
        if (!IsDlssEnabled() && !IsFsr3Enabled() && !IsFsr3Enabled())
        {
            ((SSAAImpl)SsaaImplField.GetValue(camera))
                .Switch(Mathf.Clamp(GetDefaultSsFactor(), 0.01f, 0.99f));
            
            return;
        }
        
        if (IsDlssEnabled() && GetDefaultDlssMode() != Settings.DlssMode.Value)
        {
            camera.SetAntiAliasing(
                EAntialiasingMode.None, GetDefaultDlssMode(),
                EFSR2Mode.Off, EFSR3Mode.Off
                );
            
            return;
        }
        
        if (IsFsr2Enabled() && GetDefaultFsr2Mode() != Settings.Fsr2Mode.Value)
        {
            camera.SetFSR2(GetDefaultFsr2Mode());
            return;
        }

        if (IsFsr3Enabled() && GetDefaultFsr3Mode() != Settings.Fsr3Mode.Value)
        {
            camera.SetFSR3(GetDefaultFsr3Mode());
        }
    }
}