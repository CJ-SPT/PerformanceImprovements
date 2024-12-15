using System.Diagnostics;
using BepInEx.Logging;

namespace PerformanceImprovements.Utils;

internal static class Logger
{
    private static ManualLogSource _logger => Plugin.Log;

#if DEBUG
    private static bool _debug = true;
#else
    private static bool _debug = false;
#endif
    
    public static void Debug(string message)
    {
        if (!_debug) return;
        
        _logger.LogDebug($"{GetCallersName()} :: {message}");
    }
    
    public static void Info(string message)
    {
        _logger.LogInfo(BuildMessage(message));
    }
    
    public static void Warn(string message)
    {
        _logger.LogWarning(BuildMessage(message));
    }
    
    public static void Error(string message)
    {
        _logger.LogError(BuildMessage(message));
    }
    
    public static void Fatal(string message)
    {
        _logger.LogFatal(BuildMessage(message));
    }

    private static string BuildMessage(string message)
    {
        message = _debug 
            ? $"{GetCallersName()} :: {message}"
            : message;
        
        return message;
    }
    
    private static string GetCallersName()
    {
        var stackTrace = new StackTrace();
        var methodBase = stackTrace.GetFrame(3).GetMethod();
        return $"{methodBase.DeclaringType?.Name}::{methodBase.Name}()"; 
    }
}