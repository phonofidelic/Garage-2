using Garage_2.Models;

namespace Garage_2.Interfaces;

public interface IParkingService
{
    Task<ParkingResult> ParkVehicleAsync(ParkedVehicle vehicle);
}
