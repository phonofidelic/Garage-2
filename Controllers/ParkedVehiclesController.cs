using Garage_2.Data;
using Garage_2.Interfaces;
using Garage_2.Models;
using Garage_2.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
        public async Task<IActionResult> Index(
            [FromQuery(Name = "sortBy")] OverviewSortBy? sortBy,
            [FromQuery(Name = "searchString")] string? searchString,
            [FromQuery(Name = "page")] int page = 1)
        {
            // Start with all vehicles and apply smart search
            var query = _searchService.Search(_context.ParkedVehicle, searchString);

            // Execute the query, put the data into overviewmodel and return view
            var now = DateTime.Now;
            IEnumerable<OverviewListItemViewModel> rows = query
                .Select(v => new OverviewListItemViewModel
                {
                    Id = v.Id,
                    RegistrationNumber = v.RegistrationNumber,
                    Type = v.Type,
                    ArrivalTime = v.ArrivalTime,
                    ParkedTime = now - v.ArrivalTime
                });

            //  Apply sorting
            switch (sortBy)
            {
                case OverviewSortBy.RegistrationNumber:
                    rows = rows.OrderBy(v => v.RegistrationNumber);
                    break;

                case OverviewSortBy.ArrivalTime:
                    rows = rows.OrderBy(v => v.ArrivalTime);
                    break;

                case OverviewSortBy.Type:
                    rows = rows.OrderBy(v => v.Type);
                    break;

                case OverviewSortBy.ParkedTime:
                    rows = rows.OrderByDescending(v => v.ArrivalTime);
                    break;

                default:
                    break;
            }

            int rowCount = rows.Count();
            int pageSize = 10;
            var totalPages = (int)Math.Ceiling((double)rowCount / pageSize);

            var currentRows = rows
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

            // Build the ViewModdel
            OverviewViewModel viewModel = new()
            {
                OverviewList = currentRows,
                SortBy = sortBy,
                Count = rowCount,
                SearchString = searchString,
                TotalPages = totalPages,
                CurrentPage = page
            };

            // Return the view
            return View(viewModel);
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

            // DoDo: Move to DetailsViewModel?
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

                // Lägg till fordonet i ParkedVehicle, måste göras först - för att få ett Id
                _context.ParkedVehicle.Add(parkedVehicle);
                await _context.SaveChangesAsync();

                bool parkingSpotAssigned = false;

                // Hämta alla spots från garaget samt hur mycket av varje spot som är använd och hur mycket som är fri.                 
                IQueryable<ParkingSpotWithUnits> spotsWithUsage = _context.ParkingSpots.Select(spot => new ParkingSpotWithUnits
                {
                    Spot = spot,
                    UsedUnits = spot.VehicleSpots.Sum(vs => (int?)vs.UnitsUsed) ?? 0,
                    FreeUnits = spot.CapacityUnits - (spot.VehicleSpots.Sum(vs => (int?)vs.UnitsUsed) ?? 0)  // Antal 'enheter' på p-platsen - för MC
                }
                );

                int unitsNeeded = GetUnitsForVehicle(parkedVehicle.Type);
                ParkingSpotWithUnits? mcSpot, carSpot;
                VehicleSpot vehicleSpot;

                // Försök hämta en eller flera parkeringsplatser eller enhet, beroende på fordonstyp
                if (parkedVehicle.Type == VehicleType.Motorcycle)
                {
                    // Fyll redan använda p-platser först (där det redan står 1-2 MC), därefter fyll tom p-plats  
                    mcSpot = spotsWithUsage.Where(s => s.FreeUnits >= 1).OrderByDescending(s => s.UsedUnits).ThenBy(s => s.Spot.SpotNumber).FirstOrDefault();

                    if (mcSpot != null)
                    {
                        vehicleSpot = new VehicleSpot { ParkedVehicleId = parkedVehicle.Id, ParkingSpotId = mcSpot.Spot.Id, UnitsUsed = unitsNeeded };
                        _context.VehicleSpots.Add(vehicleSpot);
                        parkingSpotAssigned = true;
                    }
                }
                else if (parkedVehicle.Type == VehicleType.Car)
                {
                    carSpot = spotsWithUsage.Where(s => s.UsedUnits == 0 && s.Spot.CapacityUnits >= 3).OrderBy(s => s.Spot.SpotNumber).FirstOrDefault();

                    if (carSpot != null)
                    {
                        vehicleSpot = new VehicleSpot { ParkedVehicleId = parkedVehicle.Id, ParkingSpotId = carSpot.Spot.Id, UnitsUsed = unitsNeeded };
                        _context.VehicleSpots.Add(vehicleSpot);
                        parkingSpotAssigned = true;
                    }
                }
                else if (parkedVehicle.Type == VehicleType.Bus)
                {
                    // Lista alla helt lediga platser i ordning
                    var freeSpots = spotsWithUsage.Where(s => s.UsedUnits == 0).OrderBy(s => s.Spot.SpotNumber).Select(s => s.Spot).ToList();

                    var consecutiveSpots = FindConsecutiveSpots(freeSpots, 2); // Kolla om det finns 2 sammanhängande spots

                    if (consecutiveSpots != null)
                    {
                        foreach (var spot in consecutiveSpots)
                        {
                            _context.VehicleSpots.Add(new VehicleSpot
                            {
                                ParkedVehicleId = parkedVehicle.Id,
                                ParkingSpotId = spot.Id,
                                UnitsUsed = 3                    // Varje VehicleSpot representerar 3 enheter (1 hel p-plats)
                            });
                        }

                        parkingSpotAssigned = true;
                    }


                }
                else if (parkedVehicle.Type == VehicleType.Boat)
                {

                    var freeSpots = spotsWithUsage.Where(s => s.UsedUnits == 0).OrderBy(s => s.Spot.SpotNumber).Select(s => s.Spot).ToList();

                    var consecutiveSpots = FindConsecutiveSpots(freeSpots, 3); // Kolla om det finns 3 sammanhängande spots

                    if (consecutiveSpots != null)
                    {
                        foreach (var spot in consecutiveSpots)
                        {
                            _context.VehicleSpots.Add(new VehicleSpot
                            {
                                ParkedVehicleId = parkedVehicle.Id,
                                ParkingSpotId = spot.Id,
                                UnitsUsed = 3            // Varje VehicleSpot representerar 3 enheter (1 hel p-plats)
                            });
                        }

                        parkingSpotAssigned = true;
                    }
                }


                if (!parkingSpotAssigned) // Om ingen parkeringsplats(-er) kunde tilldelas fordonet - Ta bort det ur ParkedVehicle-tabellen
                {
                    _context.ParkedVehicle.Remove(parkedVehicle);
                    await _context.SaveChangesAsync();

                    SetAlertInTempData(AlertType.warning, "No parking slots available for this vehicle.");
                    return RedirectToAction(nameof(Index));
                }
                else // Annars - spara tilldelad(e) parkeringplats(er) för fordonet
                {
                    await _context.SaveChangesAsync();

                    SetAlertInTempData(AlertType.success, $"Vehicle with RegNum: {viewModel.RegistrationNumber} has been parked.");

                    return RedirectToAction(nameof(Index));
                }
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

            int unitsUsed = _context.VehicleSpots
            .Where(v => v.ParkedVehicleId == id)
            .Sum(v => v.UnitsUsed);

            decimal sizeMultiplier = (decimal)unitsUsed / 3;

            // Calculate Price 
            decimal totalPrice = (decimal)Math.Ceiling(totalParkingTime.TotalHours) * _config.PricePerHour * sizeMultiplier;

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


        // ***************** Hjälpmetoder för parkering av olika fordonstyper *******************
        private int GetUnitsForVehicle(VehicleType type)
        {
            switch (type)
            {
                case VehicleType.Motorcycle:
                    return 1;    // En MC behöver 1 tredjedels plats.
                case VehicleType.Car:
                    return 3;    // Bil 3 tredjedelar, dvs en hel p-plats.
                case VehicleType.Bus:
                    return 6;    // Buss 6 tredjedelar, dvs 2 hela p-platser
                case VehicleType.Boat:
                    return 9;  // Båt 9 tredjedelar, dvs 3 hela p-platser
                default:
                    throw new NotImplementedException();
            }
        }

        private List<ParkingSpot>? FindConsecutiveSpots(List<ParkingSpot> freeSpots, int requiredSpots)
        {
            for (int i = 0; i <= freeSpots.Count - requiredSpots; i++)
            {
                var slice = freeSpots.Skip(i).Take(requiredSpots).ToList();

                bool consecutive = slice.Select(s => s.SpotNumber).SequenceEqual(Enumerable.Range(slice.First().SpotNumber, requiredSpots));
                if (consecutive)
                    return slice;
            }
            return null;
        }


    }

    public class ParkingSpotWithUnits
    {
        public ParkingSpot? Spot { get; set; }
        public int UsedUnits { get; set; }
        public int FreeUnits { get; set; }
    }
}
