using Garage_2.Data;
using Garage_2.Interfaces;
using Garage_2.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Garage_2.Controllers
{
    public class GarageStatisticsController : Controller
    {
        private readonly GarageContext _context;
        private readonly GarageConfig _config;
        private readonly IVehicleSearchService _searchService;

        public GarageStatisticsController(GarageContext context, IOptions<GarageConfig> config, IVehicleSearchService searchService)
        {
            _context = context;
            _config = config.Value;
            _searchService = searchService;
        }

        public IActionResult GarageStatsOverview()
        {
            int totalVehicles = _context.Vehicles.Count();
            int totalWheels = _context.Vehicles.Sum(v => v.NumberOfWheels);

            var vehiclesGroupedByTypeDict = _context.Vehicles.GroupBy(v => v.VehicleType.Name).Select(g => new { Type = g.Key, Count = g.Count() }).ToList();

            List<VehicleTypeCountViewModel> vehiclesPerTypeList = new List<VehicleTypeCountViewModel>();

            foreach (var vehicleType in vehiclesGroupedByTypeDict)
            {
                vehiclesPerTypeList.Add(new VehicleTypeCountViewModel() { Count = vehicleType.Count, Type = vehicleType.Type });
            }

            DateTime now = DateTime.Now;

            // Pull only the time and units for each vehicle
            var vehicleDataList = _context.ParkingSessions.Select(ps => new
            {
                ps.ArrivalTime,
                Units = ps.VehicleParkings.Sum(vp => vp.UnitsUsed)
            }).ToList();

            decimal totalRevenue = 0;

            foreach (var item in vehicleDataList)
            {
                TimeSpan duration = now - item.ArrivalTime;

                double hours = duration.TotalHours;

                // Size Multiplier (3 units = 1 spot)
                double sizeMultiplier = item.Units / 3.0;

                totalRevenue += (decimal)(hours * (double)_config.PricePerHour * sizeMultiplier);
            }

            //DateTime now = DateTime.Now;
            var arrivalTimes = _context.ParkingSessions.Select(p => p.ArrivalTime).ToList();

            double totalHours = 0;

            foreach (var arrival in arrivalTimes)
            {
                TimeSpan duration = now - arrival;
                totalHours += duration.TotalHours;
            }

            totalRevenue = (decimal)totalHours * _config.PricePerHour;

            // Round to closest int because otherwise too long
            totalRevenue = Math.Round(totalRevenue);

            GarageStatisticsViewModel GarageStatsVM = new GarageStatisticsViewModel
            {
                TotalRevenue = totalRevenue,
                TotalVehicles = totalVehicles,
                TotalWheels = totalWheels,
                VehiclesPerType = vehiclesPerTypeList
            };

            return View(GarageStatsVM);
        }
    }
}
