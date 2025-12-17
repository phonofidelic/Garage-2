using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models
{
    public class ParkedVehicle
    {
        public int Id { get; set; }
        [StringLength(6)]
        public string RegistrationNumber { get; set; } = default!;
        public string Make { get; set; } = default!;
        public string Model { get; set; } = default!;
        public int NumberOfWheels { get; set; }
        public string Color { get; set; } = default!;
        public DateTime ArrivalTime { get; set; }
        public VehicleType Type { get; set; }

    }
}
