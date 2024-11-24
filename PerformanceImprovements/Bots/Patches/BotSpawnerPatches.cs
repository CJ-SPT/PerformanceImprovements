using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using Cysharp.Threading.Tasks;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace PerformanceImprovements.Bots.Patches;

public class ActivateBotPatch : ModulePatch
{
    private static readonly List<BotOwner> BotOwnersToActivate = [];
    private static bool _isSpawning = false;
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(
            typeof(GClass888),
            nameof(GClass888.ActivateBot),
            new []{ typeof(BotCreationDataClass), typeof(BotZone), 
                typeof(bool), typeof(Func<BotOwner, BotZone, BotsGroup>),
                typeof(Action<BotOwner>), typeof(CancellationToken) });
    }

    [PatchPrefix]
    public static bool PatchPrefix(
        GClass888 __instance,
        BotCreationDataClass data,
        BotZone zone,
        bool shallBeGroup,
        Func<BotOwner, BotZone, BotsGroup> groupAction,
        Action<BotOwner> callback,
        CancellationToken cancellationToken,
        Func<GameWorld, Profile, Vector3, Task<LocalPlayer>> ___func_0,
        IBotGame ___ibotGame_0)
    {
        var action = new GClass888.Class564()
        {
            data = data,
            gclass888_0 = __instance,
            zone = zone,
            callback = callback,
            groupAction = groupAction
        };

        BotOwnersToActivate.Clear();
        _isSpawning = true;
        
        //UniTask.RunOnThreadPool(() => TrySpawnBots(data, shallBeGroup, cancellationToken, ___func_0, ___ibotGame_0));
        //UniTask.WaitUntil(() => !_isSpawning);

        TrySpawnBots(data, shallBeGroup, cancellationToken, ___func_0, ___ibotGame_0);
        
        ActivateBots(__instance, action.method_0);
        
        return false;
    }

    private static async UniTaskVoid TrySpawnBots(
        BotCreationDataClass data,
        bool shallBeGroup,
        CancellationToken cancellationToken,
        Func<GameWorld, Profile, Vector3, Task<LocalPlayer>> func,
        IBotGame botGame)
    {
        foreach (var profile in data.Profiles)
        {
            if (cancellationToken.IsCancellationRequested || data.SpawnStopped) break;
            
            if (profile is null) continue;
            
            if (shallBeGroup)
            {
                profile.Info.Settings.TryChangeRoleToAssaultGroup();
            }

            var position = data.GetPosition();
            if (position is null) continue;

            await TrySpawnBot(profile, position, cancellationToken, func, botGame);
        }

        _isSpawning = false;
        await UniTask.CompletedTask;
    }

    private static async UniTask TrySpawnBot(
        Profile profile, 
        GClass649 bornInfo, 
        CancellationToken cancellationToken,
        Func<GameWorld, Profile, Vector3, Task<LocalPlayer>> func,
        IBotGame botGame)
    {
        var localPlayer = await func(Singleton<GameWorld>.Instance, profile, bornInfo.position);

        if (localPlayer is null || cancellationToken.IsCancellationRequested) return;

        var corePoint = botGame.BotsController.CoversData.AICorePointsHolder.GetCorePoint(bornInfo.CorePointId);

        var botOwner = BotOwner.Create(
            localPlayer, 
            null, 
            botGame.GameDateTime, 
            botGame.BotsController, 
            true,
            corePoint);
        
        BotOwnersToActivate.Add(botOwner);
    }
    
    /// <summary>
    /// These must be done from the main thread
    /// </summary>
    private static void ActivateBots(GClass888 instance, Action<BotOwner> callback)
    {
        foreach (var bot in BotOwnersToActivate)
        {
            instance.method_4(bot.GetPlayer);
            instance.method_5(bot, false);
            bot.GetPlayer.CharacterController.isEnabled = false;
            callback(bot);
        }
    }
}