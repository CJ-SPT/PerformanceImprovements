using System.Collections.Generic;
using System.Reflection;
using EFT.AssetsManager;
using EFT.UI;
using EFT.UI.Settings;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace PerformanceImprovements.UI.Patches;

internal class SoundSettingsScreenShowPatch : ModulePatch
{
    public static SettingSelectSlider SelectSliderPrefab;
    public static SettingFloatSlider FloatSliderPrefab;
    public static SettingDropDown DropDownPrefab;
    public static SettingToggle TogglePrefab;
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(SoundSettingsTab), nameof(SoundSettingsTab.Show));
    }

    [PatchPostfix]
    public static void PatchPostfix(
        SettingSelectSlider ____selectSliderPrefab,
        SettingFloatSlider ____floatSliderPrefab,
        SettingDropDown ____dropDownPrefab,
        SettingToggle ____togglePrefab
        )
    {
        SelectSliderPrefab = ____selectSliderPrefab;
        FloatSliderPrefab = ____floatSliderPrefab;
        DropDownPrefab = ____dropDownPrefab;
        TogglePrefab = ____togglePrefab;
    }
}

internal class SettingsScreenShowPatch : ModulePatch
{
    private static PerformanceSettingsTab _settingsTab;
    
    private static NumberSlider _numberSliderPrefab;
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(SettingsScreen), nameof(SettingsScreen.Show));
    }

    [PatchPostfix]
    public static void PatchPostfix(Dictionary<SettingsScreen.ESettingsGroup, SettingsScreen.Struct1103> ___dictionary_0)
    {
       // ___dictionary_0.Add(SettingsScreen.ESettingsGroup.SettingsScreen, new SettingsScreen.Struct1103());
    }
}