using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using PerformanceImprovements.Bots.PathFinding;
using UnityEngine;

namespace PerformanceImprovements.Bots.Logics;

public class WanderLogic : CustomLogic
{
    private BotOwner _botOwner;

    private float _nextObjectiveTime;
    private List<Vector3> _currentPath;
    
    private Task<Vector3[]> _task;
    
    private WanderLogic(BotOwner owner) : base(owner)
    {
        _nextObjectiveTime = Time.time;
        _botOwner = owner;
    }

    public override void Update()
    {
        if (!NeedNewObjective()) return;

        if (NeedNewPath())
        {
            _task = Task.Run(() => PathFindingTasks.CalculatePath(_botOwner, Vector3.zero));
            Utils.Logger.Debug($"{nameof(WanderLogic)} :: Generating new path for {_botOwner.Profile.Nickname}");
        }
        
        if (!_task.IsCompleted) return;

        if (_task.Result != null)
        {
            _currentPath = _task.Result.ToList();
        }
        
        if (_botOwner.Mover.IsMoving) return;
        
        _botOwner.PatrollingData.SetTargetMoveSpeed();
        
        Utils.Logger.Debug($"{nameof(WanderLogic)} :: {_botOwner.Profile.Nickname} moving to {_currentPath.Last()}");
        _botOwner.GoToByWay(_currentPath.ToArray());
        _currentPath.Clear();
        
        _nextObjectiveTime = Time.time + 1f;
    }

    private bool NeedNewPath()
    {
        return _task.IsCompleted && _currentPath.Count == 0;
    }
    
    private bool NeedNewObjective()
    {
        return Time.time >= _nextObjectiveTime || _currentPath.Count == 0;
    }
}