using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace PerformanceImprovements.EFTProfiler;

public class AnalyticsModel
{
    public bool IsMainThread;
    public double MinTime;
    public double MaxTime;
    public double AvgTime;
    
    [JsonIgnore]
    public readonly List<double> AllTimings = [];

    public void CalculateBenchmark()
    {
        MinTime = AllTimings.Min();
        MaxTime = AllTimings.Max();
        AvgTime = AllTimings.Sum() / AllTimings.Count;
    }
}