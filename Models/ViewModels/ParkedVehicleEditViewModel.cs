using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models.ViewModels
{
    public class ParkedVehicleEditViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(6, MinimumLength = 6)]
        [Display(Name = "Registration number")]
        public string RegistrationNumber { get; set; } = default!;

        [Required, StringLength(100)]
        public string Make { get; set; } = default!;

        [Required, StringLength(100)]
        public string Model { get; set; } = default!;

        [Required, Range(0, 22)]
        [Display(Name = "Number of wheels")]
        public int NumberOfWheels { get; set; }

        [Required, StringLength(100)]
        public string Color { get; set; } = default!;

        // Visible in edit view, but can not be changed.
        [Display(Name = "Arrival time")]
        public DateTime ArrivalTime { get; set; }

        [Required]
        public VehicleType Type { get; set; }
    }
}
