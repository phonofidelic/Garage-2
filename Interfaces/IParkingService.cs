using Garage_2.Models;
using Garage_2.Models.Entities;

namespace Garage_2.Interfaces;

public interface IParkingService
{
    Task<ParkingResult> ParkVehicleAsync(ParkedVehicle vehicle);

    Task<ParkingResult> ParkVehicleAsync(Vehicle vehicle);
}
