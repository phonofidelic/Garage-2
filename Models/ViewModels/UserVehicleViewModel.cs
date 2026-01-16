namespace Garage_2.Models.ViewModels;

public class UserVehicleViewModel
{
    public int VehicleId { get; set; }
    public string RegistrationNumber { get; set; } = default!;
    public string Make { get; set; } = default!;
    public string Model { get; set; } = default!;
    public string Color { get; set; } = default!;
    public int NumberOfWheels { get; set; }
    public string VehicleType { get; set; } = default!;

    // Active parking (if exists)
    public DateTime? ArrivalTime { get; set; }
    public List<int> ParkingSpots { get; set; } = new();
    public int UnitsUsed { get; set; }
    public decimal PriceNow { get; set; }
}
