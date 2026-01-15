using Garage_2.Models.Entities;

namespace Garage_2.Models;

public class ParkingSpotWithUnits
{
    public ParkingSpotV2? Spot { get; set; }
    public int UsedUnits { get; set; }
    public int FreeUnits { get; set; }
}
