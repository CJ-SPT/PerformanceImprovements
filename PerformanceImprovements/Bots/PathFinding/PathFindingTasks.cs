using System.Threading.Tasks;
using EFT;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

namespace PerformanceImprovements.Bots.PathFinding;

public static class PathFindingTasks
{
    [ItemCanBeNull]
    public static Task<Vector3[]> CalculatePath(BotOwner bot, Vector3 to)
    {
        var path = new NavMeshPath();

        if (NavMesh.CalculatePath(bot.Transform.position, to, -1, path))
        {
            return Task.FromResult(path.corners);
        }

        if (NavMesh.SamplePosition(bot.Transform.position, out var hit, 7.0f, -1))
        {
            return Task.FromResult(path.corners);
        }
        
        return null;
    }

    public static void GoToByWay(this BotOwner bot, Vector3[] waypoints)
    {
        bot.Mover.GoToByWay(waypoints, 5f);
    }
}