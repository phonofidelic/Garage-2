using System.ComponentModel.DataAnnotations;

public class ReceiptViewModel
{
    [Display(Name = "Registration number")]
    public string RegistrationNumber { get; set; } = default!;

    public string Type { get; set; } = default!;

    [Display(Name = "Arrival time")]
    public DateTime ArrivalTime { get; set; }

    [Display(Name = "Checkout time")]
    public DateTime CheckoutTime { get; set; }

    [Display(Name = "Parking duration")]
    public TimeSpan ParkingDuration { get; set; }

    public decimal Price { get; set; }

    public List<int> ParkingSpots { get; set; } = new();
}

