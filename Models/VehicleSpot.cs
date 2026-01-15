using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models
{
    // Join-entitet som beskriver vilka parkeringsplatser ett fordon upptar (och hur mycket av varje plats).
    public class VehicleSpot
    {
        public int Id { get; set; }

        // Främmande nyckel från fordonstabellen
        public int ParkedVehicleId { get; set; }

        // Navigation-property till fordonet som använder denna parkeringsplats.
        // Ett fordon kan ha flera VehicleSpots (Buss, Båt), dvs 1:M 
        public ParkedVehicle ParkedVehicle { get; set; } = default!;

        // Främmande nyckel från p-platstabellen
        public int ParkingSpotId { get; set; }

        // Navigation-property till p-platsen som denna VehicleSpot-rad avser.
        // En ParkingSpot kan ha flera VehicleSpot-rader (1-3 för MC), dvs 1:M
        public ParkingSpot ParkingSpot { get; set; } = default!;

        // Anger hur stor andel av parkeringsplatsen som relaterat fordon upptar.
        // En p-plats är uppdelad i 3 enheter (Range 1,3): 1 = 1/3 plats (MC). 3 = hel plats (bil).
        // En ParkingSpot kan därför ha 1-3 VehicleSpot-rader för MC, med UnitsUsed=1 vardera.
        [Range(1, 3)]
        public int UnitsUsed { get; set; }
    }
}
