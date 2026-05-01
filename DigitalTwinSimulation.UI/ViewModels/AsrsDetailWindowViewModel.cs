using System.Collections.ObjectModel;

namespace DigitalTwinSimulation.UI.ViewModels;

public sealed class AsrsDetailWindowViewModel
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

    public string StatusColorHex { get; }

    public ObservableCollection<string> StoredVinTexts { get; } = new();

    public AsrsDetailWindowViewModel(AsrsMonitorItemViewModel monitor)
    {
        MonitorName = monitor.MonitorName;
        StatusText = monitor.StatusText;
        StopTypeText = monitor.StopTypeText;
        ReasonText = monitor.ReasonText;

        Capacity = monitor.Capacity;
        StoredVehicleCount = monitor.StoredVehicleCount;
        UsagePercentage = monitor.UsagePercentage;

        NextRequiredVinText = monitor.NextRequiredVinText;
        IsNextRequiredVinPresent = monitor.IsNextRequiredVinPresent;
        IsReleaseReady = monitor.IsReleaseReady;

        StatusColorHex = monitor.StatusColorHex;

        foreach (var vinText in monitor.StoredVinTexts)
        {
            StoredVinTexts.Add(vinText);
        }
    }
}