using DigitalTwinSimulation.Core.Enums;

namespace DigitalTwinSimulation.Core.Entities;

public sealed class SupplementaryPart
{
    public VIN Vin { get; }
    public SupplementaryPartType PartType { get; }

    public SupplementaryPart(VIN vin, SupplementaryPartType partType)
    {
        Vin = vin;
        PartType = partType;
    }
}