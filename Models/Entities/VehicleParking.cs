using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models.Entities
{
    public class VehicleParking
    {
        public int Id { get; set; }

        // FK
        public int ParkingSessionId { get; set; }

        // Nav-prop
        public ParkingSession ParkingSession { get; set; } = default!;

        //FK
        public int ParkingSpotV2Id { get; set; }

        // Nav-prop
        public ParkingSpotV2 ParkingSpot { get; set; } = default!;

        // 1...3 (tredjedelar av en parking spot)
        [Range(1, 3)]
        public int UnitsUsed { get; set; }
    }
}
