using Microsoft.AspNetCore.Identity;

namespace Garage_2.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = default!;

        public string LastName { get; set; } = default!;

        public string SSN { get; set; } = default!;

        // Nav-prop
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
