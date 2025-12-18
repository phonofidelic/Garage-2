namespace Garage_2.Models.ViewModels
{
    public class OverviewViewModel
    {

        public int Id { get; set; }

        public VehicleType Type { get; set; }

        public string RegistrationNUmber { get; set; }

        public DateTime ArrivalTime { get; set; }

        public TimeSpan ParkedTime { get; set; }

    }
}
