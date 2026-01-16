using Garage_2.Data;
using Garage_2.Interfaces;
using Garage_2.Models;
using Garage_2.Models.Entities;
using Garage_2.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Garage_2.Controllers
{
    [Authorize]
    public class VehiclesController : Controller
    {
        private readonly GarageContext _context;
        private readonly GarageConfig _config;
        private readonly IVehicleSearchService _searchService;
        private readonly IParkingService _parkingService;

        public VehiclesController(
            GarageContext context,
            IOptions<GarageConfig> config,
            IVehicleSearchService searchService,
            IParkingService parkingService)
        {
            _context = context;
            _config = config.Value;
            _searchService = searchService;
            _parkingService = parkingService;
        }

        // GET: Vehicles
        // Visar användarens fordon + om de är parkerade just nu (aktiv session)
        public async Task<IActionResult> Index(
            [FromQuery(Name = "sortBy")] OverviewSortBy? sortBy,
            [FromQuery(Name = "order")] OverviewSortOrder? order,
            [FromQuery(Name = "searchString")] string? searchString,
            [FromQuery(Name = "searchField")] string? searchField,
            [FromQuery(Name = "page")] int page = 1)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Basquery: bara användarens fordon
            IQueryable<Vehicle> query = _context.Vehicles
                .AsNoTracking()
                .Where(v => v.ApplicationUserId == userId)
                .Include(v => v.VehicleType)
                .Include(v => v.ParkingSessions.Where(ps => ps.DepartureTime == null)); // aktiv session (max 1)

            query = _searchService.Search(query, searchString, searchField);

            var now = DateTime.Now;

            var rows = query.Select(v => new OverviewListItemViewModel
            {
                Id = v.Id,
                RegistrationNumber = v.RegistrationNumber,
                Type = v.VehicleType.Name,

                ArrivalTime = v.ParkingSessions
                    .Where(ps => ps.DepartureTime == null)
                    .Select(ps => (DateTime?)ps.ArrivalTime)
                    .FirstOrDefault(),

                ParkedTime = v.ParkingSessions
                    .Where(ps => ps.DepartureTime == null)
                    .Select(ps => (TimeSpan?)(now - ps.ArrivalTime))
                    .FirstOrDefault()
            });

            sortBy ??= OverviewSortBy.ArrivalTime;
            order ??= OverviewSortOrder.Descending;

            rows = (sortBy, order) switch
            {
                (OverviewSortBy.RegistrationNumber, OverviewSortOrder.Ascending) => rows.OrderBy(r => r.RegistrationNumber),
                (OverviewSortBy.RegistrationNumber, _) => rows.OrderByDescending(r => r.RegistrationNumber),

                (OverviewSortBy.Type, OverviewSortOrder.Ascending) => rows.OrderBy(r => r.Type),
                (OverviewSortBy.Type, _) => rows.OrderByDescending(r => r.Type),

                (OverviewSortBy.ArrivalTime, OverviewSortOrder.Ascending) => rows.OrderBy(r => r.ArrivalTime),
                (OverviewSortBy.ArrivalTime, _) => rows.OrderByDescending(r => r.ArrivalTime),

                (OverviewSortBy.ParkedTime, OverviewSortOrder.Ascending) => rows.OrderBy(r => r.ParkedTime),
                _ => rows.OrderByDescending(r => r.ParkedTime)
            };

            int rowCount = await rows.CountAsync();
            int pageSize = 10;
            int totalPages = (int)Math.Ceiling((double)rowCount / pageSize);

            var currentRows = await rows
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            OverviewViewModel viewModel = new()
            {
                OverviewList = currentRows,
                SortBy = sortBy,
                SortOrder = order,
                Count = rowCount,
                SearchString = searchString,
                SearchField = searchField ?? string.Empty,
                TotalPages = totalPages,
                CurrentPage = page
            };

            return View(viewModel);
        }

        // GET: Vehicles/Details/5
        // Visar fordon + aktiv session + platser om parkerad
        public async Task<IActionResult> Details(int? id, string? searchString)
        {
            if (id is null)
            {
                SetAlertInTempData(AlertType.warning, "Vehicle not found.");
                return RedirectToAction(nameof(Index));
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var vehicle = await _context.Vehicles
                .AsNoTracking()
                .Where(v => v.ApplicationUserId == userId && v.Id == id.Value)
                .Include(v => v.VehicleType)
                .Include(v => v.ParkingSessions.Where(ps => ps.DepartureTime == null))
                    .ThenInclude(ps => ps.VehicleParkings)
                        .ThenInclude(vp => vp.ParkingSpotV2)
                .FirstOrDefaultAsync();

            if (vehicle is null)
            {
                SetAlertInTempData(AlertType.warning, "Vehicle not found.");
                return RedirectToAction(nameof(Index));
            }

            ViewData["CurrentFilter"] = searchString;

            var activeSession = vehicle.ParkingSessions.FirstOrDefault(); // max 1
            return View(new DetailsViewModel(vehicle, activeSession));
        }

        // GET: Vehicles/RegisterVehicle
        public async Task<IActionResult> RegisterVehicle()
        {
            var vm = new RegisterVehicleViewModel
            {
                VehicleTypes = await _context.VehicleTypes
                    .AsNoTracking()
                    .OrderBy(t => t.Name)
                    .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name })
                    .ToListAsync()
            };

            return View(vm);
        }

        // POST: Vehicles/RegisterVehicle
        // Registrerar fordon (ingen parkering här).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterVehicle(RegisterVehicleViewModel viewModel)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (await VehicleRegistrationExists(viewModel.RegistrationNumber))
            {
                ModelState.AddModelError(nameof(viewModel.RegistrationNumber),
                    "A vehicle with that registration number already exists.");
            }

            if (!ModelState.IsValid)
            {
                // Viktigt: fyll dropdown igen, annars kraschar vyn / blir tom
                viewModel.VehicleTypes = await _context.VehicleTypes
                    .AsNoTracking()
                    .OrderBy(t => t.Name)
                    .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name })
                    .ToListAsync();

                return View(viewModel);
            }

            var vehicle = new Vehicle
            {
                RegistrationNumber = viewModel.RegistrationNumber,
                Make = viewModel.Make,
                Model = viewModel.Model,
                NumberOfWheels = viewModel.NumberOfWheels,
                Color = viewModel.Color,
                VehicleTypeId = viewModel.VehicleTypeId,
                ApplicationUserId = userId
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            SetAlertInTempData(AlertType.success, $"Vehicle {vehicle.RegistrationNumber} registered.");
            return RedirectToAction(nameof(Index));
        }

        // POST: Vehicles/ParkVehicle/5
        // Parkerar ett redan registrerat fordon (skapar ParkingSession + VehicleParking via service)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ParkVehicle(int id)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Säkerställ ägarskap (billig kontroll)
            var vehicle = await _context.Vehicles
                .Include(v => v.VehicleType) // om din ParkingService använder VehicleType
                .FirstOrDefaultAsync(v => v.Id == id && v.ApplicationUserId == userId);

            if (vehicle is null)
            {
                SetAlertInTempData(AlertType.warning, "Vehicle not found.");
                return RedirectToAction(nameof(Index));
            }

            var result = await _parkingService.ParkVehicleAsync(vehicle);

            if (!result.Success)
            {
                SetAlertInTempData(AlertType.warning, result.ErrorMessage!);
                return RedirectToAction(nameof(Index));
            }

            SetAlertInTempData(AlertType.success, $"Vehicle {vehicle.RegistrationNumber} parked.");
            return RedirectToAction(nameof(Index));
        }

        // GET: Vehicles/UnparkVehicle/5
        public async Task<IActionResult> UnparkVehicle(int? id)
        {
            if (id is null)
            {
                SetAlertInTempData(AlertType.warning, "Vehicle not found.");
                return RedirectToAction(nameof(Index));
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var activeSession = await _context.ParkingSessions
                .AsNoTracking()
                .Where(ps => ps.DepartureTime == null && ps.VehicleId == id.Value)
                .Include(ps => ps.Vehicle)
                    .ThenInclude(v => v.VehicleType)
                .Include(ps => ps.VehicleParkings)
                    .ThenInclude(vp => vp.ParkingSpotV2)
                .FirstOrDefaultAsync();

            if (activeSession is null || activeSession.Vehicle.ApplicationUserId != userId)
            {
                SetAlertInTempData(AlertType.warning, "Active parking session not found.");
                return RedirectToAction(nameof(Index));
            }

            // Vyn ska ta ParkingSession som model
            return View(activeSession);
        }

        [HttpPost, ActionName("UnparkConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnparkConfirmed(int id) // id = VehicleId
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var session = await _context.ParkingSessions
                .Where(ps => ps.DepartureTime == null && ps.VehicleId == id)
                .Include(ps => ps.Vehicle)
                    .ThenInclude(v => v.VehicleType)
                .Include(ps => ps.VehicleParkings)
                    .ThenInclude(vp => vp.ParkingSpotV2)
                .FirstOrDefaultAsync();

            if (session is null || session.Vehicle.ApplicationUserId != userId)
            {
                SetAlertInTempData(AlertType.warning, "Active parking session not found.");
                return RedirectToAction(nameof(Index));
            }

            DateTime checkoutTime = DateTime.Now;
            session.DepartureTime = checkoutTime;

            var totalParkingTime = checkoutTime - session.ArrivalTime;

            int unitsUsed = session.VehicleParkings.Sum(vp => vp.UnitsUsed);
            decimal sizeMultiplier = unitsUsed / 3m;
            decimal totalPrice = (decimal)Math.Ceiling(totalParkingTime.TotalHours) * _config.PricePerHour * sizeMultiplier;

            await _context.SaveChangesAsync();

            var receiptVM = new ReceiptViewModel
            {
                RegistrationNumber = session.Vehicle.RegistrationNumber,
                Type = session.Vehicle.VehicleType.Name,
                ArrivalTime = session.ArrivalTime,
                CheckoutTime = checkoutTime,
                ParkingDuration = totalParkingTime,
                Price = totalPrice,
                ParkingSpots = session.VehicleParkings.Select(vp => vp.ParkingSpotV2.SpotNumber).ToList()
            };

            SetAlertInTempData(AlertType.success, $"Vehicle {receiptVM.RegistrationNumber} checked out.");
            return View("Receipt", receiptVM);
        }

        private async Task<bool> VehicleRegistrationExists(string registrationNumber)
        {
            return await _context.Vehicles.AnyAsync(v => v.RegistrationNumber == registrationNumber);
        }

        private void SetAlertInTempData(AlertType type, string message)
        {
            TempData["AlertType"] = type.ToString().ToLower();
            TempData["AlertMessage"] = message;
        }
    }
}
