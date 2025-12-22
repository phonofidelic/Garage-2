using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models.ViewModels
{
    public class ReceiptViewModel
    {
        [Display(Name = "Registration number")]
        public string RegistrationNumber { get; set; }
        public VehicleType Type { get; set; }

        [Display(Name = "Arrival time")]
        public DateTime ArrivalTime { get; set; }
        [Display(Name = "Checkout time")]
        public DateTime CheckoutTime { get; set; }

        [Display(Name = "Parking duration")]
        public TimeSpan ParkingDuration { get; set; }

        public decimal Price { get; set; }

        public List<int> ParkingSpots { get; set; } = new();
    }
}
