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
        public async Task<IActionResult> Index()
        {
            var garageContext = _context.ParkingSessions.Include(p => p.Vehicle);

            ParkingSessionsIndexViewModel viewModel = new();

            var parkingSessions = _context.ParkingSessions
                .Include(ps => ps.VehicleParkings)
                    .ThenInclude(vp => vp.ParkingSpotV2);

            if (User.IsInRole("User"))
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                var userParkingSessions = parkingSessions
                    .Include(p => p.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Where(ps => ps.Vehicle.ApplicationUserId == userId);

                viewModel.ParkingSessionsList = await userParkingSessions
                    .Select(parkingSession => new ParkingSessionsListItemViewModel()
                    {
                        Id = parkingSession.Id,
                        VehicleId = parkingSession.VehicleId,
                        VehicleType = parkingSession.Vehicle.VehicleType.Name,
                        RegistrationNumber = parkingSession.Vehicle.RegistrationNumber,
                        ArrivalTime = parkingSession.ArrivalTime,
                        DepartureTime = parkingSession.DepartureTime,
                        TotalCost = _parkingSessionService.GetTotalParkingSessionCost(parkingSession, DateTime.Now)
                    })
                    .ToListAsync();

                return View(viewModel);
            }

            if (User.IsInRole("Admin"))
            {
                viewModel.ParkingSessionsList = await parkingSessions
                    .Include(p => p.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(p => p.Vehicle)
                        .ThenInclude(v => v.User)
                    .Select(parkingSession => new ParkingSessionsListItemViewModel()
                {
                    Id = parkingSession.Id,
                    VehicleId = parkingSession.VehicleId,
                    VehicleOwner = parkingSession.Vehicle.User.UserName ?? "",
                    VehicleType = parkingSession.Vehicle.VehicleType.Name,
                    RegistrationNumber = parkingSession.Vehicle.RegistrationNumber,
                    ArrivalTime = parkingSession.ArrivalTime,
                    DepartureTime = parkingSession.DepartureTime,
                    TotalCost = _parkingSessionService.GetTotalParkingSessionCost(parkingSession, DateTime.Now)
                })
                .ToListAsync();
            }
           
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
