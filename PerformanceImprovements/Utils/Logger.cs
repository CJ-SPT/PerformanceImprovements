using BepInEx.Logging;

namespace PerformanceImprovements.Utils;

public class Logger
{
    private static ManualLogSource _logger;

#if DEBUG
    private static bool _debug = false;
#else
    private static bool _debug = true;
#endif
    
    static Logger()
    {
        _logger = new ManualLogSource("PI Logger");
    }

    public static void Debug(string message)
    {
        if (!_debug) return;
        
        _logger.LogDebug(message);
    }
    
    public static void Info(string message)
    {
        _logger.LogInfo(message);
    }
    
    public static void Warn(string message)
    {
        _logger.LogWarning(message);
    }
    
    public static void Error(string message)
    {
        _logger.LogError(message);
    }
    
    public static void Fatal(string message)
    {
        _logger.LogFatal(message);
    }
}