using DigitalTwinSimulation.Core.Layout;
using System.Collections.ObjectModel;

namespace DigitalTwinSimulation.UI.ViewModels;

public sealed class SupplementaryDetailWindowViewModel
{
    public string MonitorName { get; }
    public string StatusText { get; }
    public string StopTypeText { get; }
    public string ReasonText { get; }

    public bool IsActivated { get; }

    public int ReadyPowertrainCount { get; }
    public int ReadyPowertrainCapacity { get; }
    public double ReadyPowertrainUsagePercentage { get; }

    public string NextReadyPowertrainVinText { get; }

    public string CurrentMarriageVinText { get; }
    public bool IsCurrentMarriagePowertrainReady { get; }

    public int EngineDressingTotalStations { get; }
    public int EngineDressingOccupiedStations { get; }
    public int EngineDressingEmptyStations { get; }
    public int EngineDressingFaultedStations { get; }

    public double EngineDressingAverageWear { get; }
    public double EngineDressingMinimumWear { get; }

    public int EngineDressingMovementCountLastTakt { get; }

    public bool IsOutputBlockedByReadyBuffer { get; }

    public string StatusColorHex { get; }

    public ObservableCollection<string> ReadyPowertrainVinTexts { get; } = new();

    public ObservableCollection<StationDetailItemViewModel> EngineDressingStations { get; } = new();

    public SupplementaryDetailWindowViewModel(
        SupplementaryMonitorItemViewModel monitor,
        StationGroupSnapshot engineDressingGroupSnapshot)
    {
        MonitorName = monitor.MonitorName;
        StatusText = monitor.StatusText;
        StopTypeText = monitor.StopTypeText;
        ReasonText = monitor.ReasonText;

        IsActivated = monitor.IsActivated;

        ReadyPowertrainCount = monitor.ReadyPowertrainCount;
        ReadyPowertrainCapacity = monitor.ReadyPowertrainCapacity;
        ReadyPowertrainUsagePercentage = monitor.ReadyPowertrainUsagePercentage;

        NextReadyPowertrainVinText = monitor.NextReadyPowertrainVinText;

        CurrentMarriageVinText = monitor.CurrentMarriageVinText;
        IsCurrentMarriagePowertrainReady = monitor.IsCurrentMarriagePowertrainReady;

        EngineDressingTotalStations = monitor.EngineDressingTotalStations;
        EngineDressingOccupiedStations = monitor.EngineDressingOccupiedStations;
        EngineDressingEmptyStations = monitor.EngineDressingEmptyStations;
        EngineDressingFaultedStations = monitor.EngineDressingFaultedStations;

        EngineDressingAverageWear = monitor.EngineDressingAverageWear;
        EngineDressingMinimumWear = monitor.EngineDressingMinimumWear;

        EngineDressingMovementCountLastTakt = monitor.EngineDressingMovementCountLastTakt;

        IsOutputBlockedByReadyBuffer = monitor.IsOutputBlockedByReadyBuffer;

        StatusColorHex = monitor.StatusColorHex;

        foreach (var vinText in monitor.ReadyPowertrainVinTexts)
        {
            ReadyPowertrainVinTexts.Add(vinText);
        }

        foreach (var station in engineDressingGroupSnapshot.Stations)
        {
            EngineDressingStations.Add(new StationDetailItemViewModel(station));
        }
    }
}