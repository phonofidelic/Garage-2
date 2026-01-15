using Garage_2.Interfaces;
using Garage_2.Models;
using System.Text.RegularExpressions;

namespace Garage_2.Services;

public class VehicleSearchService : IVehicleSearchService
{
    public IQueryable<T> Search<T>(IQueryable<T> query, string? searchString, string? searchField = null) where T : class
    {
        if (string.IsNullOrWhiteSpace(searchString))
            return query;

        var search = searchString.Trim();

        if (typeof(T) == typeof(ParkedVehicle))
        {
            var vehicleQuery = query as IQueryable<ParkedVehicle>;

            if (!string.IsNullOrEmpty(searchField))
            {
                return searchField.ToLower() switch
                {
                    "wheels" => (IQueryable<T>)FilterByWheels(vehicleQuery!, search),
                    "type" => (IQueryable<T>)FilterByType(vehicleQuery!, search),
                    "date" => (IQueryable<T>)FilterByDate(vehicleQuery!, search),
                    _ => (IQueryable<T>)FilterAllFields(vehicleQuery!, search)
                };
            }

            // Default: search across all text fields
            return (IQueryable<T>)FilterAllFields(vehicleQuery!, search);
        }

        return query;
    }

    private IQueryable<ParkedVehicle> FilterByWheels(IQueryable<ParkedVehicle> query, string search)
    {
        if (int.TryParse(search, out var wheels))
        {
            return query.Where(v => v.NumberOfWheels == wheels);
        }
        // If parse fails, return no results
        return query.Where(v => false);
    }

    private IQueryable<ParkedVehicle> FilterByType(IQueryable<ParkedVehicle> query, string search)
    {
        var matchingTypes = Enum.GetValues<VehicleType>()
            .Where(t => t.ToString().Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return query.Where(v => matchingTypes.Contains(v.Type));
    }

    private IQueryable<ParkedVehicle> FilterByDate(IQueryable<ParkedVehicle> query, string search)
    {
        if (DateTime.TryParse(search, out var date))
        {
            return query.Where(v => v.ArrivalTime.Date == date.Date);
        }
        // If parse fails, return no results
        return query.Where(v => false);
    }

    private IQueryable<ParkedVehicle> FilterAllFields(IQueryable<ParkedVehicle> query, string search)
    {
        return query.Where(v =>
            v.RegistrationNumber.Contains(search) ||
            v.Make.Contains(search) ||
            v.Model.Contains(search) ||
            v.Color.Contains(search)
        );
    }
}
