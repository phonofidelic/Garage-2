using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Garage_2.Data;
using Garage_2.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Garage_2.Models.ViewModels.ParkingSessions;
using Garage_2.Interfaces;
using System.Security.Claims;
using Garage_2.Models.ViewModels;
using Microsoft.Data.SqlClient;
using Garage_2.Extensions;

namespace Garage_2.Controllers
{
    [Authorize]
    public class ParkingSessionsController : Controller
    {
        private readonly GarageContext _context;
        private readonly IParkingSessionService _parkingSessionService;

        public ParkingSessionsController(
            GarageContext context,
            IParkingSessionService parkingSessionService)
        {
            _context = context;
            _parkingSessionService = parkingSessionService;
        }

        // GET: ParkingSessions
        public async Task<IActionResult> Index(
            [FromQuery(Name = "sortBy")] ParkingSessionsSortBy sortBy = ParkingSessionsSortBy.ArrivalTime,
            [FromQuery(Name = "order")] SortOrder order = SortOrder.Ascending,
            [FromQuery(Name = "limit")] int limit = 10,
            [FromQuery(Name = "page")] int page = 1
        )
        {
            ParkingSessionsListParameters listParameters = new()
            {
                SortBy = sortBy,
                SortOrder = order,
                PageLimit = limit,
                CurrentPage = page
            };

            var garageContext = _context.ParkingSessions.Include(p => p.Vehicle);
            bool isAdmin = User.IsInRole("Admin");
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            
            IQueryable<ParkingSession> parkingSessionsQuery = _context.ParkingSessions
                .Include(ps => ps.VehicleParkings)
                    .ThenInclude(vp => vp.ParkingSpotV2)
                .Include(p => p.Vehicle)
                    .ThenInclude(v => v.VehicleType)
                .Include(p => p.Vehicle)
                    .ThenInclude(v => v.User)
                .Where(ps => isAdmin || ps.Vehicle.ApplicationUserId == userId);

            int itemsCount = parkingSessionsQuery.Count();

            IEnumerable<ParkingSessionsListItemViewModel> parkingSessionsListItems = await parkingSessionsQuery.Select(parkingSession => new ParkingSessionsListItemViewModel()
            {
                Id = parkingSession.Id,
                VehicleId = parkingSession.VehicleId,
                VehicleOwner = parkingSession.Vehicle.User.UserName ?? "",
                VehicleType = parkingSession.Vehicle.VehicleType.Name,
                RegistrationNumber = parkingSession.Vehicle.RegistrationNumber,
                ArrivalTime = parkingSession.ArrivalTime,
                DepartureTime = parkingSession.DepartureTime,
                CurrentCost = _parkingSessionService.GetTotalParkingSessionCost(parkingSession, DateTime.Now),
            })
            .ToListAsync();

            // SortByWithOrder extension defined in Extensions/ParkingSessionsExtensions
            var orderedParkingSessionsListItems = parkingSessionsListItems
            .SortByWithOrder(sortBy, order)
            .Skip(limit * (page - 1))
            .Take(limit);

            ParkingSessionsIndexViewModel viewModel = new()
            {
                IsAdmin = isAdmin,
                ListParameters = listParameters,
                ParkingSessionsList = orderedParkingSessionsListItems,
                CurrentTotal = parkingSessionsListItems.Sum(ps => ps.CurrentCost)
            };

            viewModel.ListParameters.ItemsCount = itemsCount;
            viewModel.ListParameters.FilteredItemsCount = orderedParkingSessionsListItems.Count();
            viewModel.ListParameters.TotalPages = (int)Math.Ceiling((double)itemsCount / limit);
            
            return View(viewModel);
        }

        // GET: ParkingSessions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkingSession = await _context.ParkingSessions
                .Include(p => p.Vehicle)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parkingSession == null)
            {
                return NotFound();
            }

            return View(parkingSession);
        }

        private bool ParkingSessionExists(int id)
        {
            return _context.ParkingSessions.Any(e => e.Id == id);
        }
    }
}
