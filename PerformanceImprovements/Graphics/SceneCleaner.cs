using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EFT.Interactive;
using PerformanceImprovements.Config;
using PerformanceImprovements.Models;
using PerformanceImprovements.Utils;
using UnityEngine;

namespace PerformanceImprovements.Core;

public class SceneCleaner : MonoBehaviour
{
    private static readonly List<GameObject> AllSceneObjects = [];
    private static bool _cleaned;
    private static CleanUpNameModel _cleanUpNameModel;
    
    private void Awake()
    {
        _cleanUpNameModel = Settings.GetCleanUpNamesJson();
    }
    
    private void Update()
    {
        if (!GameUtils.IsInRaid())
        {
            // We're not in raid, reset the objects
            if (AllSceneObjects.Count > 0)
            {
                AllSceneObjects.Clear();
            }

            return;
        }
        
        if (AllSceneObjects.Count == 0)
        {
            GetAllSceneObjects(GetCombinedObjectNames());
        }
        
        switch (_cleaned)
        {
            // Scene is not cleaned
            case false:
                // Cleaner is enabled
                if (Settings.EnableSceneCleaner.Value)
                {
                    Plugin.Log!.LogError("Disabling SceneObjects");

                    StartCoroutine(IterateGameObjects(false));

                    _cleaned = true;
                }
                break;
            
            // Scene has been cleaned
            case true:
                // Cleaner is not enabled
                if (!Settings.EnableSceneCleaner.Value)
                {
                    Plugin.Log!.LogError("Enabling SceneObjects");
                    
                    StartCoroutine(IterateGameObjects(true));
                    
                    _cleaned = false;
                }
                break;
        }
    }
    
    private IEnumerator IterateGameObjects(bool setActive)
    {
        Plugin.Log!.LogInfo($"Iterating scene objects: {AllSceneObjects.Count}");
        
        foreach (var obj in AllSceneObjects.ToArray())
        {
            if (obj == null)
            {
                AllSceneObjects.Remove(obj);
                continue;
            }
            
            // Object is already in our state
            if (obj.activeSelf == setActive) continue;
            
            obj.SetActive(setActive);
            
            yield return null;
        }
    }
    
    private static void GetAllSceneObjects(List<string> objectNames)
    {
        var objects =  FindObjectsOfType<GameObject>()
            .Where(g => ShouldCollect(g, objectNames)).ToList();
        
        AllSceneObjects.AddRange(objects);
    }

    private static bool ShouldCollect(GameObject obj, List<string> objectNames)
    {
        // Skip special cases
        if (obj.GetComponent<ObservedLootItem>() is not null) return false;
        if (obj.GetComponent<LootableContainer>() is not null) return false;
        if (obj.GetComponent<WorldInteractiveObject>() is not null) return false;
        
        var collect = objectNames.Any(s => obj.name.ToLower().Contains(s));
        
        if (collect) Plugin.Log?.LogInfo($"Collecting object: {obj.name}");
        
        return collect;
    }
    
    private static List<string> GetCombinedObjectNames()
    {
        var tmp = new List<string>();
        
        if (Settings.DisableGarbage.Value) 
            tmp.AddRange(_cleanUpNameModel.Garbage);
        
        if (Settings.DisableHeaps.Value) 
            tmp.AddRange(_cleanUpNameModel.Heaps);
        
        if (Settings.DisableSpentCartridges.Value) 
            tmp.AddRange(_cleanUpNameModel.SpentCartridges);
        
        if (Settings.DisableFoodDrink.Value) 
            tmp.AddRange(_cleanUpNameModel.FoodDrink);
        
        if (Settings.DisableDecals.Value)
            tmp.AddRange(_cleanUpNameModel.Decals);
        
        if (Settings.DisablePuddles.Value)
            tmp.AddRange(_cleanUpNameModel.Puddles);
        
        if (Settings.DisableShards.Value)
            tmp.AddRange(_cleanUpNameModel.Shards);

        return tmp;
    }
}