namespace DigitalTwinSimulation.Core.Entities;

public sealed class CompletedVehicleRecord
{
    public VIN Vin { get; }
    public int CompletionTakt { get; }
    public double CompletionSimulatedMinute { get; }

    public string VinText => Vin.ToString();

    public CompletedVehicleRecord(
        VIN vin,
        int completionTakt,
        double completionSimulatedMinute)
    {
        Vin = vin;
        CompletionTakt = completionTakt;
        CompletionSimulatedMinute = completionSimulatedMinute;
    }
}