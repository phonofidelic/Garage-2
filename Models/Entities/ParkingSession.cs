using System.ComponentModel.DataAnnotations.Schema;

namespace Garage_2.Models.Entities
{
    public class ParkingSession
    {
        public int Id { get; set; }

        public int VehicleId { get; set; }

        // Nav-prop
        public Vehicle Vehicle { get; set; } = default!;

        public DateTime ArrivalTime { get; set; }

        // Null = aktiv parkering
        public DateTime? DepartureTime { get; set; }

        [NotMapped]
        public TimeSpan? Duration => DepartureTime - ArrivalTime;

        public ICollection<VehicleParking> VehicleParkings { get; set; } = new List<VehicleParking>();

    }
}
