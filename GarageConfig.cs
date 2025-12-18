using Humanizer;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Garage_2
{
    public class GarageConfig
    {
        // As IServiceCollection.AddOptions() in Program.cs ignores the
        // [Required] annotation (or have I missed something?) the PricePerHour
        // property is initialized to -10.0 so at least the [Range...]
        // annotation is triggered in case the option is missing from the
        // appsettings.json file.
        [Range(0.0, Double.PositiveInfinity, ErrorMessage = "The field {0} may not be negative.")]
        [Required]
        public decimal PricePerHour { get; set; } = -10.0m;
    }
}
