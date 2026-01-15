namespace Garage_2.Models.Entities
{
    public class ParkingSession
    {
        public int Id { get; set; }

        public int VehicleId { get; set; }

        public DateTime ArrivalTime { get; set; }

        public DateTime DepartureTime { get; set; }

        public TimeSpan Duration { get => DepartureTime - ArrivalTime; }

        // ToDo: VehicleParkings
    }
}
