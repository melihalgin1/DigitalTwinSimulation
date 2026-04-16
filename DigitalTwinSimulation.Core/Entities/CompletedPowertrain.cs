namespace DigitalTwinSimulation.Core.Entities;

public sealed class CompletedPowertrain
{
    public VIN Vin { get; }

    public CompletedPowertrain(VIN vin)
    {
        Vin = vin;
    }

    public override string ToString()
        => $"PT-{Vin}";
}