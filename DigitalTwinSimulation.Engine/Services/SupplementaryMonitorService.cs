using DigitalTwinSimulation.Core.Entities;
using DigitalTwinSimulation.Core.Monitoring;

namespace DigitalTwinSimulation.Engine.Services;

public sealed class SupplementaryMonitorService
{
    private const int LowReadyBufferThreshold = 2;

    public SupplementaryMonitorSnapshot BuildSnapshot(
        SupplementaryPowertrainSystem supplementarySystem,
        VIN? currentMarriageVIN)
    {
        var stations = supplementarySystem.EngineDressingStations;

        int totalStations = stations.Count;
        int occupiedStations = stations.Count(station => station.CurrentVIN is not null);
        int emptyStations = totalStations - occupiedStations;
        int faultedStations = stations.Count(station => station.IsFaulted);

        double averageWear = totalStations == 0
            ? 0.0
            : stations.Average(station => station.WearLevel);

        double minimumWear = totalStations == 0
            ? 0.0
            : stations.Min(station => station.WearLevel);

        bool isCurrentMarriagePowertrainReady =
            currentMarriageVIN is not null &&
            supplementarySystem.IsReadyFor(currentMarriageVIN);

        var stopType = DetermineStopType(
            supplementarySystem,
            currentMarriageVIN,
            isCurrentMarriagePowertrainReady);

        var status = DetermineStatus(
            supplementarySystem,
            stopType);

        var reasonText = DetermineReasonText(
            supplementarySystem,
            stopType,
            currentMarriageVIN,
            isCurrentMarriagePowertrainReady);

        double readyUsagePercentage = supplementarySystem.ReadyCapacity == 0
            ? 0.0
            : (double)supplementarySystem.ReadyCount / supplementarySystem.ReadyCapacity * 100.0;

        return new SupplementaryMonitorSnapshot(
            monitorName: "Supplementary",
            status: status,
            stopType: stopType,
            reasonText: reasonText,
            isActivated: supplementarySystem.IsActivated,
            readyPowertrainCount: supplementarySystem.ReadyCount,
            readyPowertrainCapacity: supplementarySystem.ReadyCapacity,
            readyPowertrainUsagePercentage: readyUsagePercentage,
            nextReadyPowertrainVinText: supplementarySystem.NextRequiredVinText,
            currentMarriageVinText: currentMarriageVIN?.ToString() ?? "NONE",
            isCurrentMarriagePowertrainReady: isCurrentMarriagePowertrainReady,
            engineDressingTotalStations: totalStations,
            engineDressingOccupiedStations: occupiedStations,
            engineDressingEmptyStations: emptyStations,
            engineDressingFaultedStations: faultedStations,
            engineDressingAverageWear: averageWear,
            engineDressingMinimumWear: minimumWear,
            engineDressingMovementCountLastTakt: supplementarySystem.EngineDressingMovementCountLastTakt,
            isOutputBlockedByReadyBuffer: supplementarySystem.IsOutputBlockedByReadyBuffer,
            readyPowertrainVinTexts: supplementarySystem.ReadyPowertrainVinTexts
        );
    }

    private static GroupStopType DetermineStopType(
        SupplementaryPowertrainSystem supplementarySystem,
        VIN? currentMarriageVIN,
        bool isCurrentMarriagePowertrainReady)
    {
        if (supplementarySystem.HasMaintenanceActive)
            return GroupStopType.MaintenanceStop;

        if (supplementarySystem.IsOutputBlockedByReadyBuffer)
            return GroupStopType.FullStop;

        if (currentMarriageVIN is not null && !isCurrentMarriagePowertrainReady)
            return GroupStopType.ShortageStop;

        return GroupStopType.Running;
    }

    private static GroupMonitorStatus DetermineStatus(
        SupplementaryPowertrainSystem supplementarySystem,
        GroupStopType stopType)
    {
        if (stopType == GroupStopType.MaintenanceStop)
            return GroupMonitorStatus.Red;

        if (stopType == GroupStopType.FullStop)
            return GroupMonitorStatus.Red;

        if (stopType == GroupStopType.ShortageStop)
            return GroupMonitorStatus.Yellow;

        if (!supplementarySystem.IsActivated)
            return GroupMonitorStatus.Neutral;

        if (supplementarySystem.ReadyCount <= LowReadyBufferThreshold)
            return GroupMonitorStatus.Yellow;

        return GroupMonitorStatus.Green;
    }

    private static string DetermineReasonText(
        SupplementaryPowertrainSystem supplementarySystem,
        GroupStopType stopType,
        VIN? currentMarriageVIN,
        bool isCurrentMarriagePowertrainReady)
    {
        if (!supplementarySystem.IsActivated)
            return "Waiting for main line trigger";

        return stopType switch
        {
            GroupStopType.MaintenanceStop =>
                "Engine Dressing maintenance intervention active",

            GroupStopType.FullStop =>
                "Ready powertrain buffer full; Engine Dressing output blocked",

            GroupStopType.ShortageStop =>
                currentMarriageVIN is null
                    ? "Ready powertrain unavailable"
                    : $"Powertrain unavailable for {currentMarriageVIN}",

            _ => isCurrentMarriagePowertrainReady
                ? "Powertrain ready for current marriage VIN"
                : "Running normally"
        };
    }
}