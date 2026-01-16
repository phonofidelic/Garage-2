using Garage_2.Interfaces;
using Garage_2.Models.Entities;
using Garage_2.Models.ViewModels.ParkingSessions;
using Microsoft.Extensions.Options;

namespace Garage_2.Services
{
    public class ParkingSessionService : IParkingSessionService
    {
        private readonly GarageConfig _config;
        private readonly ILogger<ParkingSessionService> _logger;
        public ParkingSessionService(
            IOptions<GarageConfig> config, 
            ILogger<ParkingSessionService> logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        public decimal GetTotalParkingSessionCost(ParkingSession parkingSession, DateTime currentTime)
        {
            var totalParkingTime = (parkingSession.DepartureTime ?? currentTime) - parkingSession.ArrivalTime;
            int unitsUsed = parkingSession.VehicleParkings.Sum(vp => vp.UnitsUsed);
            decimal sizeMultiplier = unitsUsed / 3m;
            decimal currentPrice = (decimal)Math.Ceiling(totalParkingTime.TotalHours) * _config.PricePerHour * sizeMultiplier;

            return currentPrice;
        }
    }
}
