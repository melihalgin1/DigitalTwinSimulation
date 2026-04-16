using DigitalTwinSimulation.Core.Enums;

namespace DigitalTwinSimulation.Core.Layout;

public sealed class StationGroupDefinition
{
    public StationGroupType GroupType { get; }
    public int StationCount { get; }

    public StationGroupDefinition(StationGroupType groupType, int stationCount)
    {
        GroupType = groupType;
        StationCount = stationCount;
    }
}