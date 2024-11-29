using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;

namespace PerformanceImprovements.Utils;

internal static class GameUtils
{
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