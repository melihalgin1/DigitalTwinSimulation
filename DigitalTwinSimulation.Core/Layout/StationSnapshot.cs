namespace DigitalTwinSimulation.Core.Layout;

public sealed class StationSnapshot
{
    public string StationId { get; }
    public string VinText { get; }
    public string StateText { get; }
    public string WearText { get; }

    public StationSnapshot(
        string stationId,
        string vinText,
        string stateText,
        string wearText)
    {
        StationId = stationId;
        VinText = vinText;
        StateText = stateText;
        WearText = wearText;
    }
}