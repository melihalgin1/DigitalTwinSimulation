namespace DigitalTwinSimulation.Core.Entities;

public sealed class VIN
{
    public int SequenceNumber { get; }

    public VIN(int sequenceNumber)
    {
        SequenceNumber = sequenceNumber;
    }

    public override string ToString() => $"VIN-{SequenceNumber:D5}";
}