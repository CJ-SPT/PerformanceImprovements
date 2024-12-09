using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using PerformanceImprovements.Models;

namespace PerformanceImprovements.Utils;

public static class EmbededResourceUtil
{
    public static CleanUpNameModel GetCleanUpNamesJson()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string path = "PerformanceImprovements.json.CleanUpNames.json";

        using var stream = assembly.GetManifestResourceStream(path);
        using var reader = new StreamReader(stream!);
        
        var json = reader.ReadToEnd();
        
        return JsonConvert.DeserializeObject<CleanUpNameModel>(json);
    }

    public static Dictionary<string, string> GetEmbededLocalizationJson()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string path = "PerformanceImprovements.json.Localization.json";
        
        using var stream = assembly.GetManifestResourceStream(path);
        using var reader = new StreamReader(stream!);
        
        var json = reader.ReadToEnd();
        
        return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
    }
}