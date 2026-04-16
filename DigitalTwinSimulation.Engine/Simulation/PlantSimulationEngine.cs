using DigitalTwinSimulation.Core.Entities;
using DigitalTwinSimulation.Core.Enums;
using DigitalTwinSimulation.Core.Factories;
using DigitalTwinSimulation.Core.Layout;
using DigitalTwinSimulation.Engine.Services;

namespace DigitalTwinSimulation.Engine.Simulation;

public sealed class PlantSimulationEngine
{
    private const int MarriageStationIndexInCh2 = 4; // CH2-S05 (zero-based index)

    private int _nextVINSequence = 1;
    private readonly List<VIN> _finishedVehicles = [];
    private readonly SupplementaryPowertrainSystem _supplementarySystem;
    private readonly Random _random = new();

    public int CurrentTakt { get; private set; }
    public IReadOnlyList<StationGroup> MainLineGroups { get; }
    public StationGroup EngineDressingGroup { get; }

    public IReadOnlyList<VIN> FinishedVehicles => _finishedVehicles;
    public int FinishedVehicleCount => _finishedVehicles.Count;
    public int ReadyPowertrainCount => _supplementarySystem.ReadyCount;
    public string NextReadyPowertrainVin => _supplementarySystem.NextRequiredVinText;

    public PlantSimulationEngine()
    {
        MainLineGroups = AssemblyFactory.CreateMainLine();
        EngineDressingGroup = AssemblyFactory.CreateEngineDressing();
        _supplementarySystem = new SupplementaryPowertrainSystem(EngineDressingGroup);

        SeedInitialVehicle();
    }

    public void AdvanceOneTakt()
    {
        AdvanceWearAcrossPlant();
        ActivateSupplementarySystemIfNeeded();
        _supplementarySystem.AdvanceOneTakt();
        MoveVehiclesForward();
        CurrentTakt++;
    }

    public void Reset()
    {
        CurrentTakt = 0;
        _finishedVehicles.Clear();
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
                    continue;
                }

                if (nextStation.CurrentVIN is not null || nextStation.IsFaulted)
                    continue;

                if (IsCarrierTransitionMove(groupIndex, stationIndex, nextStation))
                {
                    if (!TryExecuteCarrierTransition(currentStation, nextStation))
                        continue;

                    continue;
                }

                if (IsMarriageStation(groupIndex, stationIndex))
                {
                    if (!TryExecuteMarriageMove(currentStation, nextStation))
                        continue;

                    continue;
                }

                nextStation.LoadVIN(currentStation.CurrentVIN);
                currentStation.Clear();
            }
        }

        SeedInitialVehicle();
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