using Garage_2.Interfaces;
using Garage_2.Models;
using Garage_2.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Garage_2.Services;

public class VehicleSearchService : IVehicleSearchService
{
    public IQueryable<T> Search<T>(IQueryable<T> query, string? searchString, string? searchField = null) where T : class
    {
        if (string.IsNullOrWhiteSpace(searchString))
            return query;

        var search = searchString.Trim();

        if (typeof(T) == typeof(Vehicle))
        {
            var vehicleQuery = query as IQueryable<Vehicle>;

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

    private IQueryable<Vehicle> FilterByWheels(IQueryable<Vehicle> query, string search)
    {
        if (int.TryParse(search, out var wheels))
        {
            return query.Where(v => v.NumberOfWheels == wheels);
        }
        // If parse fails, return no results
        return query.Where(v => false);
    }

    private IQueryable<Vehicle> FilterByType(IQueryable<Vehicle> query, string search)
    {
        var normalized = search.ToLower();

        return query.Where(v =>
        v.VehicleType != null &&
        v.VehicleType.Name.ToLower().Contains(normalized));
    }

    private IQueryable<Vehicle> FilterByDate(IQueryable<Vehicle> query, string search)
    {
        if (!DateTime.TryParse(search, out var date))
            return query.Where(_ => false);

        var start = date.Date;
        var end = start.AddDays(1);

        return query.Where(v =>
            v.ParkingSessions.Any(ps => ps.ArrivalTime >= start && ps.ArrivalTime < end && ps.DepartureTime == null));
    }

    private IQueryable<Vehicle> FilterAllFields(IQueryable<Vehicle> query, string search)
    {
        return query.Where(v =>
            v.RegistrationNumber.Contains(search) ||
            v.Make.Contains(search) ||
            v.Model.Contains(search) ||
            v.Color.Contains(search)
        );
    }
}
