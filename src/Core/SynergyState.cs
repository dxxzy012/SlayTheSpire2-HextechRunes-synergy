namespace HextechRunes.Core;

public sealed class SynergyState
{
    public SynergyDefinition Definition { get; }
    public int CurrentCount { get; internal set; }

    public SynergyStatus Status =>
        CurrentCount >= MinThreshold ? SynergyStatus.Activated :
        CurrentCount > 0 ? SynergyStatus.Progress : SynergyStatus.Inactive;

    public int MinThreshold { get; }
    public int MaxThreshold { get; }

    public SynergyColor Color => Status switch
    {
        SynergyStatus.Activated => SynergyColor.Red,
        SynergyStatus.Progress => SynergyColor.Yellow,
        _ => SynergyColor.Green
    };

    public SynergyState(SynergyDefinition definition)
    {
        Definition = definition;
        CurrentCount = 0;

        int min = int.MaxValue;
        int max = 0;
        foreach (var k in Definition.ThresholdDescriptions.Keys)
        {
            if (k < min) min = k;
            if (k > max) max = k;
        }
        MinThreshold = min == int.MaxValue ? 0 : min;
        MaxThreshold = max;
    }
}
