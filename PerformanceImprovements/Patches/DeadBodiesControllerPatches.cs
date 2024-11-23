﻿using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine.AI;

namespace PerformanceImprovements.Patches;

public class DeadBodiesControllerAddBody : ModulePatch
{
    private static readonly FieldInfo GroupsField = AccessTools.Field(typeof(DeadBodiesController), "_groups");
    private static readonly float DistNotToGroupSqr = GClass583.Core.DIST_NOT_TO_GROUP_SQR;
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(DeadBodiesController), nameof(DeadBodiesController.AddBody),
            new[] { typeof(IPlayer) });
    }

    [PatchPrefix]
    public static bool PatchPrefix(DeadBodiesController __instance, IPlayer player)
    {
        UniTask.RunOnThreadPool(() => AddBody(__instance, player));
        return false;
    }

    private static UniTask AddBody(DeadBodiesController deadBodiesController, IPlayer player)
    {
        if (deadBodiesController.HaveBody(player)) return UniTask.CompletedTask;

        var groupList = new List<BotsGroup>();
        
        foreach (var group in (BotZoneGroupsDictionary)GroupsField.GetValue(deadBodiesController))
        {
            var points = group.Key.PatrolWays[0].Points;

            foreach (var point in points)
            {
                if ((point.position - player.Transform.position).sqrMagnitude < DistNotToGroupSqr)
                {
                    groupList.AddRange(group.Value.GetGroups(true));
                }
            }
        }

        if (groupList.Count == 0) return UniTask.CompletedTask;

        BotsGroup botsGroup = null;
        
        // TODO: Fix this dumb retarded shit, like why.
        
        foreach (var botGroup in groupList)
        {
            var navMeshPath = new NavMeshPath();

            if (NavMesh.CalculatePath(
                    botGroup.BotZone.PatrolWays[0].Points[0].position, 
                    player.Transform.position, 
                    -1,
                    navMeshPath))
            {
                botsGroup = botGroup;
            }
        }

        if (botsGroup is null) return UniTask.CompletedTask;
        
        var gclass = new GClass361(
            botsGroup, 
            groupList, 
            player.AIData.IsAI, 
            player.Side, 
            player.Transform.position,
            player);

        foreach (var botGroup3 in groupList)
        {
            deadBodiesController.AddBody(botGroup3, gclass);
        }
        
        deadBodiesController.Bodies.Add(gclass);
        
        return UniTask.CompletedTask;
    }
}