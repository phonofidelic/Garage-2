using Garage_2.Data;
using Garage_2.Models;
using Garage_2.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Garage_2.Controllers
{
    public class ParkedVehiclesController : Controller
    {
        private readonly GarageContext _context;

        public ParkedVehiclesController(GarageContext context)
        {
            _context = context;
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

            // Execute the query and return the filtered results to the view
            return View(await vehicles.ToListAsync());
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

            //return View(parkedVehicle);
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
        public async Task<IActionResult> ParkNewVehicle([Bind("Id,RegistrationNumber,Make,Model,NumberOfWheels,Color,ArrivalTime,Type")] ParkedVehicle parkedVehicle)
        {
            if (ModelState.IsValid)
            {
                parkedVehicle.ArrivalTime = DateTime.UtcNow;
                _context.Add(parkedVehicle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ParkNewVehicleViewModel viewModel = new()
            {
                RegistrationNumber = parkedVehicle.RegistrationNumber,
                Make = parkedVehicle.Make,
                Model = parkedVehicle.Model,
                NumberOfWheels = parkedVehicle.NumberOfWheels,
                Color = parkedVehicle.Color,
                Type = parkedVehicle.Type
            };

            return View(viewModel);
        }

        // GET: ParkedVehicles/Edit/5
        public async Task<IActionResult> EditVehicle(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Retrieve the parked vehicle by id.
            var parkedVehicle = await _context.ParkedVehicle.FindAsync(id);
            if (parkedVehicle == null)
            {
                return NotFound();
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
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch the entity to update
                    var parkedVehicle = await _context.ParkedVehicle.FirstOrDefaultAsync(p => p.Id == id);

                    if (parkedVehicle == null) return NotFound();

                    // Update allowed fields from the view model.
                    parkedVehicle.RegistrationNumber = vm.RegistrationNumber;
                    parkedVehicle.Make = vm.Make;
                    parkedVehicle.Model = vm.Model;
                    parkedVehicle.NumberOfWheels = vm.NumberOfWheels;
                    parkedVehicle.Color = vm.Color;
                    parkedVehicle.Type = vm.Type;


                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParkedVehicleExists(vm.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
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

            // PRIS HÄR?? 
            // TODO: Borde nog ligga i appsettings
            decimal pricePerHour = 20m;
            decimal totalPrice = (decimal)Math.Ceiling(totalParkingTime.TotalHours) * pricePerHour;

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
    }
}
