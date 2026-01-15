namespace Garage_2.Models.Entities
{
    public class VehicleParking
    {
        public int ParkingSessionId { get; set; }
        public ParkingSession ParkingSession { get; set; }

        public int ParkingSpotId { get; set; }
        public ParkingSpotV2 ParkingSpot { get; set; }

        public int UnitsUsed { get; set; }
    }
}
