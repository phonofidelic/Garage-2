using Garage_2.Interfaces;
using Garage_2.Models;
using System.Text.RegularExpressions;

namespace Garage_2.Services;

public partial class VehicleSearchService : IVehicleSearchService
{
    // w(4) or wheels(4) - search by number of wheels
    [GeneratedRegex(@"^w(?:heels)?\((\d+)\)$", RegexOptions.IgnoreCase)]
    private static partial Regex WheelsPattern();

    // d(2024-12-19) or date(2024-12-19) - search by arrival date
    [GeneratedRegex(@"^d(?:ate)?\((.+)\)$", RegexOptions.IgnoreCase)]
    private static partial Regex DatePattern();

    // t(car) or type(motorcycle) - search by vehicle type
    [GeneratedRegex(@"^t(?:ype)?\((.+)\)$", RegexOptions.IgnoreCase)]
    private static partial Regex TypePattern();

    public IQueryable<T> Search<T>(IQueryable<T> query, string? searchString) where T : class
    {
        if(string.IsNullOrWhiteSpace(searchString))
            return query;

        var search = searchString.Trim();

        if(typeof(T) == typeof(ParkedVehicle))
        {
            var vehicleQuery = query as IQueryable<ParkedVehicle>;

            // Check for special command patterns first
            var wheelsMatch = WheelsPattern().Match(search);
            if (wheelsMatch.Success && int.TryParse(wheelsMatch.Groups[1].Value, out var wheels))
            {
                return (IQueryable<T>)vehicleQuery!.Where(v => v.NumberOfWheels == wheels);
            }

            var dateMatch = DatePattern().Match(search);
            if (dateMatch.Success && DateTime.TryParse(dateMatch.Groups[1].Value, out var date))
            {
                return (IQueryable<T>)vehicleQuery!.Where(v => v.ArrivalTime.Date == date.Date);
            }

            var typeMatch = TypePattern().Match(search);
            if (typeMatch.Success)
            {
                var typeSearch = typeMatch.Groups[1].Value;
                var matchingTypes = Enum.GetValues<VehicleType>()
                    .Where(t => t.ToString().Contains(typeSearch, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                return (IQueryable<T>)vehicleQuery!.Where(v => matchingTypes.Contains(v.Type));
            }

            var filtered = vehicleQuery!.Where(v =>
                v.RegistrationNumber.Contains(search) ||
                v.Make.Contains(search) ||
                v.Model.Contains(search) ||
                v.Color.Contains(search)
            );

            return (IQueryable<T>)filtered;
        }

        return query;
    }
}
