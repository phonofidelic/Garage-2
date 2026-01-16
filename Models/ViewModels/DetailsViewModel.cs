using Garage_2.Models.Entities;


namespace Garage_2.Models.ViewModels
{
    public class DetailsViewModel
    {
        public Vehicle Vehicle { get; }
        public ParkingSession? ActiveSession { get; }

        public DetailsViewModel(Vehicle vehicle, ParkingSession? activeSession)
        {
            Vehicle = vehicle;
            ActiveSession = activeSession;
        }

        // Hjälp-properties så vyn blir enkel
        public bool IsParked => ActiveSession is not null;

        public IReadOnlyList<VehicleParking> CurrentParkings =>
            ActiveSession?.VehicleParkings?.ToList() ?? new List<VehicleParking>();
    }

}
