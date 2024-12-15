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
        
        return Settings.UseExperimentalPatches.Value
            ? patches
            : patches.Where(t => t.GetCustomAttribute(typeof(ExperimentalPatchAttribute)) == null);
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DisablePatchAttribute : Attribute
{ }

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ExperimentalPatchAttribute : Attribute
{ }

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FikaDisablePatchAttribute : Attribute
{ }