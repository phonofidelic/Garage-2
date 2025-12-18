using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models.ViewModels
{
    public class ParkNewVehicleViewModel
    {
        private string _registrationNumber = default!;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        [RegularExpression(@"^[A-Za-z0-9]{6}$", ErrorMessage = "Registration number must be exactly 6 alphanumeric characters (A-Z, 0-9).")]
        [Display(Name = "Registration Number")]
        public string RegistrationNumber
        {
            get => _registrationNumber;
            set => _registrationNumber = value?.ToUpper() ?? default!;
        }

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

        [Display(Name = "Arrival time")]
        public DateTime ArrivalTime { get; set; }

        [Required]
        public VehicleType Type { get; set; }
    }
}
