using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace PerformanceImprovements.EFTProfiler;

public class AnalyticsModel
{
    public long MinTime;
    public long MaxTime;
    public long AvgTime;
    
    [JsonIgnore]
    public readonly List<long> AllTimings = [];

    public void CalculateBenchmark()
    {
        MinTime = AllTimings.Min();
        MaxTime = AllTimings.Max();
        AvgTime = AllTimings.Sum() / AllTimings.Count;
    }
}