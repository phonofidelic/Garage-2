namespace Garage_2.Models.ViewModels
{
    public class DetailsViewModel
    {
        public ParkedVehicle Vehicle { get; }

        public DetailsViewModel(ParkedVehicle parkedVehicle)
        {
            Vehicle = parkedVehicle;
        }
    }
}
