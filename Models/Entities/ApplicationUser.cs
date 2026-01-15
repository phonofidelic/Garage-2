using Microsoft.AspNetCore.Identity;

namespace Garage_2.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string SSN { get; set; }
    }
}
