namespace Garage_2.Models.Entities
{
    public class VehicleType
    {
        public int Id { get; set; }

        public string Name { get; set; } = default!;

        public int SizeInUnits { get; set; }

        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
