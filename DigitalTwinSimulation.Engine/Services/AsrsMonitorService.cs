using DigitalTwinSimulation.Core.Monitoring;

namespace DigitalTwinSimulation.Engine.Services;

public sealed class AsrsMonitorService
{
    private const int AsrsCapacity = 50;

    public AsrsMonitorSnapshot BuildPlaceholderSnapshot()
    {
        int storedVehicleCount = 0;
        double usagePercentage = 0.0;

        string nextRequiredVinText = "NOT IMPLEMENTED";
        bool isNextRequiredVinPresent = false;
        bool isReleaseReady = false;

        return new AsrsMonitorSnapshot(
            monitorName: "ASRS",
            status: GroupMonitorStatus.Neutral,
            stopType: GroupStopType.Running,
            reasonText: "ASRS internal logic not implemented yet",
            capacity: AsrsCapacity,
            storedVehicleCount: storedVehicleCount,
            usagePercentage: usagePercentage,
            nextRequiredVinText: nextRequiredVinText,
            isNextRequiredVinPresent: isNextRequiredVinPresent,
            isReleaseReady: isReleaseReady,
            storedVinTexts: []
        );
    }
}