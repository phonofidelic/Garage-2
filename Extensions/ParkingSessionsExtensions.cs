using System;
using Garage_2.Models.ViewModels.ParkingSessions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Data.SqlClient;

namespace Garage_2.Extensions;

public static class ParkingSessionsExtensions
{
    public static IEnumerable<ParkingSessionsListItemViewModel> SortByWithOrder(
        this IEnumerable<ParkingSessionsListItemViewModel> parkingSessionsList, 
        ParkingSessionsSortBy sortBy, 
        SortOrder order)
    {
        // var parkingSessionsList = await parkingSessionsListTask;
        return (sortBy, order) switch
        {
            (ParkingSessionsSortBy.VehicleType, SortOrder.Descending) => parkingSessionsList.OrderByDescending(ps => ps.VehicleType),
            (ParkingSessionsSortBy.VehicleType, _) => parkingSessionsList.OrderBy(ps => ps.VehicleType),

            (ParkingSessionsSortBy.VehicleOwner, SortOrder.Descending) => parkingSessionsList.OrderByDescending(ps => ps.VehicleOwner),
            (ParkingSessionsSortBy.VehicleOwner, _) => parkingSessionsList.OrderBy(ps => ps.VehicleOwner),

            (ParkingSessionsSortBy.RegistrationNumber, SortOrder.Descending) => parkingSessionsList.OrderByDescending(ps => ps.RegistrationNumber),
            (ParkingSessionsSortBy.RegistrationNumber, _) => parkingSessionsList.OrderBy(ps => ps.RegistrationNumber),

            (ParkingSessionsSortBy.ArrivalTime, SortOrder.Descending) => parkingSessionsList.OrderByDescending(ps => ps.ArrivalTime),
            (ParkingSessionsSortBy.ArrivalTime, _) => parkingSessionsList.OrderBy(ps => ps.ArrivalTime),

            (ParkingSessionsSortBy.DepartureTime, SortOrder.Descending) => parkingSessionsList.OrderByDescending(ps => ps.DepartureTime),
            (ParkingSessionsSortBy.DepartureTime, _) => parkingSessionsList.OrderBy(ps => ps.DepartureTime),

            (ParkingSessionsSortBy.CurrentCost, SortOrder.Descending) => parkingSessionsList.OrderByDescending(ps => ps.CurrentCost),
            (ParkingSessionsSortBy.CurrentCost, _) => parkingSessionsList.OrderBy(ps => ps.CurrentCost),

            (ParkingSessionsSortBy.ParkedTime, SortOrder.Descending) => parkingSessionsList.OrderByDescending(ps => ps.ParkedTime),
            _ => parkingSessionsList.OrderBy(ps => ps.ParkedTime),
        };
    }

    public static string GetSortOrderIndicatorStyle(this SortOrder sortOrder) 
    {
        string rotation = sortOrder == SortOrder.Ascending
            ? ""
            : "transform: rotate(0.5turn);";

        return rotation + " display: inline-block; width: 1rem; line-height: 1rem; text-align: center; font-size: 0.75rem";
    }

    public static SortOrder Toggle(this SortOrder sortOrder)
    {
        return sortOrder == SortOrder.Ascending 
            ? SortOrder.Descending
            : SortOrder.Ascending;
    }

    public static SortOrder GetSortOrder(this ParkingSessionsListParameters listParameters, ParkingSessionsSortBy sortBy)
    {
        return listParameters.SortBy == sortBy
        ? listParameters.SortOrder.Toggle()
        : SortOrder.Ascending;
    }
}
