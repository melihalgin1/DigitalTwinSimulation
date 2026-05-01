namespace DigitalTwinSimulation.Core.Monitoring;

public sealed class GroupMonitorSnapshot
{
    public string GroupName { get; }
    public GroupMonitorStatus Status { get; }
    public GroupStopType StopType { get; }
    public string ReasonText { get; }

    public int TotalStations { get; }
    public int OccupiedStations { get; }
    public int EmptyStations { get; }
    public int FaultedStations { get; }

    public double EmptyPercentage { get; }
    public double FaultedPercentage { get; }
    public double AverageWear { get; }
    public double MinimumWear { get; }

    public string LeadVINText { get; }
    public string TailVINText { get; }

    public int MovementCountLastTakt { get; }

    public GroupMonitorSnapshot(
        string groupName,
        GroupMonitorStatus status,
        GroupStopType stopType,
        string reasonText,
        int totalStations,
        int occupiedStations,
        int emptyStations,
        int faultedStations,
        double emptyPercentage,
        double faultedPercentage,
        double averageWear,
        double minimumWear,
        string leadVINText,
        string tailVINText,
        int movementCountLastTakt)
    {
        GroupName = groupName;
        Status = status;
        StopType = stopType;
        ReasonText = reasonText;
        TotalStations = totalStations;
        OccupiedStations = occupiedStations;
        EmptyStations = emptyStations;
        FaultedStations = faultedStations;
        EmptyPercentage = emptyPercentage;
        FaultedPercentage = faultedPercentage;
        AverageWear = averageWear;
        MinimumWear = minimumWear;
        LeadVINText = leadVINText;
        TailVINText = tailVINText;
        MovementCountLastTakt = movementCountLastTakt;
    }
}