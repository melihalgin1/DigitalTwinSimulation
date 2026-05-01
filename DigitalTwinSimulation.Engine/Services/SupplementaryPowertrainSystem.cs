using DigitalTwinSimulation.Core.Entities;
using DigitalTwinSimulation.Core.Enums;

namespace DigitalTwinSimulation.Engine.Services;

public sealed class SupplementaryPowertrainSystem
{
    private const int MaxReadyQueueCapacity = 8;

    private readonly StationGroup _engineDressingGroup;
    private readonly Dictionary<int, PowertrainAssemblyRecord> _assemblyRecords = [];
    private readonly Queue<CompletedPowertrain> _readyQueue = [];

    private int _nextVINToSchedule = 1;
    private bool _isActivated;

    public SupplementaryPowertrainSystem(StationGroup engineDressingGroup)
    {
        _engineDressingGroup = engineDressingGroup;
    }

    public int ReadyCount => _readyQueue.Count;

    public string NextRequiredVinText =>
        _readyQueue.Count > 0
            ? _readyQueue.Peek().Vin.ToString()
            : "NONE";

    public bool IsActivated => _isActivated;

    public void Activate()
    {
        _isActivated = true;
    }

    public bool IsReadyFor(VIN vin)
    {
        if (_readyQueue.Count == 0)
            return false;

        var nextReady = _readyQueue.Peek();
        return nextReady.Vin.SequenceNumber == vin.SequenceNumber;
    }

    public bool TryConsumeReadyPowertrain(VIN vin)
    {
        if (_readyQueue.Count == 0)
            return false;

        var nextReady = _readyQueue.Peek();

        if (nextReady.Vin.SequenceNumber != vin.SequenceNumber)
            return false;

        _readyQueue.Dequeue();
        return true;
    }

    public void AdvanceOneTakt()
    {
        if (!_isActivated)
            return;

        CompleteEngineDressingOutputIfPresent();
        MoveEngineDressingForward();
        InjectNextVINIntoEngineDressingIfPossible();
    }

    public IReadOnlyList<string> GetEngineDressingSnapshot()
    {
        var result = new List<string>();

        foreach (var station in _engineDressingGroup.Stations)
        {
            var vinText = station.CurrentVIN?.ToString() ?? "EMPTY";
            var stateText = station.State.ToString();
            var wearText = $"{station.WearLevel:0.0}%";

            result.Add($"{station.Id} : {vinText} | {stateText} | {wearText}");
        }

        return result;
    }

    private void CompleteEngineDressingOutputIfPresent()
    {
        var outputStation = _engineDressingGroup.Stations[^1];

        if (outputStation.IsFaulted)
            return;

        if (outputStation.CurrentVIN is null)
            return;

        var vin = outputStation.CurrentVIN;

        if (_assemblyRecords.TryGetValue(vin.SequenceNumber, out var record))
        {
            record.MarkEngineDressingReady();

            if (record.IsComplete)
            {
                if (_readyQueue.Count < MaxReadyQueueCapacity)
                {
                    _readyQueue.Enqueue(new CompletedPowertrain(vin));
                    _assemblyRecords.Remove(vin.SequenceNumber);
                    outputStation.Clear();
                }
                else
                {
                    return;
                }
            }
            else
            {
                outputStation.Clear();
            }
        }
        else
        {
            outputStation.Clear();
        }
    }

    private void MoveEngineDressingForward()
    {
        for (int i = _engineDressingGroup.Stations.Count - 2; i >= 0; i--)
        {
            var currentStation = _engineDressingGroup.Stations[i];
            var nextStation = _engineDressingGroup.Stations[i + 1];

            if (currentStation.CurrentVIN is null)
                continue;

            if (currentStation.IsFaulted)
                continue;

            if (nextStation.CurrentVIN is not null)
                continue;

            if (nextStation.IsFaulted)
                continue;

            nextStation.LoadVIN(currentStation.CurrentVIN);
            currentStation.Clear();
        }
    }

    private void InjectNextVINIntoEngineDressingIfPossible()
    {
        var firstStation = _engineDressingGroup.Stations[0];

        if (firstStation.CurrentVIN is not null)
            return;

        if (firstStation.IsFaulted)
            return;

        var vin = new VIN(_nextVINToSchedule++);
        firstStation.LoadVIN(vin);

        var record = new PowertrainAssemblyRecord(vin);
        record.MarkFrontSuspensionReady();
        record.MarkRearAxleReady();

        _assemblyRecords[vin.SequenceNumber] = record;
    }
}