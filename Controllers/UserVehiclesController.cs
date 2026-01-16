using Garage_2.Data;
using Garage_2.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Garage_2.Controllers
{
    public class UserVehiclesController : Controller
    {
        private readonly GarageContext _context;
        private readonly GarageConfig _config;

        public UserVehiclesController(GarageContext context, IOptions<GarageConfig> config)
        {
            _context = context;
            _config = config.Value;
        }

        public async Task<IActionResult> Index(
            string? searchString,
            UserOverviewSortBy? sortBy,
            UserOverviewSortOrder? order,
            int page = 1)
        {
            const int pageSize = 15;
            var now = DateTime.Now;

            // 1) Baslista users (med vehicle count)
            var usersQuery = _context.Users
                .AsNoTracking()
                .Select(u => new
                {
                    u.Id,
                    FullName = (u.FirstName + " " + u.LastName).Trim(),
                    u.Email,
                    VehicleCount = u.Vehicles.Count
                });

            // 2) Sök (name/email)
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var s = searchString.Trim();

                usersQuery = usersQuery.Where(u =>
                    u.FullName.Contains(s) ||
                    (u.Email != null && u.Email.Contains(s)));
            }

            var users = await usersQuery.ToListAsync();

            // 3) Aktiva sessions (begränsa till de users som finns i listan)
            var userIds = users.Select(u => u.Id).ToList();

            var activeSessions = await _context.ParkingSessions
                .AsNoTracking()
                .Where(ps => ps.DepartureTime == null && userIds.Contains(ps.Vehicle.ApplicationUserId))
                .Select(ps => new
                {
                    UserId = ps.Vehicle.ApplicationUserId,
                    ps.ArrivalTime,
                    UnitsUsed = ps.VehicleParkings.Sum(vp => vp.UnitsUsed)
                })
                .ToListAsync();

            var costByUser = activeSessions
                .GroupBy(x => x.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => CalculatePriceNow(x.ArrivalTime, x.UnitsUsed, now))
                );

            // 4) Bygg list items
            var items = users.Select(u => new UserOverviewListItemViewModel
            {
                UserId = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                VehicleCount = u.VehicleCount,
                TotalActiveCostNow = costByUser.TryGetValue(u.Id, out var cost) ? cost : 0m
            }).ToList();

            // 5) Sort
            var sort = sortBy ?? UserOverviewSortBy.FullName;
            var sortOrder = order ?? UserOverviewSortOrder.Ascending;

            items = sortOrder == UserOverviewSortOrder.Ascending
                ? sort switch
                {
                    UserOverviewSortBy.FullName => items.OrderBy(x => x.FullName).ToList(),
                    UserOverviewSortBy.Email => items.OrderBy(x => x.Email).ToList(),
                    UserOverviewSortBy.VehicleCount => items.OrderBy(x => x.VehicleCount).ToList(),
                    UserOverviewSortBy.TotalActiveCostNow => items.OrderBy(x => x.TotalActiveCostNow).ToList(),
                    _ => items.OrderBy(x => x.FullName).ToList()
                }
                : sort switch
                {
                    UserOverviewSortBy.FullName => items.OrderByDescending(x => x.FullName).ToList(),
                    UserOverviewSortBy.Email => items.OrderByDescending(x => x.Email).ToList(),
                    UserOverviewSortBy.VehicleCount => items.OrderByDescending(x => x.VehicleCount).ToList(),
                    UserOverviewSortBy.TotalActiveCostNow => items.OrderByDescending(x => x.TotalActiveCostNow).ToList(),
                    _ => items.OrderByDescending(x => x.FullName).ToList()
                };

            // 6) Paging
            var count = items.Count;
            var totalPages = (int)Math.Ceiling(count / (double)pageSize);
            totalPages = Math.Max(totalPages, 1);
            page = Math.Clamp(page, 1, totalPages);

            var paged = items
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = new UserOverviewViewModel
            {
                OverviewList = paged,
                Count = count,
                SearchString = searchString,
                SortBy = sort,
                SortOrder = sortOrder,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(vm);
        }

        private decimal CalculatePriceNow(DateTime arrivalTime, int unitsUsed, DateTime now)
        {
            var totalParkingTime = now - arrivalTime;
            var hours = (decimal)Math.Ceiling(totalParkingTime.TotalHours);

            var sizeMultiplier = unitsUsed / 3m;
            return hours * _config.PricePerHour * sizeMultiplier;
        }
    }
}
