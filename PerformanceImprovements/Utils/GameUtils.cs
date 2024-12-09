using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;

namespace PerformanceImprovements.Utils;

internal static class GameUtils
{
    public static readonly HashSet<WildSpawnType> Bosses = [
        WildSpawnType.bossBoar,             // Kaban
        WildSpawnType.bossBully,            // Reshala
        WildSpawnType.bossGluhar,           // Glukhar
        WildSpawnType.bossKilla,
        WildSpawnType.bossKnight,
        WildSpawnType.followerBigPipe,
        WildSpawnType.followerBirdEye,
        WildSpawnType.bossKolontay,
        WildSpawnType.bossKojaniy,          // Shturman
        WildSpawnType.bossPartisan,
        WildSpawnType.bossSanitar,
        WildSpawnType.bossTagilla,
        WildSpawnType.bossZryachiy,
        WildSpawnType.gifter,               // Santa
        WildSpawnType.arenaFighterEvent,    // Blood Hounds
        WildSpawnType.sectantPriest,        // Cultist Priest
        (WildSpawnType) 199,                // Legion
        (WildSpawnType) 801,                // Punisher
    ];
    
    public static bool IsInRaid()
    {
        return Singleton<IBotGame>.Instantiated;
    }
    
    public static GameWorld GetGameWorld()
    {
        return Singleton<GameWorld>.Instance;
    }
    
    public static Player GetMainPlayer()
    {
        return GetGameWorld().MainPlayer;
    }

    public static CameraClass GetCameraClass()
    {
        return CameraClass.Instance;
    }
    
    public static bool IsInHideout()
    {
        return GetGameWorld() is HideoutGameWorld;
    }
    
    public static IBotGame GetBotGame()
    {
        return Singleton<IBotGame>.Instance;
    }

    public static string GetLocation()
    {
        return GetMainPlayer().Location;
    }
}