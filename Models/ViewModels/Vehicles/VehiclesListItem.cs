using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models.ViewModels.Vehicles
{
    public class VehiclesListItem
    {
        public int Id { get; set; }

        [Display(Name = "Registration number")]
        public string RegistrationNumber { get; set; } = default!;
        
        public string Type { get; set; } = default!;
    }
}