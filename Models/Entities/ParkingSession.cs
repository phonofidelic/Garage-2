namespace Garage_2.Models.Entities
{
    public class ParkingSession
    {
        public int Id { get; set; }

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }

        public DateTime ArrivalTime { get; set; }

        public DateTime DepartureTime { get; set; }

        public TimeSpan Duration { get => DepartureTime - ArrivalTime; }

        public ICollection<VehicleParking> VehicleParkings { get; set; }

        public ICollection<ParkingSpotV2> ParkingSpots { get; set; }
    }
}
