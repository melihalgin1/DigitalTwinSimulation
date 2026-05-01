using DigitalTwinSimulation.Core.Entities;
using DigitalTwinSimulation.Core.Enums;
using DigitalTwinSimulation.Core.Factories;
using DigitalTwinSimulation.Core.Layout;
using DigitalTwinSimulation.Core.Monitoring;
using DigitalTwinSimulation.Engine.Services;

namespace DigitalTwinSimulation.Engine.Simulation;

public sealed class PlantSimulationEngine
{
    private const int MarriageStationIndexInCh2 = 4; // CH2-S05 (zero-based index)

    private int _nextVINSequence = 1;
    private readonly List<VIN> _finishedVehicles = [];
    private SupplementaryPowertrainSystem _supplementarySystem;
    private readonly StandardGroupMonitorService _standardGroupMonitorService = new();
    private readonly SupplementaryMonitorService _supplementaryMonitorService = new();
    private readonly AsrsMonitorService _asrsMonitorService = new();
    private readonly Dictionary<StationGroupType, int> _movementCountsLastTakt = [];
    private readonly Random _random = new();

    public int CurrentTakt { get; private set; }
    public double SimulatedElapsedMinutes { get; private set; }

    public IReadOnlyList<StationGroup> MainLineGroups { get; private set; }
    public StationGroup EngineDressingGroup { get; private set; }

    public IReadOnlyList<VIN> FinishedVehicles => _finishedVehicles;
    public int FinishedVehicleCount => _finishedVehicles.Count;
    public int ReadyPowertrainCount => _supplementarySystem.ReadyCount;
    public string NextReadyPowertrainVin => _supplementarySystem.NextRequiredVinText;

    public PlantSimulationEngine()
    {
        MainLineGroups = AssemblyFactory.CreateMainLine();
        EngineDressingGroup = AssemblyFactory.CreateEngineDressing();
        _supplementarySystem = new SupplementaryPowertrainSystem(EngineDressingGroup);

        InitializeMovementCounters();
        SeedInitialVehicle();
    }

    public void AdvanceOneTakt()
    {
        AdvanceOneTakt(1.0);
    }

    public void AdvanceOneTakt(double taktDurationMinutes)
    {
        if (taktDurationMinutes <= 0)
        {
            taktDurationMinutes = 1.0;
        }

        ResetMovementCounters();
        AdvanceWearAcrossPlant();
        ActivateSupplementarySystemIfNeeded();
        _supplementarySystem.AdvanceOneTakt();
        MoveVehiclesForward();

        CurrentTakt++;
        SimulatedElapsedMinutes += taktDurationMinutes;
    }

    public void Reset()
    {
        CurrentTakt = 0;
        SimulatedElapsedMinutes = 0.0;
        _nextVINSequence = 1;
        _finishedVehicles.Clear();

        MainLineGroups = AssemblyFactory.CreateMainLine();
        EngineDressingGroup = AssemblyFactory.CreateEngineDressing();
        _supplementarySystem = new SupplementaryPowertrainSystem(EngineDressingGroup);

        _movementCountsLastTakt.Clear();
        InitializeMovementCounters();
        SeedInitialVehicle();
    }

    public IReadOnlyList<GroupMonitorSnapshot> GetStandardGroupMonitors()
    {
        return _standardGroupMonitorService.BuildSnapshots(
            CurrentTakt,
            MainLineGroups,
            _movementCountsLastTakt,
            vin => _supplementarySystem.IsReadyFor(vin),
            () => CanAsrsFeedTr1(),
            () => CanTr1FeedTr2(),
            () => CanFeedCh1EntryFromTr2(),
            () => CanCh1FeedCh2(),
            () => CanCh2FeedFinal()
        );
    }

    public SupplementaryMonitorSnapshot GetSupplementaryMonitor()
    {
        return _supplementaryMonitorService.BuildSnapshot(
            _supplementarySystem,
            GetCurrentMarriageVIN()
        );
    }

    public AsrsMonitorSnapshot GetAsrsMonitor()
    {
        return _asrsMonitorService.BuildPlaceholderSnapshot();
    }

    public IReadOnlyList<StationGroupSnapshot> GetGroupedSnapshot()
    {
        var result = new List<StationGroupSnapshot>();

        foreach (var group in MainLineGroups)
        {
            var stationSnapshots = new List<StationSnapshot>();

            foreach (var station in group.Stations)
            {
                var vinText = station.CurrentVIN?.ToString() ?? "EMPTY";
                var stateText = station.State.ToString();
                var wearText = $"{station.WearLevel:0.0}%";

                stationSnapshots.Add(
                    new StationSnapshot(
                        station.Id,
                        vinText,
                        stateText,
                        wearText
                    )
                );
            }

            result.Add(
                new StationGroupSnapshot(
                    group.GroupType.ToString(),
                    stationSnapshots
                )
            );
        }

        return result;
    }

    public IReadOnlyList<string> GetEngineDressingSnapshot()
    {
        return _supplementarySystem.GetEngineDressingSnapshot();
    }

    private void InitializeMovementCounters()
    {
        foreach (var group in MainLineGroups)
        {
            _movementCountsLastTakt[group.GroupType] = 0;
        }
    }

    private void ResetMovementCounters()
    {
        var keys = _movementCountsLastTakt.Keys.ToList();

        foreach (var key in keys)
        {
            _movementCountsLastTakt[key] = 0;
        }
    }

    private void IncrementMovementCount(StationGroupType groupType)
    {
        if (_movementCountsLastTakt.ContainsKey(groupType))
        {
            _movementCountsLastTakt[groupType]++;
        }
    }

    private void AdvanceWearAcrossPlant()
    {
        foreach (var group in MainLineGroups)
        {
            foreach (var station in group.Stations)
            {
                station.AdvanceWear(_random);
            }
        }

        foreach (var station in EngineDressingGroup.Stations)
        {
            station.AdvanceWear(_random);
        }
    }

    private void ActivateSupplementarySystemIfNeeded()
    {
        if (_supplementarySystem.IsActivated)
            return;

        var tr2 = MainLineGroups.First(g => g.GroupType == StationGroupType.TR2);
        var tr2First = tr2.Stations[0];

        if (tr2First.CurrentVIN is not null)
        {
            _supplementarySystem.Activate();
        }
    }

    private void SeedInitialVehicle()
    {
        var firstGroup = MainLineGroups[0];
        var firstStation = firstGroup.Stations[0];

        if (firstStation.CurrentVIN is null && !firstStation.IsFaulted)
        {
            firstStation.LoadVIN(CreateNextVIN());
        }
    }

    private VIN CreateNextVIN()
    {
        return new VIN(_nextVINSequence++);
    }

    private void MoveVehiclesForward()
    {
        for (int groupIndex = MainLineGroups.Count - 1; groupIndex >= 0; groupIndex--)
        {
            var group = MainLineGroups[groupIndex];

            for (int stationIndex = group.Stations.Count - 1; stationIndex >= 0; stationIndex--)
            {
                var currentStation = group.Stations[stationIndex];

                if (currentStation.CurrentVIN is null)
                    continue;

                if (currentStation.IsFaulted)
                    continue;

                if (IsChassisStation(group.GroupType) &&
                    ShouldHoldChassisZone(groupIndex, stationIndex))
                {
                    continue;
                }

                Station? nextStation = GetNextStation(groupIndex, stationIndex);

                if (nextStation is null)
                {
                    _finishedVehicles.Add(currentStation.CurrentVIN);
                    currentStation.Clear();
                    IncrementMovementCount(group.GroupType);
                    continue;
                }

                if (nextStation.CurrentVIN is not null || nextStation.IsFaulted)
                    continue;

                if (IsCarrierTransitionMove(groupIndex, stationIndex, nextStation))
                {
                    if (!TryExecuteCarrierTransition(currentStation, nextStation))
                        continue;

                    IncrementMovementCount(group.GroupType);
                    continue;
                }

                if (IsMarriageStation(groupIndex, stationIndex))
                {
                    if (!TryExecuteMarriageMove(currentStation, nextStation))
                        continue;

                    IncrementMovementCount(group.GroupType);
                    continue;
                }

                nextStation.LoadVIN(currentStation.CurrentVIN);
                currentStation.Clear();
                IncrementMovementCount(group.GroupType);
            }
        }

        SeedInitialVehicle();
    }

    private VIN? GetCurrentMarriageVIN()
    {
        var ch2 = MainLineGroups.First(group => group.GroupType == StationGroupType.CH2);

        if (ch2.Stations.Count <= MarriageStationIndexInCh2)
            return null;

        return ch2.Stations[MarriageStationIndexInCh2].CurrentVIN;
    }

    private bool IsMarriageStation(int groupIndex, int stationIndex)
    {
        var group = MainLineGroups[groupIndex];
        return group.GroupType == StationGroupType.CH2
               && stationIndex == MarriageStationIndexInCh2;
    }

    private bool TryExecuteMarriageMove(Station currentStation, Station nextStation)
    {
        if (currentStation.CurrentVIN is null)
            return false;

        if (currentStation.IsFaulted)
            return false;

        if (nextStation.CurrentVIN is not null)
            return false;

        if (nextStation.IsFaulted)
            return false;

        if (!_supplementarySystem.TryConsumeReadyPowertrain(currentStation.CurrentVIN))
            return false;

        nextStation.LoadVIN(currentStation.CurrentVIN);
        currentStation.Clear();
        return true;
    }

    private bool IsCarrierTransitionMove(int groupIndex, int stationIndex, Station nextStation)
    {
        var currentGroup = MainLineGroups[groupIndex];

        return currentGroup.GroupType == StationGroupType.TR2
               && stationIndex == currentGroup.Stations.Count - 1
               && nextStation.GroupType == StationGroupType.CH1
               && nextStation.Id == "CH1-S01";
    }

    private bool TryExecuteCarrierTransition(Station currentStation, Station nextStation)
    {
        if (currentStation.CurrentVIN is null)
            return false;

        if (currentStation.IsFaulted)
            return false;

        if (nextStation.CurrentVIN is not null)
            return false;

        if (nextStation.IsFaulted)
            return false;

        if (!IsLoadedSkidAvailableAtTransferStation(currentStation))
            return false;

        nextStation.LoadVIN(currentStation.CurrentVIN);
        currentStation.Clear();
        return true;
    }

    private bool IsLoadedSkidAvailableAtTransferStation(Station station)
    {
        return station.CurrentVIN is not null;
    }

    private bool IsChassisStation(StationGroupType groupType)
    {
        return groupType == StationGroupType.CH1 || groupType == StationGroupType.CH2;
    }

    private bool ShouldHoldChassisZone(int groupIndex, int stationIndex)
    {
        int currentLinearIndex = GetChassisLinearIndex(groupIndex, stationIndex);
        if (currentLinearIndex < 0)
            return false;

        if (!CanFeedCh1EntryFromTr2())
            return true;

        for (int g = 0; g < MainLineGroups.Count; g++)
        {
            var group = MainLineGroups[g];

            if (!IsChassisStation(group.GroupType))
                continue;

            for (int s = 0; s < group.Stations.Count; s++)
            {
                int linearIndex = GetChassisLinearIndex(g, s);
                if (linearIndex < 0 || linearIndex >= currentLinearIndex)
                    continue;

                if (group.Stations[s].IsFaulted)
                    return true;
            }
        }

        return false;
    }

    private bool CanAsrsFeedTr1()
    {
        var tr1 = MainLineGroups.First(g => g.GroupType == StationGroupType.TR1);
        var tr1First = tr1.Stations[0];

        if (tr1First.IsFaulted)
            return false;

        return true;
    }

    private bool CanTr1FeedTr2()
    {
        var tr1 = MainLineGroups.First(g => g.GroupType == StationGroupType.TR1);
        var tr2 = MainLineGroups.First(g => g.GroupType == StationGroupType.TR2);

        var tr1Last = tr1.Stations[^1];
        var tr2First = tr2.Stations[0];

        if (tr1Last.IsFaulted)
            return false;

        if (tr1Last.CurrentVIN is null)
            return false;

        if (tr2First.IsFaulted)
            return false;

        return true;
    }

    private bool CanFeedCh1EntryFromTr2()
    {
        var tr2 = MainLineGroups.First(g => g.GroupType == StationGroupType.TR2);
        var ch1 = MainLineGroups.First(g => g.GroupType == StationGroupType.CH1);

        var tr2Last = tr2.Stations[^1];
        var ch1First = ch1.Stations[0];

        if (tr2Last.IsFaulted)
            return false;

        if (tr2Last.CurrentVIN is null)
            return false;

        if (ch1First.IsFaulted)
            return false;

        return true;
    }

    private bool CanCh1FeedCh2()
    {
        var ch1 = MainLineGroups.First(g => g.GroupType == StationGroupType.CH1);
        var ch2 = MainLineGroups.First(g => g.GroupType == StationGroupType.CH2);

        var ch1Last = ch1.Stations[^1];
        var ch2First = ch2.Stations[0];

        if (ch1Last.IsFaulted)
            return false;

        if (ch1Last.CurrentVIN is null)
            return false;

        if (ch2First.IsFaulted)
            return false;

        return true;
    }

    private bool CanCh2FeedFinal()
    {
        var ch2 = MainLineGroups.First(g => g.GroupType == StationGroupType.CH2);
        var final = MainLineGroups.First(g => g.GroupType == StationGroupType.Final);

        var ch2Last = ch2.Stations[^1];
        var finalFirst = final.Stations[0];

        if (ch2Last.IsFaulted)
            return false;

        if (ch2Last.CurrentVIN is null)
            return false;

        if (finalFirst.IsFaulted)
            return false;

        return true;
    }

    private int GetChassisLinearIndex(int groupIndex, int stationIndex)
    {
        var group = MainLineGroups[groupIndex];

        return group.GroupType switch
        {
            StationGroupType.CH1 => stationIndex,
            StationGroupType.CH2 => 9 + stationIndex,
            _ => -1
        };
    }

    private Station? GetNextStation(int groupIndex, int stationIndex)
    {
        var currentGroup = MainLineGroups[groupIndex];

        if (stationIndex < currentGroup.Stations.Count - 1)
        {
            return currentGroup.Stations[stationIndex + 1];
        }

        if (groupIndex < MainLineGroups.Count - 1)
        {
            var nextGroup = MainLineGroups[groupIndex + 1];
            return nextGroup.Stations[0];
        }

        return null;
    }
}