using Garage_2.Data;
using Garage_2.Interfaces;
using Garage_2.Models;
using Garage_2.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Garage_2.Controllers
{
    public class ParkedVehiclesController : Controller
    {
        private readonly GarageContext _context;
        private readonly GarageConfig _config;
        private readonly IVehicleSearchService _searchService;

        public ParkedVehiclesController(GarageContext context, IOptions<GarageConfig> config, IVehicleSearchService searchService)
        {
            _context = context;
            _config = config.Value;
            _searchService = searchService;
        }

        // GET: ParkedVehicles
        public async Task<IActionResult> Index(string? searchString, int page = 1)
        {
            // Store the search string in ViewData
            ViewData["CurrentFilter"] = searchString;

            // Start with all vehicles and apply smart search
            var query = _searchService.Search(_context.ParkedVehicle, searchString);

            // Execute the query, put the data into overviewmodel and return view
            var now = DateTime.Now;
            var rows = await query
                .Select(v => new OverviewViewModel
                {
                    Id = v.Id,
                    RegistrationNumber = v.RegistrationNumber,
                    Type = v.Type,
                    ArrivalTime = v.ArrivalTime
                })
                .ToListAsync();

            rows.ForEach(r => r.ParkedTime = now - r.ArrivalTime);

            int pageSize = 10;
            var totalPages = (int)Math.Ceiling((double)rows.Count / pageSize);

            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;

            var currentRows = rows
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

            return View(currentRows);
        }

        // GET: ParkedVehicles/Details/5
        public async Task<IActionResult> Details(int? id, string? searchString)
        {
            if (id == null)
            {
                SetAlertInTempData(AlertType.warning, "Vehicle not found.");
                return RedirectToAction(nameof(Index));
            }

            var parkedVehicle = await _context.ParkedVehicle.FirstOrDefaultAsync(m => m.Id == id);

            if (parkedVehicle == null)
            {
                SetAlertInTempData(AlertType.warning, "Vehicle not found.");
                return RedirectToAction(nameof(Index));
            }

            // Store the search string to pass back to Index
            ViewData["CurrentFilter"] = searchString;

            return View(new DetailsViewModel(parkedVehicle));
        }

        // GET: ParkedVehicles/ParkNewVehicle
        public IActionResult ParkNewVehicle()
        {
            return View();
        }

        // POST: ParkedVehicles/ParkNewVehicle
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ParkNewVehicle([Bind("Id,RegistrationNumber,Make,Model,NumberOfWheels,Color,ArrivalTime,Type")] ParkNewVehicleViewModel viewModel)
        {
            if (VehicleRegistrationExists(viewModel.RegistrationNumber))
            {
                ModelState.AddModelError(nameof(viewModel.RegistrationNumber), errorMessage: "A vehicle with that registration number is already parked in this garage.");
            }

            if (ModelState.IsValid)
            {
                ParkedVehicle parkedVehicle = new()
                {
                    RegistrationNumber = viewModel.RegistrationNumber,
                    Make = viewModel.Make,
                    Model = viewModel.Model,
                    NumberOfWheels = viewModel.NumberOfWheels,
                    Color = viewModel.Color,
                    ArrivalTime = DateTime.Now,
                    Type = viewModel.Type
                };

                _context.Add(parkedVehicle);
                await _context.SaveChangesAsync();

                SetAlertInTempData(AlertType.success, $"Vehicle with RegNum: {viewModel.RegistrationNumber} has been parked.");

                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        // GET: ParkedVehicles/Edit/5
        public async Task<IActionResult> EditVehicle(int? id)
        {
            if (id == null)
            {
                SetAlertInTempData(AlertType.warning, "Vehicle not found.");
                return RedirectToAction(nameof(Index));
            }

            // Retrieve the parked vehicle by id.
            var parkedVehicle = await _context.ParkedVehicle.FindAsync(id);
            if (parkedVehicle == null)
            {
                SetAlertInTempData(AlertType.warning, "Vehicle not found.");
                return RedirectToAction(nameof(Index));
            }

            // Map entity to an EditViewModel; only expose fields that are editable.
            var vm = new ParkedVehicleEditViewModel
            {
                Id = parkedVehicle.Id,
                RegistrationNumber = parkedVehicle.RegistrationNumber,
                Make = parkedVehicle.Make,
                Model = parkedVehicle.Model,
                NumberOfWheels = parkedVehicle.NumberOfWheels,
                Color = parkedVehicle.Color,
                Type = parkedVehicle.Type,
                ArrivalTime = parkedVehicle.ArrivalTime
            };

            return View(vm);
        }

        // POST: ParkedVehicles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVehicle(int id, ParkedVehicleEditViewModel vm)
        {
            if (id != vm.Id)
            {
                SetAlertInTempData(AlertType.warning, "Vehicle not found.");
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch the entity to update
                    var parkedVehicle = await _context.ParkedVehicle.FirstOrDefaultAsync(p => p.Id == id);

                    if (parkedVehicle == null)
                    {
                        SetAlertInTempData(AlertType.warning, "Vehicle not found.");
                        return RedirectToAction(nameof(Index));
                    }

                    // Update allowed fields from the view model.
                    parkedVehicle.RegistrationNumber = vm.RegistrationNumber;
                    parkedVehicle.Make = vm.Make;
                    parkedVehicle.Model = vm.Model;
                    parkedVehicle.NumberOfWheels = vm.NumberOfWheels;
                    parkedVehicle.Color = vm.Color;
                    parkedVehicle.Type = vm.Type;


                    await _context.SaveChangesAsync();
                    SetAlertInTempData(AlertType.success, $"Vehicle with RegNum: {parkedVehicle.RegistrationNumber} was updated successfully.");
                    return RedirectToAction(nameof(Index));

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParkedVehicleExists(vm.Id))
                    {
                        SetAlertInTempData(AlertType.warning, "Vehicle not found.");
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(vm);
        }

        // GET: ParkedVehicles/Delete/5
        public async Task<IActionResult> UnparkVehicle(int? id)
        {
            if (id == null)
            {
                SetAlertInTempData(AlertType.warning, "Vehicle not found.");
                return RedirectToAction(nameof(Index));
            }

            var parkedVehicle = await _context.ParkedVehicle.FirstOrDefaultAsync(m => m.Id == id);

            if (parkedVehicle == null)
            {
                SetAlertInTempData(AlertType.warning, "Vehicle not found.");
                return RedirectToAction(nameof(Index));
            }

            return View(parkedVehicle);
        }


        [HttpPost, ActionName("UnparkConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnparkConfirmed(int id)
        {
            var vehicle = await _context.ParkedVehicle.FindAsync(id);

            if (vehicle == null)
            { //return NotFound();
                SetAlertInTempData(AlertType.warning, "Vehicle not found.");
                return RedirectToAction(nameof(Index));
            }

            DateTime checkoutTime = DateTime.Now;
            TimeSpan totalParkingTime = checkoutTime - vehicle.ArrivalTime;

            // Price calculated on every started hour
            decimal totalPrice = (decimal)Math.Ceiling(totalParkingTime.TotalHours) * _config.PricePerHour;

            var receiptVM = new ReceiptViewModel
            {
                RegistrationNumber = vehicle.RegistrationNumber,
                Type = vehicle.Type,
                ArrivalTime = vehicle.ArrivalTime,
                CheckoutTime = checkoutTime,
                ParkingDuration = totalParkingTime,
                Price = totalPrice
            };

            // Todo: try-catch här
            _context.ParkedVehicle.Remove(vehicle);
            await _context.SaveChangesAsync();

            SetAlertInTempData(AlertType.success, $"Vehicle with RegNo: {receiptVM.RegistrationNumber} has been checked out.");

            return View("Receipt", receiptVM);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        private bool ParkedVehicleExists(int id)
        {
            return _context.ParkedVehicle.Any(e => e.Id == id);
        }

        private bool VehicleRegistrationExists(string registrationNumger)
        {
            return _context.ParkedVehicle.Any(vehicle => vehicle.RegistrationNumber == registrationNumger);
        }

        private void SetAlertInTempData(AlertType type, string message)
        {
            TempData["AlertType"] = type.ToString().ToLower(); // "success", "warning", "danger", "info"
            TempData["AlertMessage"] = message;
        }

    }
}
