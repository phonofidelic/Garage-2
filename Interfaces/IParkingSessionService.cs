using Garage_2.Models.Entities;
using Garage_2.Models.ViewModels.ParkingSessions;

namespace Garage_2.Interfaces
{
    public interface IParkingSessionService
    {
        decimal GetTotalParkingSessionCost(ParkingSession parkingSession, DateTime currentTime);
    }
}
