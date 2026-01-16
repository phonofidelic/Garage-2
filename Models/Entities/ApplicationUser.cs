using Microsoft.AspNetCore.Identity;

namespace Garage_2.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = "";

        public string LastName { get; set; } = ""!;

        public string SSN { get; set; } = "";

        // Nav-prop
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
