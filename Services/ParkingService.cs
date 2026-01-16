using Garage_2.Data;
using Garage_2.Interfaces;
using Garage_2.Models;
using Garage_2.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Garage_2.Services;

public class ParkingService : IParkingService
{
    private readonly GarageContext _context;

    public ParkingService(GarageContext context)
    {
        _context = context;
    }

    public async Task<ParkingResult> ParkVehicleAsync(Vehicle vehicle)
    {
        // Lägger in Vehicle i ParkingSession dvs startar aktiv parkering.


        if (vehicle.Id == 0)
        {
            return new ParkingResult { Success = false, ErrorMessage = "Fordonet är ej registrerat!" };
        }

        // Kolla att VehicleType finns på fordonet
        vehicle = await _context.Vehicles.Include(v => v.VehicleType).FirstOrDefaultAsync(v => v.Id == vehicle.Id);

        if (vehicle is null)
        {
            return new ParkingResult { Success = false, ErrorMessage = "Fordonet kunde ej hittas!" };
        }

        if (vehicle.VehicleType is null)
        {
            return new ParkingResult { Success = false, ErrorMessage = "Fordonstyp saknas!" };
        }

        // Om det finns en aktiv session för detta fordon är det redan parkerat
        bool hasActiveSession = await _context.ParkingSessions.AnyAsync(ps => ps.VehicleId == vehicle.Id && ps.DepartureTime == null);

        if (hasActiveSession)
        {
            return new ParkingResult { Success = false, ErrorMessage = "Fordonet är redan parkerat!" };
        }

        // Fordon ok att parkera - Skapa ny ParkingSession
        var parkingSession = new ParkingSession
        {
            VehicleId = vehicle.Id,
            ArrivalTime = DateTime.Now,
            DepartureTime = null
        };

        // Transaktion: ny rad i parkingSession + koll av parkeringsplats måste hänga ihop i en sammanhängande operation
        // Annars kan man få en aktiv parkering utan plats, halv plats osv, eller två parkeringar som lyckas samtidigt av två användare

        await using var tx = await _context.Database.BeginTransactionAsync();

        try
        {
            _context.ParkingSessions.Add(parkingSession);
            await _context.SaveChangesAsync(); // behövs för att få session.Id

            // Beläggning per spot ska baseras på aktiva ParkingSessions
            // Bygg en query som räknar nuvarande beläggning per parkeringsplats (bara aktiva)
            IQueryable<ParkingSpotWithUnitsV2> spotsWithUsage = _context.ParkingSpotV2
                .Select(
                spot => new ParkingSpotWithUnitsV2
                {
                    Spot = spot,
                    UsedUnits = spot.VehicleParkings.Where(vp => vp.ParkingSession.DepartureTime == null).Sum(vp => (int?)vp.UnitsUsed) ?? 0
                })
                .Select(x => new ParkingSpotWithUnitsV2 // .Select() på .Select() återanvänder resultatet från första queryn (dvs x)
                {
                    Spot = x.Spot,
                    UsedUnits = x.UsedUnits,
                    FreeUnits = x.Spot.CapacityUnits - x.UsedUnits
                });

            int unitsNeeded = vehicle.VehicleType.SizeInUnits;

            // _context.VehicleParkings.Add körs i Assign-metoderna nedan
            bool parkingSpotAssigned = vehicle.VehicleType.Name switch
            {
                "Motorcycle" => await AssignMotorcycleSpotAsync(parkingSession, spotsWithUsage, unitsNeeded),
                "Car" => await AssignCarSpotAsync(parkingSession, spotsWithUsage, unitsNeeded),
                "Bus" => await AssignBusSpotAsync(parkingSession, spotsWithUsage),
                "Boat" => await AssignBoatSpotAsync(parkingSession, spotsWithUsage),
                _ => throw new NotImplementedException($"Fordonstyp '{vehicle.VehicleType.Name}' saknar stöd i systemet!")
            };

            if (!parkingSpotAssigned)
            {
                // rulla tillbaka hela sessionen
                await tx.RollbackAsync();

                return new ParkingResult
                {
                    Success = false,
                    ErrorMessage = "Inga parkeringsplatser tillgängliga för detta fordon!"
                };
            }

            await _context.SaveChangesAsync();  // Både tillagda rader i ParkingSessions och VehicleParkings sparas här
            await tx.CommitAsync();

            return new ParkingResult { Success = true };
        }
        catch (DbUpdateException)
        {
            // här hamnar du t.ex. om filtered unique index triggas (race condition)
            await tx.RollbackAsync();
            return new ParkingResult { Success = false, ErrorMessage = "Parkeringen misslyckades pga en konflikt. Försök igen." };
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    private Task<bool> AssignMotorcycleSpotAsync(ParkingSession session, IQueryable<ParkingSpotWithUnitsV2> spotsWithUsage, int unitsNeeded)
    {
        // Fyll först och främst MC på p-platser med andra MC (UsedUnits=2 väljs före 1, som väljs före 0) 
        var mcSpot = spotsWithUsage
            .Where(s => !s.Spot.IsBlocked && s.FreeUnits >= 1)
            .OrderByDescending(s => s.UsedUnits)
            .ThenBy(s => s.Spot.SpotNumber)
            .FirstOrDefault();

        if (mcSpot is null) return Task.FromResult(false);

        _context.VehicleParkings.Add(new VehicleParking
        {
            ParkingSessionId = session.Id,
            ParkingSpotV2Id = mcSpot.Spot.Id,
            UnitsUsed = unitsNeeded // 1 för MC
        });

        return Task.FromResult(true);
    }

    private Task<bool> AssignCarSpotAsync(ParkingSession session, IQueryable<ParkingSpotWithUnitsV2> spotsWithUsage, int unitsNeeded)
    {
        var carSpot = spotsWithUsage
            .Where(s => !s.Spot.IsBlocked && s.UsedUnits == 0 && s.Spot.CapacityUnits >= 3)
            .OrderBy(s => s.Spot.SpotNumber)
            .FirstOrDefault();

        if (carSpot is null) return Task.FromResult(false);

        _context.VehicleParkings.Add(new VehicleParking
        {
            ParkingSessionId = session.Id,
            ParkingSpotV2Id = carSpot.Spot.Id,
            UnitsUsed = unitsNeeded // 3 för bil
        });

        return Task.FromResult(true);
    }

    private Task<bool> AssignBusSpotAsync(ParkingSession session, IQueryable<ParkingSpotWithUnitsV2> spotsWithUsage)
    {
        var freeSpots = spotsWithUsage
            .Where(s => !s.Spot.IsBlocked && s.UsedUnits == 0)
            .OrderBy(s => s.Spot.SpotNumber)
            .Select(s => s.Spot)
            .ToList();

        var consecutiveSpots = FindConsecutiveSpots(freeSpots, requiredSpots: 2);
        if (consecutiveSpots is null) return Task.FromResult(false);

        foreach (var spot in consecutiveSpots)
        {
            _context.VehicleParkings.Add(new VehicleParking
            {
                ParkingSessionId = session.Id,
                ParkingSpotV2Id = spot.Id,
                UnitsUsed = 3
            });
        }

        return Task.FromResult(true);
    }

    private Task<bool> AssignBoatSpotAsync(ParkingSession session, IQueryable<ParkingSpotWithUnitsV2> spotsWithUsage)
    {
        var freeSpots = spotsWithUsage
            .Where(s => !s.Spot.IsBlocked && s.UsedUnits == 0)
            .OrderBy(s => s.Spot.SpotNumber)
            .Select(s => s.Spot)
            .ToList();

        var consecutiveSpots = FindConsecutiveSpots(freeSpots, requiredSpots: 3);
        if (consecutiveSpots is null) return Task.FromResult(false);

        foreach (var spot in consecutiveSpots)
        {
            _context.VehicleParkings.Add(new VehicleParking
            {
                ParkingSessionId = session.Id,
                ParkingSpotV2Id = spot.Id,
                UnitsUsed = 3
            });
        }

        return Task.FromResult(true);
    }

    private static List<ParkingSpotV2>? FindConsecutiveSpots(List<ParkingSpotV2> freeSpots, int requiredSpots)
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

    // Intern klass för beläggningsberäkning (ParkingSpotV2 + units)
    private sealed class ParkingSpotWithUnitsV2
    {
        public ParkingSpotV2 Spot { get; set; } = default!;
        public int UsedUnits { get; set; }
        public int FreeUnits { get; set; }
    }
}
