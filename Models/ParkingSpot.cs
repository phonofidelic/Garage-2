using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models
{
    public class ParkingSpot
    {
        public int Id { get; set; }

        // Obs! Hårkodat maxvärde på p-platsnummer här (1000)
        // Todo: Fixa detta?
        [Range(1, 1000)]
        public int SpotNumber { get; set; }

        // Varje spot har en kapacitet av 3 units.
        // Värdet är konstant, men används för beräkningar.
        public int CapacityUnits { get; set; } = 3;

        // Navigation-property för vilket fordon som använder denna parkeringsplats (via VehicleSpot).
        // 1-3 st MC kan vara kopplade till samma ParkingSpot, därför 1:M relation mellan ParkingSpot och VehicleSpot 
        public ICollection<VehicleSpot> VehicleSpots { get; set; } = new List<VehicleSpot>();
    }
}
