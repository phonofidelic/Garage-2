using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models.ViewModels
{
    public class ParkedVehicleEditViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(6, MinimumLength = 6)]
        public string RegistrationNumber { get; set; } = default!;

        [Required, StringLength(100)]
        public string Make { get; set; } = default!;

        [Required, StringLength(100)]
        public string Model { get; set; } = default!;

        [Required, Range(0, 22)]
        public int NumberOfWheels { get; set; }

        [Required, StringLength(100)]
        public string Color { get; set; } = default!;

        // Visible in edit view, but can not be changed.
        public DateTime ArrivalTime { get; set; }

        [Required]
        public VehicleType Type { get; set; }
    }
}
