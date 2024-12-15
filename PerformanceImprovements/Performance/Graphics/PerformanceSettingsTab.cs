using System.Threading.Tasks;
using EFT.UI.Settings;

namespace PerformanceImprovements.Performance.Graphics;

public class PerformanceSettingsTab : SettingsTab
{
    public override Task TakeSettingsFrom(SharedGameSettingsClass settingsManager)
    {
        return Task.CompletedTask;
    }
}