using DigitalTwinSimulation.Core.Entities;
using DigitalTwinSimulation.Core.Enums;
using DigitalTwinSimulation.Core.Monitoring;

namespace DigitalTwinSimulation.Engine.Services;

public sealed class StandardGroupMonitorService
{
    private const int StartupFillTakts = 40;

    public IReadOnlyList<GroupMonitorSnapshot> BuildSnapshots(
        int currentTakt,
        IReadOnlyList<StationGroup> mainLineGroups,
        IReadOnlyDictionary<StationGroupType, int> movementCountsLastTakt,
        Func<VIN, bool> isPowertrainReady,
        Func<bool> canAsrsFeedTr1,
        Func<bool> canTr1FeedTr2,
        Func<bool> canTr2FeedCh1,
        Func<bool> canCh1FeedCh2,
        Func<bool> canCh2FeedFinal)
    {
        var result = new List<GroupMonitorSnapshot>();

        foreach (var group in mainLineGroups)
        {
            result.Add(
                BuildSnapshot(
                    currentTakt,
                    group,
                    mainLineGroups,
                    movementCountsLastTakt,
                    isPowertrainReady,
                    canAsrsFeedTr1,
                    canTr1FeedTr2,
                    canTr2FeedCh1,
                    canCh1FeedCh2,
                    canCh2FeedFinal));
        }

        return result;
    }

    private GroupMonitorSnapshot BuildSnapshot(
        int currentTakt,
        StationGroup group,
        IReadOnlyList<StationGroup> mainLineGroups,
        IReadOnlyDictionary<StationGroupType, int> movementCountsLastTakt,
        Func<VIN, bool> isPowertrainReady,
        Func<bool> canAsrsFeedTr1,
        Func<bool> canTr1FeedTr2,
        Func<bool> canTr2FeedCh1,
        Func<bool> canCh1FeedCh2,
        Func<bool> canCh2FeedFinal)
    {
        int totalStations = group.Stations.Count;
        int emptyStations = group.Stations.Count(s => s.CurrentVIN is null);
        int occupiedStations = totalStations - emptyStations;
        int faultedStations = group.Stations.Count(s => s.IsFaulted);

        double emptyPercentage = totalStations == 0 ? 0 : (double)emptyStations / totalStations * 100.0;
        double faultedPercentage = totalStations == 0 ? 0 : (double)faultedStations / totalStations * 100.0;
        double averageWear = totalStations == 0 ? 0 : group.Stations.Average(s => s.WearLevel);
        double minimumWear = totalStations == 0 ? 0 : group.Stations.Min(s => s.WearLevel);

        string leadVINText = GetLeadVINText(group);
        string tailVINText = GetTailVINText(group);

        int movementCountLastTakt = movementCountsLastTakt.TryGetValue(group.GroupType, out var count)
            ? count
            : 0;

        bool isStartupPhase = currentTakt < StartupFillTakts;

        var stopType = DetermineStopType(
            isStartupPhase,
            group,
            canAsrsFeedTr1,
            canTr1FeedTr2,
            canTr2FeedCh1,
            canCh1FeedCh2,
            canCh2FeedFinal);

        var status = DetermineStatus(
            isStartupPhase,
            group,
            mainLineGroups,
            stopType,
            emptyPercentage,
            averageWear,
            isPowertrainReady);

        var reasonText = DetermineReasonText(
            isStartupPhase,
            group,
            stopType,
            isPowertrainReady,
            canAsrsFeedTr1,
            canTr1FeedTr2,
            canTr2FeedCh1,
            canCh1FeedCh2,
            canCh2FeedFinal);

        return new GroupMonitorSnapshot(
            group.GroupType.ToString(),
            status,
            stopType,
            reasonText,
            totalStations,
            occupiedStations,
            emptyStations,
            faultedStations,
            emptyPercentage,
            faultedPercentage,
            averageWear,
            minimumWear,
            leadVINText,
            tailVINText,
            movementCountLastTakt
        );
    }

    private GroupMonitorStatus DetermineStatus(
        bool isStartupPhase,
        StationGroup group,
        IReadOnlyList<StationGroup> mainLineGroups,
        GroupStopType stopType,
        double emptyPercentage,
        double averageWear,
        Func<VIN, bool> isPowertrainReady)
    {
        if (stopType == GroupStopType.MaintenanceStop)
            return GroupMonitorStatus.Red;

        if (isStartupPhase)
            return GroupMonitorStatus.Neutral;

        if (stopType == GroupStopType.FullStop)
            return GroupMonitorStatus.Red;

        if (stopType == GroupStopType.ShortageStop)
            return GroupMonitorStatus.Yellow;

        return group.GroupType switch
        {
            StationGroupType.TR1 or StationGroupType.TR2
                => emptyPercentage >= 30.0 ? GroupMonitorStatus.Yellow : GroupMonitorStatus.Green,

            StationGroupType.CH1
                => IsCh1WaitingForVIN(mainLineGroups) ? GroupMonitorStatus.Yellow : GroupMonitorStatus.Green,

            StationGroupType.CH2
                => IsMarriageWaiting(group, isPowertrainReady) ? GroupMonitorStatus.Yellow : GroupMonitorStatus.Green,

            StationGroupType.Final
                => averageWear < 50.0 ? GroupMonitorStatus.Yellow : GroupMonitorStatus.Green,

            _ => GroupMonitorStatus.Green
        };
    }

    private GroupStopType DetermineStopType(
        bool isStartupPhase,
        StationGroup group,
        Func<bool> canAsrsFeedTr1,
        Func<bool> canTr1FeedTr2,
        Func<bool> canTr2FeedCh1,
        Func<bool> canCh1FeedCh2,
        Func<bool> canCh2FeedFinal)
    {
        if (group.Stations.Any(s => s.State == StationState.Faulted ||
                                    s.State == StationState.AgvMoving ||
                                    s.State == StationState.Repairing))
        {
            return GroupStopType.MaintenanceStop;
        }

        if (isStartupPhase)
        {
            return GroupStopType.Running;
        }

        if (IsFullStop(group, canTr1FeedTr2, canTr2FeedCh1, canCh1FeedCh2, canCh2FeedFinal))
        {
            return GroupStopType.FullStop;
        }

        if (IsShortageStop(group, canAsrsFeedTr1, canTr1FeedTr2, canTr2FeedCh1, canCh1FeedCh2, canCh2FeedFinal))
        {
            return GroupStopType.ShortageStop;
        }

        return GroupStopType.Running;
    }

    private string DetermineReasonText(
        bool isStartupPhase,
        StationGroup group,
        GroupStopType stopType,
        Func<VIN, bool> isPowertrainReady,
        Func<bool> canAsrsFeedTr1,
        Func<bool> canTr1FeedTr2,
        Func<bool> canTr2FeedCh1,
        Func<bool> canCh1FeedCh2,
        Func<bool> canCh2FeedFinal)
    {
        if (stopType == GroupStopType.MaintenanceStop)
            return "Station fault / AGV / repair active";

        if (isStartupPhase)
            return "Startup fill phase";

        return stopType switch
        {
            GroupStopType.FullStop => group.GroupType switch
            {
                StationGroupType.TR1 => "TR1 full and blocked by TR2",
                StationGroupType.TR2 => "TR2 full and blocked by CH1",
                StationGroupType.CH1 => "CH1 full and blocked by CH2",
                StationGroupType.CH2 => "CH2 full and blocked by Final or marriage downstream hold",
                StationGroupType.Final => "Final full and unable to discharge",
                _ => "Group full and blocked ahead"
            },

            GroupStopType.ShortageStop => group.GroupType switch
            {
                StationGroupType.TR1 => canAsrsFeedTr1() ? "Running normally" : "ASRS cannot feed TR1-S01",
                StationGroupType.TR2 => canTr1FeedTr2() ? "Running normally" : "TR1 cannot feed TR2-S01",
                StationGroupType.CH1 => canTr2FeedCh1() ? "Running normally" : "TR2 cannot feed CH1-S01",
                StationGroupType.CH2 => canCh1FeedCh2() ? "Running normally" : "CH1 cannot feed CH2-S01",
                StationGroupType.Final => canCh2FeedFinal() ? "Running normally" : "CH2 cannot feed Final-S01",
                _ => "Upstream group cannot feed first station"
            },

            _ => group.GroupType switch
            {
                StationGroupType.CH2 when IsMarriageWaiting(group, isPowertrainReady) => "Marriage waiting for powertrain",
                _ => "Running normally"
            }
        };
    }

    private bool IsFullStop(
        StationGroup group,
        Func<bool> canTr1FeedTr2,
        Func<bool> canTr2FeedCh1,
        Func<bool> canCh1FeedCh2,
        Func<bool> canCh2FeedFinal)
    {
        bool allStationsFilled = group.Stations.All(s => s.CurrentVIN is not null);

        if (!allStationsFilled)
            return false;

        return group.GroupType switch
        {
            StationGroupType.TR1 => !canTr1FeedTr2(),
            StationGroupType.TR2 => !canTr2FeedCh1(),
            StationGroupType.CH1 => !canCh1FeedCh2(),
            StationGroupType.CH2 => !canCh2FeedFinal(),
            StationGroupType.Final => false,
            _ => false
        };
    }

    private bool IsShortageStop(
        StationGroup group,
        Func<bool> canAsrsFeedTr1,
        Func<bool> canTr1FeedTr2,
        Func<bool> canTr2FeedCh1,
        Func<bool> canCh1FeedCh2,
        Func<bool> canCh2FeedFinal)
    {
        var firstStation = group.Stations[0];

        if (firstStation.CurrentVIN is not null)
            return false;

        if (firstStation.IsFaulted)
            return false;

        return group.GroupType switch
        {
            StationGroupType.TR1 => !canAsrsFeedTr1(),
            StationGroupType.TR2 => !canTr1FeedTr2(),
            StationGroupType.CH1 => !canTr2FeedCh1(),
            StationGroupType.CH2 => !canCh1FeedCh2(),
            StationGroupType.Final => !canCh2FeedFinal(),
            _ => false
        };
    }

    private static string GetLeadVINText(StationGroup group)
    {
        var lead = group.Stations.LastOrDefault(s => s.CurrentVIN is not null)?.CurrentVIN;
        return lead?.ToString() ?? "NONE";
    }

    private static string GetTailVINText(StationGroup group)
    {
        var tail = group.Stations.FirstOrDefault(s => s.CurrentVIN is not null)?.CurrentVIN;
        return tail?.ToString() ?? "NONE";
    }

    private static bool IsCh1WaitingForVIN(IReadOnlyList<StationGroup> groups)
    {
        var ch1 = groups.First(g => g.GroupType == StationGroupType.CH1);
        var tr2 = groups.First(g => g.GroupType == StationGroupType.TR2);

        var ch1First = ch1.Stations[0];
        var tr2Last = tr2.Stations[^1];

        return ch1First.CurrentVIN is null &&
               !ch1First.IsFaulted &&
               (tr2Last.CurrentVIN is null || tr2Last.IsFaulted);
    }

    private static bool IsMarriageWaiting(StationGroup ch2Group, Func<VIN, bool> isPowertrainReady)
    {
        if (ch2Group.Stations.Count <= 4)
            return false;

        var marriageStation = ch2Group.Stations[4];

        if (marriageStation.CurrentVIN is null)
            return false;

        if (marriageStation.IsFaulted)
            return false;

        return !isPowertrainReady(marriageStation.CurrentVIN);
    }
}