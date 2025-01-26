using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PerformanceImprovements.Config;
using SPT.Reflection.Patching;

namespace PerformanceImprovements.Utils;

public static class PatchManager
{
    public static void EnablePatches()
    {
        foreach (var patch in GetAllPatches())
        {
            ((ModulePatch)Activator.CreateInstance(patch)).Enable();
        }
    }

    public static void DisablePatches()
    {
        foreach (var patch in GetAllPatches())
        {
            ((ModulePatch)Activator.CreateInstance(patch)).Disable();
        }
    }
    
    public static void EnableTargetPatch(Type type)
    {
        ((ModulePatch)Activator.CreateInstance(type)).Enable();
    }
    
    public static void DisableTargetPatch(Type type)
    {
        ((ModulePatch)Activator.CreateInstance(type)).Disable();
    }
    
    private static IEnumerable<Type> GetAllPatches()
    {
        var patches = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.BaseType == typeof(ModulePatch) && 
                        t.GetCustomAttribute(typeof(DisablePatchAttribute)) == null);
        
        patches = Plugin.IsFikaPresent 
            ? patches.Where(t => t.GetCustomAttribute(typeof(FikaDisablePatchAttribute)) == null)
            : patches;
        
        patches = Plugin.IsQuestingBotsPresent 
            ? patches.Where(t => t.GetCustomAttribute(typeof(QBDisablePatchAttribute)) == null)
            : patches;
        
        return Settings.UseExperimentalPatches.Value
            ? patches
            : patches.Where(t => t.GetCustomAttribute(typeof(ExperimentalPatchAttribute)) == null);
    }
}

/// <summary>
/// Used to indicate a patch is not ready for prod and should be ignored during patching
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DisablePatchAttribute : Attribute
{ }

/// <summary>
/// Used to indicate a patch which is experimental and should be allowed to be disabled
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ExperimentalPatchAttribute : Attribute
{ }

/// <summary>
/// Indicates a patch which is not compatible with Fika and should be ignored if present
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FikaDisablePatchAttribute : Attribute
{ }

/// <summary>
/// Indicates a patch which is not compatible with questing bots and should be ignored if present
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class QBDisablePatchAttribute : Attribute
{ }