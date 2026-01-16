using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models.ViewModels.ParkingSessions
{
    public class ParkingSessionsListItemViewModel
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }

        public string VehicleType { get; set; } = default!;

        public string RegistrationNumber { get; set; } = default!;

        [Display(Name = "Start time")]
        public DateTime ArrivalTime { get; set; }

        [Display(Name = "Parked time")]
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh\\:mm}")]
        public TimeSpan? ParkedTime => (DepartureTime ?? DateTime.Now) - ArrivalTime;
        
        [Display(Name = "Checkout time")]
        public DateTime? DepartureTime { get; set; }

        [Display(Name = "Total cost")]
        [DataType(DataType.Currency)]
        public decimal TotalCost { get; set; }
    }
}
