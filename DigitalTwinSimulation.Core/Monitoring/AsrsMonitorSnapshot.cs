namespace DigitalTwinSimulation.Core.Monitoring;

public sealed class AsrsMonitorSnapshot
{
    public string MonitorName { get; }

    public GroupMonitorStatus Status { get; }
    public GroupStopType StopType { get; }
    public string ReasonText { get; }

    public int Capacity { get; }
    public int StoredVehicleCount { get; }
    public double UsagePercentage { get; }

    public string NextRequiredVinText { get; }
    public bool IsNextRequiredVinPresent { get; }
    public bool IsReleaseReady { get; }

    public IReadOnlyList<string> StoredVinTexts { get; }

    public AsrsMonitorSnapshot(
        string monitorName,
        GroupMonitorStatus status,
        GroupStopType stopType,
        string reasonText,
        int capacity,
        int storedVehicleCount,
        double usagePercentage,
        string nextRequiredVinText,
        bool isNextRequiredVinPresent,
        bool isReleaseReady,
        IReadOnlyList<string> storedVinTexts)
    {
        MonitorName = monitorName;
        Status = status;
        StopType = stopType;
        ReasonText = reasonText;
        Capacity = capacity;
        StoredVehicleCount = storedVehicleCount;
        UsagePercentage = usagePercentage;
        NextRequiredVinText = nextRequiredVinText;
        IsNextRequiredVinPresent = isNextRequiredVinPresent;
        IsReleaseReady = isReleaseReady;
        StoredVinTexts = storedVinTexts;
    }
}