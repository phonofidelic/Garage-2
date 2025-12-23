using System.ComponentModel;

namespace Garage_2.Models.ViewModels
{
    public class GarageStatisticsViewModel
    {
        [DisplayName("Total number of vehicles")]
        public int TotalVehicles { get; set; }
        [DisplayName("Total number of wheels")]
        public int TotalWheels { get; set; }
        [DisplayName("Total revenue")]
        public decimal TotalRevenue { get; set; }

        [DisplayName("Vehicle types")]
        public List<VehicleTypeCountViewModel>? VehiclesPerType { get; set; }
    }

}
