using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models.ViewModels
{
    public class EditVehicleViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(6, MinimumLength = 6)]
        [RegularExpression(@"^[A-Za-z0-9]{6}$")]
        [Display(Name = "Registration number")]
        public string RegistrationNumber { get; set; } = default!;

        [Required, StringLength(50)]
        [RegularExpression(@"^[\w\-\.\s]+$")]
        public string Make { get; set; } = default!;

        [Required, StringLength(50)]
        [RegularExpression(@"^[\w\-\.\s]+$")]
        public string Model { get; set; } = default!;

        [Required, Range(0, 22)]
        [Display(Name = "Number of wheels")]
        public int NumberOfWheels { get; set; }

        [Required, StringLength(25)]
        [RegularExpression(@"^[\w\s]+$")]
        public string Color { get; set; } = default!;

        // Read-only info
        public string VehicleTypeName { get; set; } = default!;
        [Display(Name = "Arrival time")]
        public DateTime? ArrivalTime { get; set; }
    }
}

