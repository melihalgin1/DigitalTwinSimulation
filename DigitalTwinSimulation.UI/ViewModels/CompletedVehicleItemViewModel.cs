using System;
using DigitalTwinSimulation.Core.Entities;

namespace DigitalTwinSimulation.UI.ViewModels;

public sealed class CompletedVehicleItemViewModel
{
    public string VinText { get; }
    public int CompletionTakt { get; }
    public double CompletionSimulatedMinute { get; }

    public string CompletionTimeText
    {
        get
        {
            int totalMinutes = (int)Math.Round(CompletionSimulatedMinute);
            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;

            return $"{hours:D2}:{minutes:D2}";
        }
    }

    public CompletedVehicleItemViewModel(CompletedVehicleRecord record)
    {
        VinText = record.VinText;
        CompletionTakt = record.CompletionTakt;
        CompletionSimulatedMinute = record.CompletionSimulatedMinute;
    }
}