using DigitalTwinSimulation.Core.Layout;

namespace DigitalTwinSimulation.UI.ViewModels;

public sealed class StationDetailItemViewModel
{
    public string StationId { get; }
    public string VinText { get; }
    public string StateText { get; }
    public string WearText { get; }

    public double WearPercentage { get; }

    public StationDetailItemViewModel(StationSnapshot snapshot)
    {
        StationId = snapshot.StationId;
        VinText = snapshot.VinText;
        StateText = snapshot.StateText;
        WearText = snapshot.WearText;

        WearPercentage = ParseWearPercentage(snapshot.WearText);
    }

    private static double ParseWearPercentage(string wearText)
    {
        string normalized = wearText.Replace("%", "").Trim();

        return double.TryParse(normalized, out double value)
            ? value
            : 0.0;
    }
}