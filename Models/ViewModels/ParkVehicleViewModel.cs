using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models.ViewModels
{
    public class ParkVehicleViewModel
    {
        [Required]
        [StringLength(6, MinimumLength = 6)]
        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; } = default!;

        [Required]
        [StringLength(100)]
        public string Make { get; set; } = default!;

        [Required]
        [StringLength(100)]
        public string Model { get; set; } = default!;

        [Required]
        [Range(0, 22)]
        [Display(Name = "Number of wheels")]
        public int NumberOfWheels { get; set; }

        [Required]
        [StringLength(100)]
        public string Color { get; set; } = default!;


        [Required]
        public VehicleType Type { get; set; }
    }
}
