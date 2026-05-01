namespace DigitalTwinSimulation.Core.Monitoring;

public sealed class SupplementaryMonitorSnapshot
{
    public string MonitorName { get; }

    public GroupMonitorStatus Status { get; }
    public GroupStopType StopType { get; }
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

    public SupplementaryMonitorSnapshot(
        string monitorName,
        GroupMonitorStatus status,
        GroupStopType stopType,
        string reasonText,
        bool isActivated,
        int readyPowertrainCount,
        int readyPowertrainCapacity,
        double readyPowertrainUsagePercentage,
        string nextReadyPowertrainVinText,
        string currentMarriageVinText,
        bool isCurrentMarriagePowertrainReady,
        int engineDressingTotalStations,
        int engineDressingOccupiedStations,
        int engineDressingEmptyStations,
        int engineDressingFaultedStations,
        double engineDressingAverageWear,
        double engineDressingMinimumWear,
        int engineDressingMovementCountLastTakt,
        bool isOutputBlockedByReadyBuffer,
        IReadOnlyList<string> readyPowertrainVinTexts)
    {
        MonitorName = monitorName;
        Status = status;
        StopType = stopType;
        ReasonText = reasonText;
        IsActivated = isActivated;
        ReadyPowertrainCount = readyPowertrainCount;
        ReadyPowertrainCapacity = readyPowertrainCapacity;
        ReadyPowertrainUsagePercentage = readyPowertrainUsagePercentage;
        NextReadyPowertrainVinText = nextReadyPowertrainVinText;
        CurrentMarriageVinText = currentMarriageVinText;
        IsCurrentMarriagePowertrainReady = isCurrentMarriagePowertrainReady;
        EngineDressingTotalStations = engineDressingTotalStations;
        EngineDressingOccupiedStations = engineDressingOccupiedStations;
        EngineDressingEmptyStations = engineDressingEmptyStations;
        EngineDressingFaultedStations = engineDressingFaultedStations;
        EngineDressingAverageWear = engineDressingAverageWear;
        EngineDressingMinimumWear = engineDressingMinimumWear;
        EngineDressingMovementCountLastTakt = engineDressingMovementCountLastTakt;
        IsOutputBlockedByReadyBuffer = isOutputBlockedByReadyBuffer;
        ReadyPowertrainVinTexts = readyPowertrainVinTexts;
    }
}