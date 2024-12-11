using DrakiaXYZ.BigBrain.Brains;
using EFT;

namespace PerformanceImprovements.Bots.Logics;

public class EatDrinkLogic : CustomLogic
{
    private BotEatDrinkData _data;
    
    private EatDrinkLogic(BotOwner owner) : base(owner)
    {
        _data = new(owner);
    }

    public override void Update()
    {
        _data.ManualUpdate();
    }
}