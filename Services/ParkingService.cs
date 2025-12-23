using Garage_2.Data;
using Garage_2.Interfaces;
using Garage_2.Models;
using Microsoft.EntityFrameworkCore;

namespace Garage_2.Services;

public class ParkingService : IParkingService
{
    private readonly GarageContext _context;

    public ParkingService(GarageContext context)
    {
        _context = context;
    }

    public async Task<ParkingResult> ParkVehicleAsync(ParkedVehicle parkedVehicle)
    {
        // Add the vehicle first to get an Id
        _context.ParkedVehicle.Add(parkedVehicle);
        await _context.SaveChangesAsync();

        bool parkingSpotAssigned = false;

        // Get all spots with usage information
        IQueryable<ParkingSpotWithUnits> spotsWithUsage = _context.ParkingSpots.Select(spot => new ParkingSpotWithUnits
        {
            Spot = spot,
            UsedUnits = spot.VehicleSpots.Sum(vs => (int?)vs.UnitsUsed) ?? 0,
            FreeUnits = spot.CapacityUnits - (spot.VehicleSpots.Sum(vs => (int?)vs.UnitsUsed) ?? 0)
        });

        int unitsNeeded = GetUnitsForVehicle(parkedVehicle.Type);

        // Attempt to assign parking spot(s) based on vehicle type
        switch (parkedVehicle.Type)
        {
            case VehicleType.Motorcycle:
                parkingSpotAssigned = await AssignMotorcycleSpotAsync(parkedVehicle, spotsWithUsage, unitsNeeded);
                break;

            case VehicleType.Car:
                parkingSpotAssigned = await AssignCarSpotAsync(parkedVehicle, spotsWithUsage, unitsNeeded);
                break;

            case VehicleType.Bus:
                parkingSpotAssigned = await AssignBusSpotAsync(parkedVehicle, spotsWithUsage);
                break;

            case VehicleType.Boat:
                parkingSpotAssigned = await AssignBoatSpotAsync(parkedVehicle, spotsWithUsage);
                break;
        }

        if (!parkingSpotAssigned)
        {
            // Remove the vehicle if no parking spot could be assigned
            _context.ParkedVehicle.Remove(parkedVehicle);
            await _context.SaveChangesAsync();

            return new ParkingResult
            {
                Success = false,
                ErrorMessage = "No parking slots available for this vehicle."
            };
        }

        // Save assigned parking spot(s)
        await _context.SaveChangesAsync();

        return new ParkingResult
        {
            Success = true
        };
    }

    private async Task<bool> AssignMotorcycleSpotAsync(ParkedVehicle vehicle, IQueryable<ParkingSpotWithUnits> spotsWithUsage, int unitsNeeded)
    {
        // Fill already used spots first (with 1-2 motorcycles), then fill empty spot
        var mcSpot = spotsWithUsage
            .Where(s => s.FreeUnits >= 1)
            .OrderByDescending(s => s.UsedUnits)
            .ThenBy(s => s.Spot.SpotNumber)
            .FirstOrDefault();

        if (mcSpot != null)
        {
            var vehicleSpot = new VehicleSpot
            {
                ParkedVehicleId = vehicle.Id,
                ParkingSpotId = mcSpot.Spot.Id,
                UnitsUsed = unitsNeeded
            };
            _context.VehicleSpots.Add(vehicleSpot);
            return true;
        }

        return false;
    }

    private async Task<bool> AssignCarSpotAsync(ParkedVehicle vehicle, IQueryable<ParkingSpotWithUnits> spotsWithUsage, int unitsNeeded)
    {
        var carSpot = spotsWithUsage
            .Where(s => s.UsedUnits == 0 && s.Spot.CapacityUnits >= 3)
            .OrderBy(s => s.Spot.SpotNumber)
            .FirstOrDefault();

        if (carSpot != null)
        {
            var vehicleSpot = new VehicleSpot
            {
                ParkedVehicleId = vehicle.Id,
                ParkingSpotId = carSpot.Spot.Id,
                UnitsUsed = unitsNeeded
            };
            _context.VehicleSpots.Add(vehicleSpot);
            return true;
        }

        return false;
    }

    private async Task<bool> AssignBusSpotAsync(ParkedVehicle vehicle, IQueryable<ParkingSpotWithUnits> spotsWithUsage)
    {
        var freeSpots = spotsWithUsage
            .Where(s => s.UsedUnits == 0)
            .OrderBy(s => s.Spot.SpotNumber)
            .Select(s => s.Spot)
            .ToList();

        var consecutiveSpots = FindConsecutiveSpots(freeSpots, 2);

        if (consecutiveSpots != null)
        {
            foreach (var spot in consecutiveSpots)
            {
                _context.VehicleSpots.Add(new VehicleSpot
                {
                    ParkedVehicleId = vehicle.Id,
                    ParkingSpotId = spot.Id,
                    UnitsUsed = 3
                });
            }
            return true;
        }

        return false;
    }

    private async Task<bool> AssignBoatSpotAsync(ParkedVehicle vehicle, IQueryable<ParkingSpotWithUnits> spotsWithUsage)
    {
        var freeSpots = spotsWithUsage
            .Where(s => s.UsedUnits == 0)
            .OrderBy(s => s.Spot.SpotNumber)
            .Select(s => s.Spot)
            .ToList();

        var consecutiveSpots = FindConsecutiveSpots(freeSpots, 3);

        if (consecutiveSpots != null)
        {
            foreach (var spot in consecutiveSpots)
            {
                _context.VehicleSpots.Add(new VehicleSpot
                {
                    ParkedVehicleId = vehicle.Id,
                    ParkingSpotId = spot.Id,
                    UnitsUsed = 3
                });
            }
            return true;
        }

        return false;
    }

    private int GetUnitsForVehicle(VehicleType type)
    {
        return type switch
        {
            VehicleType.Motorcycle => 1,
            VehicleType.Car => 3,
            VehicleType.Bus => 6,
            VehicleType.Boat => 9,
            _ => throw new NotImplementedException($"Vehicle type {type} is not supported.")
        };
    }

    private List<ParkingSpot>? FindConsecutiveSpots(List<ParkingSpot> freeSpots, int requiredSpots)
    {
        for (int i = 0; i <= freeSpots.Count - requiredSpots; i++)
        {
            var slice = freeSpots.Skip(i).Take(requiredSpots).ToList();

            bool consecutive = slice.Select(s => s.SpotNumber)
                .SequenceEqual(Enumerable.Range(slice.First().SpotNumber, requiredSpots));

            if (consecutive)
                return slice;
        }
        return null;
    }
}