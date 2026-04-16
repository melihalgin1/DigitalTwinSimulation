using DigitalTwinSimulation.Core.Entities;
using DigitalTwinSimulation.Core.Layout;

namespace DigitalTwinSimulation.Core.Factories;

public static class AssemblyFactory
{
    public static IReadOnlyList<StationGroup> CreateMainLine()
    {
        var groups = new List<StationGroup>();

        foreach (var definition in AssemblyLayoutDefinition.MainLineGroups)
        {
            var stations = new List<Station>();

            for (int i = 1; i <= definition.StationCount;  i++)
            {
                var stationId = $"{definition.GroupType}-S{i:D2}";
                stations.Add(new Station(stationId, definition.GroupType));
            }

            groups.Add(new StationGroup(definition.GroupType, stations));
        }

        return groups;
    }

    public static StationGroup CreateEngineDressing()
    {
        var definition = AssemblyLayoutDefinition.EngineDressing;
        var stations = new List<Station>();

        for(int i = 1;i <= definition.StationCount;i++)
        {
            var stationId = $"{definition.GroupType}-S{i:D2}";
            stations.Add(new Station(stationId,definition.GroupType));
        }

        return new StationGroup(definition.GroupType, stations);
    }
}