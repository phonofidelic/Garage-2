namespace Garage_2.Models.ViewModels
{
    public class DetailsViewModel
    {
        public ParkedVehicle Vehicle { get; }
        public IReadOnlyList<VehicleSpot> VehicleSpots { get; }
        public DetailsViewModel(ParkedVehicle vehicle, List<VehicleSpot> vehicleSpots)
        {
            Vehicle = vehicle;
            VehicleSpots = vehicleSpots;
        }
    }
}
