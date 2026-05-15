using System;
using System.Collections.Generic;

namespace HextechRunes.Core;

public sealed class SynergyDefinition
{
    public string Id { get; }
    public string Name { get; }
    public IReadOnlyList<Type> RuneTypes { get; }
    public Dictionary<int, string> ThresholdDescriptions { get; }

    public SynergyDefinition(string id, string name, List<Type> runeTypes, Dictionary<int, string> thresholds)
    {
        Id = id;
        Name = name;
        RuneTypes = runeTypes.AsReadOnly();
        ThresholdDescriptions = thresholds;
    }
}
