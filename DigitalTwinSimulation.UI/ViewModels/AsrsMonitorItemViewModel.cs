using DigitalTwinSimulation.Core.Monitoring;
using System.Collections.Generic;

namespace DigitalTwinSimulation.UI.ViewModels;

public sealed class AsrsMonitorItemViewModel
{
    public string MonitorName { get; }
    public string StatusText { get; }
    public string StopTypeText { get; }
    public string ReasonText { get; }

    public int Capacity { get; }
    public int StoredVehicleCount { get; }
    public double UsagePercentage { get; }

    public string NextRequiredVinText { get; }
    public bool IsNextRequiredVinPresent { get; }
    public bool IsReleaseReady { get; }

    public IReadOnlyList<string> StoredVinTexts { get; }

    public string StatusColorHex { get; }

    public AsrsMonitorItemViewModel(AsrsMonitorSnapshot snapshot)
    {
        MonitorName = snapshot.MonitorName;
        StatusText = snapshot.Status.ToString();
        StopTypeText = snapshot.StopType.ToString();
        ReasonText = snapshot.ReasonText;

        Capacity = snapshot.Capacity;
        StoredVehicleCount = snapshot.StoredVehicleCount;
        UsagePercentage = snapshot.UsagePercentage;

        NextRequiredVinText = snapshot.NextRequiredVinText;
        IsNextRequiredVinPresent = snapshot.IsNextRequiredVinPresent;
        IsReleaseReady = snapshot.IsReleaseReady;

        StoredVinTexts = snapshot.StoredVinTexts;

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