using DigitalTwinSimulation.Core.Enums;

namespace DigitalTwinSimulation.Core.Entities;

public sealed class Station
{
    private const double TrimWearDrop = 0.25;
    private const double ChassisWearDrop = 0.35;
    private const double FinalLineWearDrop = 0.0;
    private const double EngineDressingWearDrop = 0.35;

    private const double TrimRandomWearSpike = 2.5;
    private const double ChassisRandomWearSpike = 5.0;
    private const double FinalLineRandomWearSpike = 3.0;
    private const double EngineDressingRandomWearSpike = 5.0;

    private const double RandomWearSpikeProbability = 0.01;

    private const double RepairRate = 70.0;
    private const double FaultThreshold = 10.0;
    private const double RepairCompleteThreshold = 75.0;

    private const int PostRepairSpikeImmunityTakts = 8;

    public string Id { get; }
    public StationGroupType GroupType { get; }

    public StationState State { get; private set; } = StationState.Empty;

    public VIN? CurrentVIN { get; private set; }

    public double WearLevel { get; private set; } = 100.0;

    private int _agvTravelTimer;
    private int _postRepairSpikeImmunityTimer;

    public Station(string id, StationGroupType groupType)
    {
        Id = id;
        GroupType = groupType;
    }

    public void LoadVIN(VIN vin)
    {
        CurrentVIN = vin;
        State = StationState.Loaded;
    }

    public void Clear()
    {
        CurrentVIN = null;
        State = StationState.Empty;
    }

    public bool IsFaulted =>
        State == StationState.Faulted ||
        State == StationState.AgvMoving ||
        State == StationState.Repairing;

    public void AdvanceWear(Random random)
    {
        if (_postRepairSpikeImmunityTimer > 0 &&
            State != StationState.AgvMoving &&
            State != StationState.Repairing &&
            State != StationState.Faulted)
        {
            _postRepairSpikeImmunityTimer--;
        }

        if (State == StationState.AgvMoving)
        {
            _agvTravelTimer--;

            if (_agvTravelTimer <= 0)
            {
                State = StationState.Repairing;
            }

            return;
        }

        if (State == StationState.Repairing)
        {
            WearLevel += RepairRate;

            if (WearLevel > 100.0)
            {
                WearLevel = 100.0;
            }

            if (WearLevel >= RepairCompleteThreshold)
            {
                _postRepairSpikeImmunityTimer = PostRepairSpikeImmunityTakts;

                State = CurrentVIN is null
                    ? StationState.Empty
                    : StationState.Loaded;
            }

            return;
        }

        if (State == StationState.Faulted)
        {
            State = StationState.AgvMoving;
            _agvTravelTimer = 1;
            return;
        }

        // Empty stations do not accumulate gradual wear.
        if (CurrentVIN is not null)
        {
            WearLevel -= GetGradualWearDrop();

            if (_postRepairSpikeImmunityTimer == 0 &&
                random.NextDouble() < RandomWearSpikeProbability)
            {
                WearLevel -= GetRandomWearSpike();
            }
        }

        if (WearLevel <= FaultThreshold)
        {
            WearLevel = FaultThreshold;
            State = StationState.Faulted;
        }
    }

    private double GetGradualWearDrop()
    {
        return GroupType switch
        {
            StationGroupType.TR1 => TrimWearDrop,
            StationGroupType.TR2 => TrimWearDrop,
            StationGroupType.CH1 => ChassisWearDrop,
            StationGroupType.CH2 => ChassisWearDrop,
            StationGroupType.Final => FinalLineWearDrop,
            StationGroupType.EngineDressing => EngineDressingWearDrop,
            _ => ChassisWearDrop
        };
    }

    private double GetRandomWearSpike()
    {
        return GroupType switch
        {
            StationGroupType.TR1 => TrimRandomWearSpike,
            StationGroupType.TR2 => TrimRandomWearSpike,
            StationGroupType.CH1 => ChassisRandomWearSpike,
            StationGroupType.CH2 => ChassisRandomWearSpike,
            StationGroupType.Final => FinalLineRandomWearSpike,
            StationGroupType.EngineDressing => EngineDressingRandomWearSpike,
            _ => ChassisRandomWearSpike
        };
    }
}