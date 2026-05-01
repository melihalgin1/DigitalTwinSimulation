using DigitalTwinSimulation.Core.Monitoring;
using System.Collections.Generic;

namespace DigitalTwinSimulation.UI.ViewModels;

public sealed class SupplementaryMonitorItemViewModel
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

    public IReadOnlyList<string> ReadyPowertrainVinTexts { get; }

    public string StatusColorHex { get; }

    public SupplementaryMonitorItemViewModel(SupplementaryMonitorSnapshot snapshot)
    {
        MonitorName = snapshot.MonitorName;
        StatusText = snapshot.Status.ToString();
        StopTypeText = snapshot.StopType.ToString();
        ReasonText = snapshot.ReasonText;

        IsActivated = snapshot.IsActivated;

        ReadyPowertrainCount = snapshot.ReadyPowertrainCount;
        ReadyPowertrainCapacity = snapshot.ReadyPowertrainCapacity;
        ReadyPowertrainUsagePercentage = snapshot.ReadyPowertrainUsagePercentage;

        NextReadyPowertrainVinText = snapshot.NextReadyPowertrainVinText;

        CurrentMarriageVinText = snapshot.CurrentMarriageVinText;
        IsCurrentMarriagePowertrainReady = snapshot.IsCurrentMarriagePowertrainReady;

        EngineDressingTotalStations = snapshot.EngineDressingTotalStations;
        EngineDressingOccupiedStations = snapshot.EngineDressingOccupiedStations;
        EngineDressingEmptyStations = snapshot.EngineDressingEmptyStations;
        EngineDressingFaultedStations = snapshot.EngineDressingFaultedStations;

        EngineDressingAverageWear = snapshot.EngineDressingAverageWear;
        EngineDressingMinimumWear = snapshot.EngineDressingMinimumWear;

        EngineDressingMovementCountLastTakt = snapshot.EngineDressingMovementCountLastTakt;

        IsOutputBlockedByReadyBuffer = snapshot.IsOutputBlockedByReadyBuffer;

        ReadyPowertrainVinTexts = snapshot.ReadyPowertrainVinTexts;

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