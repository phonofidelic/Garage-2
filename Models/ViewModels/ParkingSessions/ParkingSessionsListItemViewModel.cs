using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models.ViewModels.ParkingSessions
{
    public class ParkingSessionsListItemViewModel
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }

        [Display(Name = "Vehicle type")]
        public string VehicleType { get; set; } = default!;

        [Display(Name = "Vehicle owner")]
        public string VehicleOwner { get; set; } = string.Empty;

        [Display(Name = "Registration number")]
        public string RegistrationNumber { get; set; } = default!;

        [Display(Name = "Start time")]
        public DateTime ArrivalTime { get; set; }

        [Display(Name = "Parked time")]
        [DataType(DataType.Duration)]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm}")]
        public TimeSpan? ParkedTime => (DepartureTime ?? DateTime.Now) - ArrivalTime;
        
        [Display(Name = "Checkout time")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(NullDisplayText = "-")]
        public DateTime? DepartureTime { get; set; }

        [Display(Name = "Current")]
        [DataType(DataType.Currency)]
        public decimal CurrentCost { get; set; }
    }
}
