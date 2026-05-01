using DigitalTwinSimulation.Core.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DigitalTwinSimulation.UI.ViewModels;

public sealed class CompletedVehiclesWindowViewModel
{
    public string Title { get; }
    public string WindowDescription { get; }
    public string SnapshotTimeText { get; }

    public int VehicleCount => CompletedVehicles.Count;

    public ObservableCollection<CompletedVehicleItemViewModel> CompletedVehicles { get; } = new();

    public CompletedVehiclesWindowViewModel(
        IReadOnlyList<CompletedVehicleRecord> completedVehicleRecords,
        double windowMinutes,
        double snapshotSimulatedMinute)
    {
        Title = "Completed Vehicles";
        WindowDescription = $"Vehicles completed in the last {windowMinutes:0} simulated minutes";
        SnapshotTimeText = $"Snapshot taken at simulated time: {FormatSimulatedTime(snapshotSimulatedMinute)}";

        foreach (var record in completedVehicleRecords)
        {
            CompletedVehicles.Add(new CompletedVehicleItemViewModel(record));
        }
    }

    private static string FormatSimulatedTime(double simulatedMinute)
    {
        int totalMinutes = (int)Math.Round(simulatedMinute);
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;

        return $"{hours:D2}:{minutes:D2}";
    }
}