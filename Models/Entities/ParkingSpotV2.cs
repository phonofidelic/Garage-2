namespace Garage_2.Models.Entities
{
    public class ParkingSpotV2
    {
        public int Id { get; set; }

        public int SpotNumber { get; set; }

        public int CapacityUnits { get; set; }

        public bool IsBlocked { get; set; }

        public ICollection<VehicleParking> VehicleParkings { get; set; }

        public ICollection<ParkingSession> ParkingSessions{ get; set; }
    }
}
