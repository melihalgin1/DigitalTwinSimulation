using DigitalTwinSimulation.Core.Monitoring;

namespace DigitalTwinSimulation.UI.ViewModels;

public sealed class GroupMonitorItemViewModel
{
    public string GroupName { get; }
    public string StatusText { get; }
    public string StopTypeText { get; }
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

    public string StatusColorHex { get; }

    public GroupMonitorItemViewModel(GroupMonitorSnapshot snapshot)
    {
        GroupName = snapshot.GroupName;
        StatusText = snapshot.Status.ToString();
        StopTypeText = snapshot.StopType.ToString();
        ReasonText = snapshot.ReasonText;

        TotalStations = snapshot.TotalStations;
        OccupiedStations = snapshot.OccupiedStations;
        EmptyStations = snapshot.EmptyStations;
        FaultedStations = snapshot.FaultedStations;

        EmptyPercentage = snapshot.EmptyPercentage;
        FaultedPercentage = snapshot.FaultedPercentage;
        AverageWear = snapshot.AverageWear;
        MinimumWear = snapshot.MinimumWear;

        LeadVINText = snapshot.LeadVINText;
        TailVINText = snapshot.TailVINText;

        MovementCountLastTakt = snapshot.MovementCountLastTakt;

        StatusColorHex = snapshot.Status switch
        {
            GroupMonitorStatus.Green => "#2E7D32",
            GroupMonitorStatus.Yellow => "#F9A825",
            GroupMonitorStatus.Red => "#C62828",
            GroupMonitorStatus.Neutral => "#546E7A",
            _ => "#546E7A"
        };
    }
}