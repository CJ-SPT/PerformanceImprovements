using System.Linq;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using PerformanceImprovements.Bots.Logics;
using PerformanceImprovements.Utils;

namespace PerformanceImprovements.Bots.Layers;

public class SoloPatrolLayer(BotOwner botOwner, int priority) 
    : CustomLayer(botOwner, priority)
{
    public override string GetName()
    {
        return "PI_SolarPatrolLayer";
    }

    public override bool IsActive()
    {
        // Solo and not a boss
        return botOwner.BotsGroup.Allies.Count == 0 &&
               !botOwner.Boss.IamBoss;
    }

    public override Action GetNextAction()
    {
        if (botOwner.EatDrinkData.HaveActions())
        {
            Logger.Debug($"GetNextAction({GetName()}) :: {nameof(EatDrinkLogic)} :: Eating");
            return new Action(typeof(EatDrinkLogic), "EatDrink");
        }
        
        return new Action(typeof(WanderLogic), "Wander");
    }

    public override bool IsCurrentActionEnding()
    {
        var currentActionType = CurrentAction?.Type;

        if (currentActionType == typeof(EatDrinkLogic))
        {
            return EndEatDrinkLogic();
        }
        
        if (currentActionType == typeof(WanderLogic))
        {
            return EndWanderLogic();
        }
        
        // If it's not a logic we handle, end it
        return true;
    }

    private bool EndEatDrinkLogic()
    {
        return !botOwner.EatDrinkData.HaveActions();
    }

    private bool EndWanderLogic()
    {
        return botOwner.EnemiesController.EnemyInfos.Count > 0;
    }
}