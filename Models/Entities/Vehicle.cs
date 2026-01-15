namespace Garage_2.Models.Entities
{
    public class Vehicle
    {
        public int Id { get; set; }

        public string RegistrationNumber { get; set; }

        public string Make { get; set; }

        public string Model { get; set; }

        public int NumberOfWheels { get; set; }

        public int VehicleTypeId { get; set; }
        public VehicleType Type { get; set; }

        public int ApplicationUserId { get; set; }
        public ApplicationUser User { get; set; }

        public ICollection<ParkingSession> ParkingSessions{ get; set; }
    }
}
