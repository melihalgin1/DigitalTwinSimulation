using DigitalTwinSimulation.Core.Enums;

namespace DigitalTwinSimulation.Core.Layout;

public static class AssemblyLayoutDefinition
{
    public static readonly IReadOnlyList<StationGroupDefinition> MainLineGroups =
    [
        new(StationGroupType.TR1, 8),
        new(StationGroupType.TR2, 8),
        new(StationGroupType.CH1, 9),
        new(StationGroupType.CH2, 9),
        new(StationGroupType.Final, 6)
    ];

    public static readonly StationGroupDefinition EngineDressing =
        new(StationGroupType.EngineDressing, 5);
}