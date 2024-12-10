using System.Collections.Generic;
using DrakiaXYZ.BigBrain.Brains;
using PerformanceImprovements.Bots.Layers;
using PerformanceImprovements.Utils;

namespace PerformanceImprovements.Bots;

internal static class LayerManager
{
    private static List<string> _brains = 
    [
        EBrain.PMC.ToString(),
        EBrain.Assault.ToString()
    ];
    
    private static List<string> _layersToRemove =
    [
        "PatrolAssault"
    ];
    
    internal static void AddLayers()
    {
        BrainManager.AddCustomLayer(typeof(SoloPatrolLayer), _brains, 80);
        
        Logger.Info($"Added {_layersToRemove.Count} layers to {_brains.Count} brains");
        
        BrainManager.RemoveLayers(_layersToRemove, _brains);
        
        Logger.Info($"Removed {_layersToRemove.Count} layers from {_brains.Count} brains");
    }
}