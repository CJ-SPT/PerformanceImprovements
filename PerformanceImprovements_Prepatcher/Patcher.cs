using Mono.Cecil;
using System.Collections.Generic;
using System;
using BepInEx.Logging;
using System.Diagnostics;
using System.Linq;
using Mono.Collections.Generic;
using FieldAttributes = Mono.Cecil.FieldAttributes;

public static class PerformancePrepatcher
{
    public static IEnumerable<string> TargetDLLs { get; } = new string[] { "Assembly-CSharp.dll" };

    public static void Patch(ref AssemblyDefinition assembly)
    {
        try
        {
            PatchNewSampleModes(ref assembly);

            Logger.CreateLogSource("Performance Patch").LogInfo("Patching Complete!");
        }
        catch (Exception ex)
        {
            // Get stack trace for the exception with source file information
            var st = new StackTrace(ex, true);
            // Get the top stack frame
            var frame = st.GetFrame(0);
            // Get the line number from the stack frame
            var line = frame.GetFileLineNumber();

            Logger.CreateLogSource("Performance Patch")
                .LogError("Error When Patching: " + ex.Message + " - Line " + line);
        }
    }

    private static FieldDefinition CreateNewEnum(string fieldConstName, TypeDefinition EnumClass, int CustomConstant)
    {
        var newEnum = new FieldDefinition(
                fieldConstName,
                FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault,
                EnumClass)
            { Constant = CustomConstant };

        return newEnum;
    }

    private static void PatchNewSampleModes(ref AssemblyDefinition assembly)
    {
        var sampleEnum = assembly.MainModule.GetType("EFT.Settings.Graphics.ESamplingMode");

        var index = 0;
        
        foreach (var field in sampleEnum.Fields.ToArray())
        {
            if (!field.IsStatic) continue;
            
            sampleEnum.Fields.Remove(field);
        }
        
        var off = CreateNewEnum(
            "Off",
            sampleEnum,
            index++);
        
        var down50 = CreateNewEnum(
            "DownX05",
            sampleEnum,
            index++);
        
        var down60 = CreateNewEnum(
            "DownX06",
            sampleEnum,
            index++);

        var down70 = CreateNewEnum(
            "DownX07",
            sampleEnum,
            index++);
        
        var down75 = CreateNewEnum(
            "DownX075",
            sampleEnum,
            index++);

        var down80 = CreateNewEnum(
            "DownX08",
            sampleEnum,
            index++);

        var down90 = CreateNewEnum(
            "DownX09",
            sampleEnum,
            index++);
        
        var super2 = CreateNewEnum(
            "SuperX2",
            sampleEnum,
            index++);
        
        var super4 = CreateNewEnum(
            "SuperX4",
            sampleEnum,
            index++);
        
        sampleEnum.Fields.Add(off);
        sampleEnum.Fields.Add(down50);
        sampleEnum.Fields.Add(down60);
        sampleEnum.Fields.Add(down70);
        sampleEnum.Fields.Add(down75);
        sampleEnum.Fields.Add(down80);
        sampleEnum.Fields.Add(down90);
        
        sampleEnum.Fields.Add(super2);
        sampleEnum.Fields.Add(super4);
    }
}