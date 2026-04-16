namespace DigitalTwinSimulation.Core.Entities;

public sealed class PowertrainAssemblyRecord
{
    public VIN Vin { get; }

    public bool FrontSuspensionReady { get; private set; }
    public bool RearAxleReady { get; private set; }
    public bool EngineDressingReady { get; private set; }

    public bool IsComplete =>
        FrontSuspensionReady &&
        RearAxleReady &&
        EngineDressingReady;

    public PowertrainAssemblyRecord(VIN vin)
    {
        Vin = vin;
    }

    public void MarkFrontSuspensionReady()
    {
        FrontSuspensionReady = true;
    }

    public void MarkRearAxleReady()
    {
        RearAxleReady = true;
    }

    public void MarkEngineDressingReady()
    {
        EngineDressingReady = true;
    }
}