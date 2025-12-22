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

            DateTime now = DateTime.Now;
            var arrivalTimes = _context.ParkedVehicle.Select(v => v.ArrivalTime).ToList();

            double totalHours = 0;

            foreach (var arrival in arrivalTimes)
            {
                TimeSpan duration = now - arrival;
                totalHours += duration.TotalHours;
            }

            decimal totalRevenue = (decimal)totalHours * _config.PricePerHour;

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
