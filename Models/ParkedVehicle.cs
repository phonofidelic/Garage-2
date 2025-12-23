using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models
{
    public class ParkedVehicle
    {

        private string _registrationNumber = default!;

        public int Id { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6)]
        [RegularExpression(@"^[A-Za-z0-9]{6}$", ErrorMessage = "Registration number must be exactly 6 alphanumeric characters (A-Z, 0-9).")]
        [Display(Name = "Registration number")]
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

        [Required]
        [Display(Name = "Arrival time")]
        public DateTime ArrivalTime { get; set; }

        [Required]
        public VehicleType Type { get; set; }

        // 1:M relation till p-platser via en join-tabell
        public ICollection<VehicleSpot> VehicleSpots { get; set; } = new List<VehicleSpot>();

    }
}
