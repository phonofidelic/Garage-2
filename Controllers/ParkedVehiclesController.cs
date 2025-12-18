using Garage_2.Data;
using Garage_2.Models;
using Garage_2.Models.ViewModels;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Garage_2.Controllers
{
    public class ParkedVehiclesController : Controller
    {
        private readonly GarageContext _context;
        private readonly GarageConfig _config;

        public ParkedVehiclesController(GarageContext context, IOptions<GarageConfig> config)
        {
            _context = context;
            _config = config.Value;
        }

        // GET: ParkedVehicles
        public async Task<IActionResult> Index(string? searchString)
        {
            // Store the search string in ViewData
            ViewData["CurrentFilter"] = searchString;

            // Start with all vehicles from the database
            var vehicles = from v in _context.ParkedVehicle select v;

            // Filter vehicles by registration number if a search string is provided
            if (!string.IsNullOrEmpty(searchString))
            {
                vehicles = vehicles.Where(v => v.RegistrationNumber.Contains(searchString));
            }

            // Execute the query, put the data into overviewmodel and return view
            return View(await vehicles.Select(v => new OverviewViewModel
            {
                Id = v.Id,
                RegistrationNUmber = v.RegistrationNumber,
                Type = v.Type,
                ArrivalTime = v.ArrivalTime
            }).ToListAsync());
        }

        // GET: ParkedVehicles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkedVehicle = await _context.ParkedVehicle
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parkedVehicle == null)
            {
                return NotFound();
            }

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

                TempData["AlertType"] = "success";
                TempData["AlertMessage"] = $"Vehicle with <strong>RegNum: {viewModel.RegistrationNumber}</strong> has been parked.";
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        // GET: ParkedVehicles/Edit/5
        public async Task<IActionResult> EditVehicle(int? id)
        {
            if (id == null)
            {

                TempData["AlertType"] = "warning";
                TempData["AlertMessage"] = "Vehicle not found.";
                return RedirectToAction(nameof(Index));
            }

            // Retrieve the parked vehicle by id.
            var parkedVehicle = await _context.ParkedVehicle.FindAsync(id);
            if (parkedVehicle == null)
            {
                TempData["AlertType"] = "warning";
                TempData["AlertMessage"] = "Vehicle not found.";
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
                TempData["AlertType"] = "warning";
                TempData["AlertMessage"] = "Vehicle not found.";
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
                        TempData["AlertType"] = "warning";
                        TempData["AlertMessage"] = "Vehicle not found.";
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
                    TempData["AlertType"] = "success";
                    TempData["AlertMessage"] = $"Vehicle with <strong>RegNum: {parkedVehicle.RegistrationNumber}</strong> was updated successfully.";
                    return RedirectToAction(nameof(Index));

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParkedVehicleExists(vm.Id))
                    {
                        TempData["AlertType"] = "warning";
                        TempData["AlertMessage"] = "Vehicle not found.";
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
                return NotFound();
            }

            var parkedVehicle = await _context.ParkedVehicle.FirstOrDefaultAsync(m => m.Id == id);

            if (parkedVehicle == null)
            {
                return NotFound();
            }

            return View(parkedVehicle);
        }


        [HttpPost, ActionName("UnparkConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnparkConfirmed(int id)
        {
            var vehicle = await _context.ParkedVehicle.FindAsync(id);

            if (vehicle == null)
                return NotFound();

            DateTime checkoutTime = DateTime.Now;
            TimeSpan totalParkingTime = checkoutTime - vehicle.ArrivalTime;

            decimal totalPrice = (decimal)Math.Ceiling(totalParkingTime.TotalHours) * _config.PricePerHour;

            var receipt = new ReceiptViewModel
            {
                RegistrationNumber = vehicle.RegistrationNumber,
                Type = vehicle.Type,
                ArrivalTime = vehicle.ArrivalTime,
                CheckoutTime = checkoutTime,
                ParkingDuration = totalParkingTime,
                Price = totalPrice
            };

            _context.ParkedVehicle.Remove(vehicle);
            await _context.SaveChangesAsync();

            return View("Receipt", receipt);
        }

        private bool ParkedVehicleExists(int id)
        {
            return _context.ParkedVehicle.Any(e => e.Id == id);
        }

        private bool VehicleRegistrationExists(string registrationNumger)
        {
            return _context.ParkedVehicle.Any(vehicle => vehicle.RegistrationNumber == registrationNumger);
        }
    }
}
