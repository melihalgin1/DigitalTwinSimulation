using DigitalTwinSimulation.Core.Layout;
using DigitalTwinSimulation.Core.Monitoring;
using System.Collections.ObjectModel;

namespace DigitalTwinSimulation.UI.ViewModels;

public sealed class StationGroupDetailWindowViewModel
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

    public bool IsFinalGroup => GroupName == "Final";

    public bool ShowCompletedVehiclesButton => IsFinalGroup;

    public string CompletedVehiclesButtonText => "Show Completed Vehicles";

    public ObservableCollection<StationDetailItemViewModel> Stations { get; } = new();

    public StationGroupDetailWindowViewModel(
        GroupMonitorSnapshot monitorSnapshot,
        StationGroupSnapshot groupSnapshot)
    {
        GroupName = monitorSnapshot.GroupName;
        StatusText = monitorSnapshot.Status.ToString();
        StopTypeText = monitorSnapshot.StopType.ToString();
        ReasonText = monitorSnapshot.ReasonText;

        TotalStations = monitorSnapshot.TotalStations;
        OccupiedStations = monitorSnapshot.OccupiedStations;
        EmptyStations = monitorSnapshot.EmptyStations;
        FaultedStations = monitorSnapshot.FaultedStations;

        EmptyPercentage = monitorSnapshot.EmptyPercentage;
        FaultedPercentage = monitorSnapshot.FaultedPercentage;
        AverageWear = monitorSnapshot.AverageWear;
        MinimumWear = monitorSnapshot.MinimumWear;

        LeadVINText = monitorSnapshot.LeadVINText;
        TailVINText = monitorSnapshot.TailVINText;

        MovementCountLastTakt = monitorSnapshot.MovementCountLastTakt;

        StatusColorHex = monitorSnapshot.Status switch
        {
            GroupMonitorStatus.Green => "#2E7D32",
            GroupMonitorStatus.Yellow => "#F9A825",
            GroupMonitorStatus.Red => "#C62828",
            GroupMonitorStatus.Neutral => "#546E7A",
            _ => "#546E7A"
        };

        foreach (var station in groupSnapshot.Stations)
        {
            Stations.Add(new StationDetailItemViewModel(station));
        }
    }
}