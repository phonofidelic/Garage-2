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
            int totalVehicles = _context.ParkedVehicle.Count();
            int totalWheels = _context.ParkedVehicle.Sum(v => v.NumberOfWheels);

            var vehiclesGroupedByTypeDict = _context.ParkedVehicle.GroupBy(v => v.Type).Select(g => new { Type = g.Key, Count = g.Count() }).ToList();

            List<VehicleTypeCountViewModel> vehiclesPerTypeList = new List<VehicleTypeCountViewModel>();

            foreach (var vehicleType in vehiclesGroupedByTypeDict)
            {
                vehiclesPerTypeList.Add(new VehicleTypeCountViewModel() { Count = vehicleType.Count, Type = vehicleType.Type });
            }

            GarageStatisticsViewModel GarageStatsVM = new GarageStatisticsViewModel
            {
                TotalRevenue = 1000000,
                TotalVehicles = totalVehicles,
                TotalWheels = totalWheels,
                VehiclesPerType = vehiclesPerTypeList
            };

            return View(GarageStatsVM);
        }
    }
}
