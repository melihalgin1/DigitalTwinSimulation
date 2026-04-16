namespace DigitalTwinSimulation.Core.Layout;

public sealed class StationGroupSnapshot
{
    public string GroupName { get; }
    public IReadOnlyList<StationSnapshot> Stations { get; }

    public StationGroupSnapshot(string groupName, IReadOnlyList<StationSnapshot> stations)
    {
        GroupName = groupName;
        Stations = stations;
    }
}