using System.Reflection;
using Bsg.GameSettings;
using EFT.UI.Settings;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace PerformanceImprovements.Graphics.Patches;

public class GraphicsSettingsTabShowPatch : ModulePatch
{
    private static GameSetting<ShadowResolution> _shadowDistance;
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GraphicsSettingsTab), nameof(GraphicsSettingsTab.Show));
    }

    [PatchPostfix]
    public static void PatchPostfix(
        GraphicsSettingsTab __instance,
        Transform ____otherSettingsContainer,
        SettingDropDown ____dropDownPrefab,
        SettingDropDown ____shadowsQuality,
        GClass1039 ____tempSettings)
    {
        
        var startingIndex = ____shadowsQuality.transform.parent.GetSiblingIndex();
        
        AddNewDropDowns(__instance, 
            ____otherSettingsContainer, 
            ____dropDownPrefab, 
            startingIndex,
            ____tempSettings);
    }

    private static void AddNewDropDowns(
        GraphicsSettingsTab settingsTab,
        Transform parent,
        SettingDropDown prefab,
        int index,
        GClass1039 tmpSettings)
    {
        var graphicsSettings = GraphicSettingsManager.SettingsModel;
        
        var shadowDistDropdown = settingsTab.CreateControl(prefab, parent); 
        shadowDistDropdown.GetOrCreateTooltip().SetMessageText("Settings/Graphics/ShadowResolutionTooltip");
        shadowDistDropdown.transform.SetSiblingIndex(index + 1);
        _shadowDistance = tmpSettings.method_4("Settings/Graphics/ShadowResolution", graphicsSettings.ShadowResolution);
        shadowDistDropdown.BindToEnum(_shadowDistance);

        _shadowDistance.Bind(GraphicSettingsManager.ShadowResolutionSettingChanged);
    }
}