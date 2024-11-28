using Comfort.Common;
using EFT.Console.Core;
using EFT.UI;

namespace PerformanceImprovements.Utils;

public static class ConsoleCommands
{
    public static void RegisterCommands()
    {
        ConsoleScreen.Processor.RegisterCommandGroup<GeneralCommandGroup>();
        ConsoleScreen.Processor.RegisterCommandGroup<ProfilerCommandGroup>();
    }
    
    private class GeneralCommandGroup
    {
        [ConsoleCommand("clear", "", null, "Clears the console", ["cls"])]
        public static void ClearConsole()
        {
            Singleton<PreloaderUI>.Instance.Console.Clear();
        }
    }
    
    private class ProfilerCommandGroup
    {
        [ConsoleCommand("set_class_to_profile", "", null, "Profiles the specified class")]
        public static void SetClassToProfile([ConsoleArgument("", "Set the type to profile")] string type)
        {
            Plugin.Profiler?.SetTypeToProfile(type);
        }
        
        [ConsoleCommand("enable_profiler", "", null, "Should the profiler run")]
        public static void EnableProfiler([ConsoleArgument(false, "Enable or Disable the profiler")] bool enable)
        {
            Plugin.Profiler?.TryEnableOrDisable(enable);
        }
        
        [ConsoleCommand("dump_analytics", "", null, "Write timings to disk")]
        public static void DumpAnalytics()
        {
            Plugin.Profiler?.DumpAnalytics();
        }
        
        [ConsoleCommand("help_profiler", "", null, "Profiler command list")]
        public static void HelpTextProfiler()
        {
            ConsoleScreen.Log("dump_analytics       : Dumps the analytics to disk");
            ConsoleScreen.Log("enable_profiler      : Enables or disables the profiler (t/f)");
            ConsoleScreen.Log("set_class_to_profile : Sets the class to profile");
        }
    }
}