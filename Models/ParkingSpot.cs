using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models
{
    public class ParkingSpot
    {
        public int Id { get; set; }

        // Obs! Hårkodad maxkapacitet av antalet parkeringsplatser här
        // Todo: Fixa detta?
        [Range(1, 100)]
        public int SpotNumber { get; set; }

        // Varje spot har en kapacitet av 3 units.
        // Har alltid kapaciteten 3, men används för uträkningar i koden. 
        public int CapacityUnits { get; set; } = 3;

        // 1:M relation med vehicleSpots-tabellen
        public ICollection<VehicleSpot> VehicleSpots { get; set; } = new List<VehicleSpot>();
    }
}
