using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models
{
    // Join-tabell: Vilka platser ett fordon använder, dvs vem står var?
    public class VehicleSpot
    {
        public int Id { get; set; }

        // Främmande nyckel från fordonstabellen
        public int ParkedVehicleId { get; set; }

        // Navigation-property från fordonstabellen
        public ParkedVehicle ParkedVehicle { get; set; } = default!;


        // Främmande nyckel från p-platstabellen
        public int ParkingSpotId { get; set; }

        // Nav-property från p-platstabellen
        public ParkingSpot ParkingSpot { get; set; } = default!;

        // Hur många 'enheter' (typ 'tredjedelar') av en p-plats som upptas - 1, 2 eller 3
        // Behövs eftersom varje spot kan ta 3 motorcyklar och därför är uppdelad i tredjedelar
        [Range(1, 3)]
        public int UnitsUsed { get; set; }
    }
}
