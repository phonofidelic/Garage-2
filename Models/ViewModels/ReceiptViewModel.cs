namespace Garage_2.Models.ViewModels
{
    public class ReceiptViewModel
    {
        public string RegistrationNumber { get; set; }
        public VehicleType Type { get; set; }

        public DateTime ArrivalTime { get; set; }
        public DateTime CheckoutTime { get; set; }

        public TimeSpan ParkingDuration { get; set; }

        public decimal Price { get; set; }
    }
}
