namespace Garage_2.Models.Entities
{
    public class Vehicle
    {
        public int Id { get; set; }

        public string RegistrationNumber { get; set; } = default!;

        public string Make { get; set; } = default!;

        public string Model { get; set; } = default!;

        public int NumberOfWheels { get; set; }

        public string Color { get; set; } = default!;

        public int VehicleTypeId { get; set; }
        public VehicleType VehicleType { get; set; } = default!;

        public string ApplicationUserId { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;

        public ICollection<ParkingSession> ParkingSessions { get; set; } = new List<ParkingSession>();
    }
}
