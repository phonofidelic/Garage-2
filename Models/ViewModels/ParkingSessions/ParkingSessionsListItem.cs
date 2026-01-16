namespace Garage_2.Models.ViewModels.ParkingSessions
{
    public class ParkingSessionsListItem
    {
        public int Id { get; set; }

        public int VehicleId { get; set; }

        public DateTime ArrivalTime { get; set; }

        public DateTime? DepartureTime { get; set; }

        public TimeSpan? Duration => DepartureTime - ArrivalTime;
    }
}
