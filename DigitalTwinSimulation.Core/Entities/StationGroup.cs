using DigitalTwinSimulation.Core.Enums;

namespace DigitalTwinSimulation.Core.Entities;

public sealed class StationGroup
{
    public StationGroupType GroupType { get; }
    public IReadOnlyList<Station> Stations { get; }

    public StationGroup(StationGroupType groupType, IReadOnlyList<Station> stations)
    {  
        GroupType = groupType; 
        Stations = stations; 
    }
}